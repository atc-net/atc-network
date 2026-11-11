namespace Atc.Network.Discovery;

/// <summary>
/// Sends discovery queries from an ephemeral port and receives the answers, for a fixed duration.
/// </summary>
/// <remarks>
/// <para>
/// The socket is bound to a random port, never to 5353 (mDNS) or 1900 (SSDP), which the operating system
/// often uses already. SSDP devices and mDNS responders answer a query from another port by unicast
/// to that port ("one-shot" or "legacy unicast" queries).
/// </para>
/// <para>
/// A query to a multicast address is sent on every network interface that is up and has an IPv4 address,
/// so devices behind every adapter are reached, not only behind the default one.
/// </para>
/// </remarks>
internal sealed class DiscoverySession : IDisposable
{
    private const int ReceiveBufferLength = 9_000;

    private readonly Socket socket;
    private readonly IPEndPoint target;
    private readonly NetworkInstrumentation instrumentation;
    private readonly CancellationToken cancellationToken;
    private readonly CancellationTokenSource durationCancellationTokenSource;
    private readonly byte[] receiveBuffer = new byte[ReceiveBufferLength];

    public DiscoverySession(
        IPEndPoint target,
        int multicastTimeToLive,
        string component,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        this.target = target;
        this.cancellationToken = cancellationToken;
        instrumentation = new NetworkInstrumentation(component, target.Address.ToString(), target.Port);

        durationCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        durationCancellationTokenSource.CancelAfter(duration);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        if (IsMulticast(target.Address))
        {
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, multicastTimeToLive);
        }
    }

    /// <summary>
    /// Gets the instrumentation, to start the session's activity.
    /// </summary>
    public NetworkInstrumentation Instrumentation
        => instrumentation;

    /// <summary>
    /// Sends a query to the target, on every IPv4 interface when the target is a multicast address.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>A task that completes when the query has been sent.</returns>
    /// <exception cref="SocketException">The query could not be sent on any interface.</exception>
    public async Task SendAsync(byte[] query)
    {
        var interfaceAddresses = IsMulticast(target.Address)
            ? GetMulticastInterfaceAddresses()
            : [];

        if (interfaceAddresses.Count == 0)
        {
            await socket.SendToAsync(query, SocketFlags.None, target, cancellationToken);
            instrumentation.Sent(query.Length);
            return;
        }

        SocketException? lastException = null;
        var sent = false;
        foreach (var interfaceAddress in interfaceAddresses)
        {
            try
            {
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, interfaceAddress.GetAddressBytes());
                await socket.SendToAsync(query, SocketFlags.None, target, cancellationToken);
                instrumentation.Sent(query.Length);
                sent = true;
            }
            catch (SocketException ex)
            {
                // An adapter that can't send multicast (such as a disconnected VPN) does not stop the others.
                lastException = ex;
            }
        }

        if (!sent && lastException is not null)
        {
            throw lastException;
        }
    }

    /// <summary>
    /// Receives the next answer, or returns <see langword="null"/> when the duration is over.
    /// </summary>
    /// <returns>The answer and where it came from, or <see langword="null"/>.</returns>
    /// <exception cref="OperationCanceledException">The caller's cancellation token was cancelled.</exception>
    public async Task<(byte[] Data, IPEndPoint From)?> ReceiveAsync()
    {
        while (true)
        {
            try
            {
                var result = await socket.ReceiveFromAsync(
                    receiveBuffer,
                    SocketFlags.None,
                    new IPEndPoint(IPAddress.Any, 0),
                    durationCancellationTokenSource.Token);

                instrumentation.Received(result.ReceivedBytes);
                var from = IPEndPointHelper.WithoutIPv4Mapping((IPEndPoint)result.RemoteEndPoint);
                return (receiveBuffer.AsSpan(0, result.ReceivedBytes).ToArray(), from);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                // Windows reports an earlier "port unreachable" as a reset on the next receive; keep listening.
            }
        }
    }

    public void Dispose()
    {
        socket.Dispose();
        durationCancellationTokenSource.Dispose();
    }

    private static bool IsMulticast(IPAddress address)
        => address.AddressFamily == AddressFamily.InterNetwork &&
           (address.GetAddressBytes()[0] & 0xF0) == 0xE0;

    private static List<IPAddress> GetMulticastInterfaceAddresses()
        => NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(x => x.OperationalStatus == OperationalStatus.Up &&
                        x.SupportsMulticast &&
                        x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(x => x.GetIPProperties().UnicastAddresses)
            .Select(x => x.Address)
            .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
            .Distinct()
            .ToList();
}