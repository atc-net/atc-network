namespace Atc.Network.Helpers;

/// <summary>
/// Asks a host for its NetBIOS names and MAC address with a node status query (RFC 1002, as <c>nbtstat -A</c> does).
/// </summary>
/// <remarks>
/// Windows computers with NetBIOS over TCP/IP, and Samba servers (NAS boxes, Linux file servers), answer on UDP port 137
/// with their computer name and workgroup, even when DNS knows nothing about them. Most other devices don't answer.
/// </remarks>
public static class NetBiosHelper
{
    /// <summary>
    /// The NetBIOS name service port.
    /// </summary>
    public const int DefaultPort = 137;

    /// <summary>
    /// The default time to wait for the answer, in milliseconds.
    /// </summary>
    public const int DefaultTimeoutInMs = 1_000;

    private const int HeaderLength = 12;
    private const int EncodedNameLength = 32;
    private const ushort NodeStatusType = 0x0021;
    private const ushort ClassInternet = 0x0001;
    private const int ResponseFlag = 0x80;
    private const int NameEntryLength = 18;
    private const int MacAddressLength = 6;
    private const int ReceiveBufferLength = 1_024;

    /// <summary>
    /// Asks a host for its NetBIOS names and MAC address.
    /// </summary>
    /// <param name="ipAddress">The host's IPv4 address.</param>
    /// <param name="timeout">How long to wait for the answer. The default is 1 second.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The host's names and MAC address, or <see langword="null"/> when it does not answer in time, which is normal
    /// for hosts without NetBIOS.
    /// </returns>
    /// <exception cref="OperationCanceledException">The cancellation token was cancelled.</exception>
    public static Task<NetBiosNodeStatus?> GetNodeStatus(
        IPAddress ipAddress,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        return GetNodeStatus(
            new IPEndPoint(ipAddress, DefaultPort),
            timeout ?? TimeSpan.FromMilliseconds(DefaultTimeoutInMs),
            cancellationToken);
    }

    /// <summary>
    /// Sends the node status query to an endpoint, the host's port 137 or (in tests) a fake responder.
    /// </summary>
    /// <param name="endPoint">Where to send the query.</param>
    /// <param name="timeout">How long to wait for the answer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The node status, or <see langword="null"/> when there is no answer in time.</returns>
    internal static async Task<NetBiosNodeStatus?> GetNodeStatus(
        IPEndPoint endPoint,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var instrumentation = new NetworkInstrumentation("netbios_client", endPoint.Address.ToString(), endPoint.Port);
        using var activity = instrumentation.StartActivity("NetBios.GetNodeStatus", ActivityKind.Client);

        using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellationTokenSource.CancelAfter(timeout);

        using var socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        // An unpredictable transaction id makes a spoofed answer harder.
        var transactionId = (ushort)System.Security.Cryptography.RandomNumberGenerator.GetInt32(ushort.MaxValue + 1);
        var request = CreateRequest(transactionId);

        try
        {
            await socket.ConnectAsync(endPoint, timeoutCancellationTokenSource.Token);
            await socket.SendAsync(request, SocketFlags.None, timeoutCancellationTokenSource.Token);
            instrumentation.Sent(request.Length);

            var buffer = new byte[ReceiveBufferLength];
            while (true)
            {
                var received = await socket.ReceiveAsync(buffer, SocketFlags.None, timeoutCancellationTokenSource.Token);
                instrumentation.Received(received);

                var nodeStatus = ParseResponse(buffer.AsSpan(0, received), endPoint.Address, transactionId);
                if (nodeStatus is not null)
                {
                    return nodeStatus;
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (SocketException ex) when (ex.SocketErrorCode is SocketError.ConnectionReset or SocketError.ConnectionRefused)
        {
            // The host's port 137 is closed ("port unreachable").
            return null;
        }
    }

    /// <summary>
    /// Creates a node status request for the wildcard name "*".
    /// </summary>
    /// <param name="transactionId">The transaction id, which the answer repeats.</param>
    /// <returns>The 50-byte request.</returns>
    internal static byte[] CreateRequest(ushort transactionId)
    {
        var request = new byte[HeaderLength + 1 + EncodedNameLength + 1 + 4];
        BinaryPrimitives.WriteUInt16BigEndian(request, transactionId);
        BinaryPrimitives.WriteUInt16BigEndian(request.AsSpan(4), 1);

        // The name "*" padded with zero bytes to 16 bytes, first-level encoded: each half-byte becomes 'A' + its value.
        request[HeaderLength] = EncodedNameLength;
        var name = new byte[16];
        name[0] = (byte)'*';
        for (var i = 0; i < name.Length; i++)
        {
            request[HeaderLength + 1 + (i * 2)] = (byte)('A' + (name[i] >> 4));
            request[HeaderLength + 2 + (i * 2)] = (byte)('A' + (name[i] & 0x0F));
        }

        var offset = HeaderLength + 1 + EncodedNameLength + 1;
        BinaryPrimitives.WriteUInt16BigEndian(request.AsSpan(offset), NodeStatusType);
        BinaryPrimitives.WriteUInt16BigEndian(request.AsSpan(offset + 2), ClassInternet);
        return request;
    }

    /// <summary>
    /// Reads a node status answer. Returns <see langword="null"/> for anything else: another transaction id,
    /// a query, or a malformed or truncated packet.
    /// </summary>
    /// <param name="response">The datagram.</param>
    /// <param name="ipAddress">The address that answered.</param>
    /// <param name="expectedTransactionId">The transaction id of the request.</param>
    /// <returns>The node status, or <see langword="null"/>.</returns>
    internal static NetBiosNodeStatus? ParseResponse(
        ReadOnlySpan<byte> response,
        IPAddress ipAddress,
        ushort expectedTransactionId)
    {
        if (response.Length < HeaderLength ||
            BinaryPrimitives.ReadUInt16BigEndian(response) != expectedTransactionId ||
            (response[2] & ResponseFlag) == 0 ||
            BinaryPrimitives.ReadUInt16BigEndian(response[6..]) == 0)
        {
            return null;
        }

        // Skip the questions (some hosts repeat the question), then the answer's name, type, class and TTL.
        var offset = HeaderLength;
        var questionCount = BinaryPrimitives.ReadUInt16BigEndian(response[4..]);
        for (var i = 0; i < questionCount; i++)
        {
            if (!TrySkipName(response, ref offset) || !TrySkip(response, ref offset, 4))
            {
                return null;
            }
        }

        if (!TrySkipName(response, ref offset) ||
            offset + 10 > response.Length ||
            BinaryPrimitives.ReadUInt16BigEndian(response[offset..]) != NodeStatusType)
        {
            return null;
        }

        var dataLength = BinaryPrimitives.ReadUInt16BigEndian(response[(offset + 8)..]);
        offset += 10;
        if (offset + dataLength > response.Length)
        {
            return null;
        }

        return ReadNodeStatus(response.Slice(offset, dataLength), ipAddress);
    }

    private static NetBiosNodeStatus? ReadNodeStatus(
        ReadOnlySpan<byte> data,
        IPAddress ipAddress)
    {
        if (data.Length < 1)
        {
            return null;
        }

        var nameCount = data[0];
        var macOffset = 1 + (nameCount * NameEntryLength);
        if (macOffset + MacAddressLength > data.Length)
        {
            return null;
        }

        var names = new List<NetBiosName>(nameCount);
        for (var i = 0; i < nameCount; i++)
        {
            var entry = data.Slice(1 + (i * NameEntryLength), NameEntryLength);
            names.Add(new NetBiosName(
                Encoding.Latin1.GetString(entry[..15]).TrimEnd(' ', '\0'),
                entry[15],
                IsGroup: (entry[16] & 0x80) != 0));
        }

        var macBytes = data.Slice(macOffset, MacAddressLength);
        var macAddress = macBytes.ContainsAnyExcept((byte)0)
            ? Convert.ToHexString(macBytes)
                .Chunk(2)
                .Select(x => new string(x))
                .Aggregate((current, next) => current + "-" + next)
            : null;

        return new NetBiosNodeStatus(ipAddress, names, macAddress);
    }

    /// <summary>
    /// Skips a name: a compression pointer (2 bytes) or labels up to the zero byte.
    /// </summary>
    private static bool TrySkipName(
        ReadOnlySpan<byte> data,
        ref int offset)
    {
        while (offset < data.Length)
        {
            var length = data[offset];
            if ((length & 0xC0) == 0xC0)
            {
                return TrySkip(data, ref offset, 2);
            }

            if (length > 63)
            {
                return false;
            }

            offset += length + 1;
            if (length == 0)
            {
                return offset <= data.Length;
            }
        }

        return false;
    }

    private static bool TrySkip(
        ReadOnlySpan<byte> data,
        ref int offset,
        int count)
    {
        if (offset + count > data.Length)
        {
            return false;
        }

        offset += count;
        return true;
    }
}