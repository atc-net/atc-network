namespace Atc.Network.Test;

/// <summary>
/// IPv6 and dual-mode (IPv4 and IPv6 on one socket). Skipped where the machine has no IPv6.
/// </summary>
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class IPv6Tests
{
    [Fact]
    public async Task UdpClientAndUdpServer_OverIPv6_ExchangeDatagrams()
    {
        // Arrange
        SkipWithoutIPv6();
        var port = GetFreeUdpPort(IPAddress.IPv6Loopback);
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.IPv6Loopback, port);

        // UdpServer.Send logs socket errors instead of throwing, so this async handler does not throw.
        udpServer.DatagramReceived += async (_, e) => await udpServer.Send(e.RemoteEndPoint, "pong", CancellationToken.None);
        await udpServer.StartAsync(CancellationToken.None);

        using var udpClient = new UdpClient(NullLogger.Instance, IPAddress.IPv6Loopback, port);
        var received = new ConcurrentQueue<string>();
        udpClient.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        // Act
        Assert.True(await udpClient.Connect());
        await udpClient.Send("ping", CancellationToken.None);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("pong", StringComparer.Ordinal)), "The reply should arrive over IPv6.");

        await udpClient.Disconnect();
        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UdpServer_DualMode_ReceivesFromIPv4AndIPv6()
    {
        // Arrange
        SkipWithoutIPv6();
        var port = GetFreeUdpPort(IPAddress.IPv6Any);
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.IPv6Any, port, new UdpServerConfig { DualMode = true });

        var senders = new ConcurrentQueue<IPAddress>();
        udpServer.DatagramReceived += (_, e) => senders.Enqueue(e.RemoteEndPoint.Address);
        await udpServer.StartAsync(CancellationToken.None);

        // Act
        using (var ipv4Client = new System.Net.Sockets.UdpClient(System.Net.Sockets.AddressFamily.InterNetwork))
        {
            await ipv4Client.SendAsync(Encoding.ASCII.GetBytes("v4"), new IPEndPoint(IPAddress.Loopback, port));
        }

        using (var ipv6Client = new System.Net.Sockets.UdpClient(System.Net.Sockets.AddressFamily.InterNetworkV6))
        {
            await ipv6Client.SendAsync(Encoding.ASCII.GetBytes("v6"), new IPEndPoint(IPAddress.IPv6Loopback, port));
        }

        // Assert - the IPv4 sender is reported as an IPv4 address, not as ::ffff:127.0.0.1.
        Assert.True(await WaitHelper.WaitUntil(() => senders.Count == 2), $"Expected 2 datagrams, got {senders.Count}.");
        Assert.Contains(IPAddress.Loopback, senders);
        Assert.Contains(IPAddress.IPv6Loopback, senders);

        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpClientAndTcpServer_OverIPv6_ExchangeMessages()
    {
        // Arrange
        SkipWithoutIPv6();
        var port = GetFreeTcpPort(IPAddress.IPv6Loopback);
        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.IPv6Loopback, port);

        // Send logs and returns false on connection errors, so this async handler does not throw.
        tcpServer.ClientDataReceived += async (_, e) => await tcpServer.Send(e.ClientId, "pong", CancellationToken.None);
        await tcpServer.StartAsync(CancellationToken.None);

        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            IPAddress.IPv6Loopback,
            port,
            reconnectConfig: new TcpClientReconnectConfig { Enable = false });
        var received = new ConcurrentQueue<string>();
        tcpClient.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        // Act
        Assert.True(await tcpClient.Connect());
        await tcpClient.Send("ping");

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("pong", StringComparer.Ordinal)), "The reply should arrive over IPv6.");

        await tcpClient.Disconnect();
        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_DualMode_AcceptsIPv4AndIPv6Clients()
    {
        // Arrange
        SkipWithoutIPv6();
        var port = GetFreeTcpPort(IPAddress.IPv6Any);
        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.IPv6Any, port, new TcpServerConfig { DualMode = true });

        var clients = new ConcurrentQueue<IPAddress>();
        tcpServer.ClientConnected += (_, e) => clients.Enqueue(e.RemoteEndPoint.Address);
        await tcpServer.StartAsync(CancellationToken.None);

        // Act
        using var ipv4Client = new System.Net.Sockets.TcpClient(System.Net.Sockets.AddressFamily.InterNetwork);
        await ipv4Client.ConnectAsync(IPAddress.Loopback, port);
        using var ipv6Client = new System.Net.Sockets.TcpClient(System.Net.Sockets.AddressFamily.InterNetworkV6);
        await ipv6Client.ConnectAsync(IPAddress.IPv6Loopback, port);

        // Assert - the IPv4 client is reported as an IPv4 address, not as ::ffff:127.0.0.1.
        Assert.True(await WaitHelper.WaitUntil(() => clients.Count == 2), $"Expected 2 clients, got {clients.Count}.");
        Assert.Contains(IPAddress.Loopback, clients);
        Assert.Contains(IPAddress.IPv6Loopback, clients);

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void TcpServer_DualModeWithIPv4Address_Throws()
        => Assert.Throws<ArgumentException>(() => new TcpServer(NullLogger.Instance, IPAddress.Loopback, 0, new TcpServerConfig { DualMode = true }));

    [Fact]
    public void UdpServer_DualModeWithIPv4Address_Throws()
        => Assert.Throws<ArgumentException>(() => new UdpServer(NullLogger.Instance, IPAddress.Loopback, 0, new UdpServerConfig { DualMode = true }));

    [Fact]
    public async Task IPScanner_ScanSingleIPv6Address_FindsTheOpenPort()
    {
        // Arrange
        SkipWithoutIPv6();
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.IPv6Loopback, 0);
        listener.Start();
        var port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port;

        using var ipScanner = new IPScanner(
            NullLogger.Instance,
            new IPScannerConfig
            {
                IcmpPing = false,
                ResolveHostName = false,
                ResolveMacAddress = false,
                ResolveVendorFromMacAddress = false,
                TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
                PortNumbers = [port],
            });

        // Act
        var results = await ipScanner.Scan(IPAddress.IPv6Loopback, CancellationToken.None);

        // Assert
        Assert.Null(results.ErrorMessage);
        var scanResult = Assert.Single(results.CollectedResults);
        Assert.Equal(IPAddress.IPv6Loopback, scanResult.IPAddress);
        Assert.Contains(port, scanResult.OpenPortNumbers);
    }

    [Fact]
    public async Task IPScanner_ScanIPv6Range_ReturnsAnErrorMessage()
    {
        // Arrange
        using var ipScanner = new IPScanner(NullLogger.Instance);

        // Act
        var results = await ipScanner.ScanRange(IPAddress.Parse("2001:db8::1"), IPAddress.Parse("2001:db8::10"), CancellationToken.None);

        // Assert
        Assert.NotNull(results.ErrorMessage);
        Assert.Contains("IPv6", results.ErrorMessage, StringComparison.Ordinal);
        Assert.Empty(results.CollectedResults);
    }

    [Fact]
    [SuppressMessage("Reliability", "CA2025:Ensure tasks using IDisposable instances complete before the instances are disposed", Justification = "serverTask is awaited before the listener is disposed.")]
    public async Task IPPortScan_CanConnectWithHttp_OverIPv6_ReturnsTrue()
    {
        // Arrange
        SkipWithoutIPv6();
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.IPv6Loopback, 0);
        listener.Start();
        var port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port;
        var serverTask = AnswerFirstRequestWith401(listener);

        using var ipPortScan = new IPPortScan(IPAddress.IPv6Loopback, timeoutInMs: 2000);

        // Act
        var actual = await ipPortScan.CanConnectWithHttp(port, CancellationToken.None);
        await serverTask;

        // Assert - the URL needs brackets around an IPv6 address: http://[::1]:port.
        Assert.True(actual);
    }

    private static void SkipWithoutIPv6()
        => Assert.SkipUnless(System.Net.Sockets.Socket.OSSupportsIPv6, "IPv6 is not available on this machine.");

    private static async Task AnswerFirstRequestWith401(
        System.Net.Sockets.TcpListener listener)
    {
        try
        {
            using var serverSide = await listener.AcceptTcpClientAsync().WaitAsync(TimeSpan.FromSeconds(5));
            var stream = serverSide.GetStream();
            var buffer = new byte[4096];
            var request = new StringBuilder();
            while (!request.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                var read = await stream.ReadAsync(buffer);
                if (read == 0)
                {
                    return;
                }

                request.Append(Encoding.ASCII.GetString(buffer, 0, read));
            }

            await stream.WriteAsync(Encoding.ASCII.GetBytes("HTTP/1.1 401 Unauthorized\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"));
        }
        catch (Exception ex) when (ex is IOException or TimeoutException)
        {
            // No request arrived; the assert on the client's result reports it.
        }
    }

    private static int GetFreeTcpPort(IPAddress address)
    {
        using var listener = new System.Net.Sockets.TcpListener(address, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static int GetFreeUdpPort(IPAddress address)
    {
        using var probe = new System.Net.Sockets.UdpClient(new IPEndPoint(address, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint).Port;
    }
}