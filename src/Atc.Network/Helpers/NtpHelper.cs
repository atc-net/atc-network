namespace Atc.Network.Helpers;

/// <summary>
/// Gets the time from an NTP server with SNTP (RFC 4330), and how far the local clock is from it.
/// </summary>
/// <remarks>
/// One request is sent per call, as a UDP datagram to port 123. The result has the clock offset and
/// the round-trip delay, from the four NTP timestamps. NTP servers serve UTC; convert it with
/// <see cref="TimeZoneInfo.ConvertTimeFromUtc(DateTime, TimeZoneInfo)"/> for a time zone.
/// Public servers limit how often a client may ask; don't call this more than about once a minute per server.
/// </remarks>
public static class NtpHelper
{
    /// <summary>
    /// The default server: the NTP Pool, which picks a nearby public server.
    /// </summary>
    public const string DefaultHost = "pool.ntp.org";

    /// <summary>
    /// The NTP port.
    /// </summary>
    public const int DefaultPort = 123;

    /// <summary>
    /// The default time to wait for the reply, in milliseconds.
    /// </summary>
    public const int DefaultTimeoutInMs = 5_000;

    private const int PacketLength = 48;
    private const int ReceiveBufferLength = 1024;
    private const byte ClientRequestHeader = 0x23; // Leap indicator 0, version 4, mode 3 (client).
    private const int ServerMode = 4;
    private const int LeapIndicatorNotSynchronized = 3;
    private const uint EraHighBit = 0x8000_0000;

    private static readonly DateTime NtpEra0 = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime NtpEra1 = new(2036, 2, 7, 6, 28, 16, DateTimeKind.Utc);

    /// <summary>
    /// Gets the time from an NTP server by host name or IP address.
    /// </summary>
    /// <param name="host">The server's host name or IP address. The default is <see cref="DefaultHost"/>.</param>
    /// <param name="timeout">How long to wait for the reply. The default is 5 seconds. Resolving the host name is not included.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The server's time and the local clock's offset from it.</returns>
    /// <exception cref="TimeoutException">The server did not answer in time.</exception>
    /// <exception cref="InvalidDataException">The server refused the request, or its clock is not synchronized.</exception>
    /// <exception cref="SocketException">The host name could not be resolved, or the request could not be sent.</exception>
    public static async Task<NtpResult> GetTime(
        string host = DefaultHost,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        if (!IPAddress.TryParse(host, out var ipAddress))
        {
            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);

            // Prefer IPv4: many networks resolve IPv6 addresses without being able to reach them.
            ipAddress = addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork) ??
                        addresses.FirstOrDefault() ??
                        throw new SocketException((int)SocketError.HostNotFound);
        }

        return await GetTime(new IPEndPoint(ipAddress, DefaultPort), timeout, cancellationToken);
    }

    /// <summary>
    /// Gets the time from an NTP server at an endpoint.
    /// </summary>
    /// <param name="endPoint">The server's address and port.</param>
    /// <param name="timeout">How long to wait for the reply. The default is 5 seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The server's time and the local clock's offset from it.</returns>
    /// <exception cref="TimeoutException">The server did not answer in time.</exception>
    /// <exception cref="InvalidDataException">The server refused the request, or its clock is not synchronized.</exception>
    /// <exception cref="SocketException">The request could not be sent, or the server's port is closed.</exception>
    public static async Task<NtpResult> GetTime(
        IPEndPoint endPoint,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endPoint);

        var timeoutValue = timeout ?? TimeSpan.FromMilliseconds(DefaultTimeoutInMs);
        var instrumentation = new NetworkInstrumentation("ntp_client", endPoint.Address.ToString(), endPoint.Port);
        using var activity = instrumentation.StartActivity("Ntp.GetTime", ActivityKind.Client);

        using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellationTokenSource.CancelAfter(timeoutValue);

        try
        {
            return await Exchange(endPoint, instrumentation, timeoutCancellationTokenSource.Token);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Timeout");
            throw new TimeoutException($"The NTP server {endPoint} did not answer within {timeoutValue.TotalMilliseconds} ms.", ex);
        }
        catch (Exception ex) when (ex is SocketException or InvalidDataException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Converts a UTC time to a 64-bit NTP timestamp: seconds since 1900 in the high 32 bits,
    /// the fraction of a second in the low 32 bits. Times from 2036-02-07T06:28:16Z wrap around to 0.
    /// </summary>
    /// <param name="utcTime">The UTC time.</param>
    /// <returns>The NTP timestamp.</returns>
    internal static ulong ToNtpTimestamp(DateTime utcTime)
    {
        var ticks = utcTime.Ticks - NtpEra0.Ticks;
        var seconds = (ulong)(ticks / TimeSpan.TicksPerSecond) & uint.MaxValue;
        var remainder = (ulong)(ticks % TimeSpan.TicksPerSecond);
        var fraction = ((remainder << 32) + (TimeSpan.TicksPerSecond / 2)) / TimeSpan.TicksPerSecond;
        return (seconds << 32) | fraction;
    }

    /// <summary>
    /// Converts a 64-bit NTP timestamp to a UTC time. Seconds with the highest bit clear are read as
    /// after 2036-02-07T06:28:16Z (RFC 4330, section 3), which covers 1968 to 2104.
    /// </summary>
    /// <param name="timestamp">The NTP timestamp.</param>
    /// <returns>The UTC time.</returns>
    internal static DateTime FromNtpTimestamp(ulong timestamp)
    {
        var seconds = timestamp >> 32;
        var fraction = timestamp & uint.MaxValue;
        var era = (seconds & EraHighBit) != 0 ? NtpEra0 : NtpEra1;
        var fractionTicks = ((fraction * TimeSpan.TicksPerSecond) + EraHighBit) >> 32;
        return era.AddTicks(((long)seconds * TimeSpan.TicksPerSecond) + (long)fractionTicks);
    }

    /// <summary>
    /// Creates an SNTP version 4 client request with the transmit time, which the server echoes back.
    /// </summary>
    /// <param name="transmitTime">The UTC time the request is sent.</param>
    /// <returns>The 48-byte request.</returns>
    internal static byte[] CreateRequest(DateTime transmitTime)
    {
        var request = new byte[PacketLength];
        request[0] = ClientRequestHeader;
        BinaryPrimitives.WriteUInt64BigEndian(request.AsSpan(40), ToNtpTimestamp(transmitTime));
        return request;
    }

    /// <summary>
    /// Reads a server reply. Returns <see langword="null"/> for a datagram that is not the reply to our request:
    /// too short, or not echoing our transmit timestamp (an old or spoofed reply).
    /// </summary>
    /// <param name="reply">The received datagram.</param>
    /// <param name="expectedOriginate">The transmit timestamp of our request.</param>
    /// <returns>The reply, or <see langword="null"/> when the datagram is not the reply to our request.</returns>
    /// <exception cref="InvalidDataException">The reply is ours but can't be used.</exception>
    internal static NtpReply? ParseReply(
        ReadOnlySpan<byte> reply,
        ulong expectedOriginate)
    {
        if (reply.Length < PacketLength ||
            BinaryPrimitives.ReadUInt64BigEndian(reply[24..]) != expectedOriginate)
        {
            return null;
        }

        var leapIndicator = reply[0] >> 6;
        var mode = reply[0] & 0x07;
        var stratum = reply[1];

        if (mode != ServerMode)
        {
            throw new InvalidDataException($"The NTP reply has mode {mode}, not {ServerMode} (server).");
        }

        if (stratum == 0)
        {
            var kissCode = Encoding.ASCII.GetString(reply.Slice(12, 4)).TrimEnd('\0');
            throw new InvalidDataException($"The NTP server refused the request (kiss code '{kissCode}').");
        }

        if (leapIndicator == LeapIndicatorNotSynchronized)
        {
            throw new InvalidDataException("The NTP server's clock is not synchronized.");
        }

        var transmitTimestamp = BinaryPrimitives.ReadUInt64BigEndian(reply[40..]);
        if (transmitTimestamp == 0)
        {
            throw new InvalidDataException("The NTP reply has no transmit timestamp.");
        }

        return new NtpReply(
            stratum,
            FromNtpTimestamp(BinaryPrimitives.ReadUInt64BigEndian(reply[32..])),
            FromNtpTimestamp(transmitTimestamp));
    }

    /// <summary>
    /// Calculates the result from the four NTP timestamps: t1 request sent (local), t2 request received (server),
    /// t3 reply sent (server), t4 reply received (local).
    /// </summary>
    /// <param name="server">The server that answered.</param>
    /// <param name="stratum">The server's stratum.</param>
    /// <param name="t1">The time the request was sent, by the local clock.</param>
    /// <param name="t2">The time the request arrived, by the server's clock.</param>
    /// <param name="t3">The time the reply was sent, by the server's clock.</param>
    /// <param name="t4">The time the reply arrived, by the local clock.</param>
    /// <returns>The result.</returns>
    internal static NtpResult CalculateResult(
        IPEndPoint server,
        int stratum,
        DateTime t1,
        DateTime t2,
        DateTime t3,
        DateTime t4)
    {
        var offset = TimeSpan.FromTicks(((t2 - t1) + (t3 - t4)).Ticks / 2);
        var roundTripDelay = (t4 - t1) - (t3 - t2);
        return new NtpResult(
            server,
            stratum,
            offset,
            roundTripDelay,
            DateTime.SpecifyKind(t4 + offset, DateTimeKind.Utc));
    }

    private static async Task<NtpResult> Exchange(
        IPEndPoint endPoint,
        NetworkInstrumentation instrumentation,
        CancellationToken cancellationToken)
    {
        using var socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        await socket.ConnectAsync(endPoint, cancellationToken);

        // t4 is measured with a Stopwatch from t1, so a clock change during the exchange can't distort the delay.
        var t1 = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var request = CreateRequest(t1);
        var originate = ToNtpTimestamp(t1);

        await socket.SendAsync(request, SocketFlags.None, cancellationToken);
        instrumentation.Sent(request.Length);

        var buffer = new byte[ReceiveBufferLength];
        while (true)
        {
            var received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
            var t4 = t1 + stopwatch.Elapsed;
            instrumentation.Received(received);

            var reply = ParseReply(buffer.AsSpan(0, received), originate);
            if (reply is not null)
            {
                return CalculateResult(endPoint, reply.Stratum, t1, reply.ReceiveTime, reply.TransmitTime, t4);
            }
        }
    }

    /// <summary>
    /// The parts of a server reply that the result needs.
    /// </summary>
    internal sealed record NtpReply(
        int Stratum,
        DateTime ReceiveTime,
        DateTime TransmitTime);
}