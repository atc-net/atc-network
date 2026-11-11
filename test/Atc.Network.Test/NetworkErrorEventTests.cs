namespace Atc.Network.Test;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class NetworkErrorEventTests
{
    [Fact]
    public async Task TcpClient_DataReceivedHandlerThrows_RaisesErrorWithTheException()
    {
        // Arrange
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            reconnectConfig: new TcpClientReconnectConfig { Enable = false });

        var handlerException = new InvalidOperationException("handler bug");
        tcpClient.DataReceived += _ => throw handlerException;

        var errors = new ConcurrentQueue<NetworkErrorEventArgs>();
        tcpClient.ErrorOccurred += (_, e) => errors.Enqueue(e);

        var acceptTask = listener.AcceptTcpClientAsync();
        Assert.True(await tcpClient.Connect());
        using var serverSide = await acceptTask;

        // Act
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes("x"));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => !errors.IsEmpty), "Error should be raised.");
        var error = Assert.Single(errors);
        Assert.Equal(NetworkErrorKind.EventHandler, error.Kind);
        Assert.Equal("DataReceived", error.EventName);
        Assert.Same(handlerException, error.Exception);

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task TcpClient_ErrorHandlerThrows_KeepsReceiving()
    {
        // Arrange
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var logger = new CapturingLogger();
        using var tcpClient = new TcpClient(
            logger,
            IPAddress.Loopback,
            port,
            reconnectConfig: new TcpClientReconnectConfig { Enable = false });

        var received = 0;
        tcpClient.DataReceived += _ =>
        {
            Interlocked.Increment(ref received);
            throw new InvalidOperationException("handler bug");
        };
        tcpClient.ErrorOccurred += (_, _) => throw new InvalidOperationException("error handler bug");

        var acceptTask = listener.AcceptTcpClientAsync();
        Assert.True(await tcpClient.Connect());
        using var serverSide = await acceptTask;

        // Act
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes("a"));
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref received) == 1), "The first message should arrive.");
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes("b"));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref received) == 2), "The client should keep receiving.");
        Assert.True(tcpClient.IsConnected);
        Assert.Contains(
            logger.Entries,
            x => x.EventId.Id == LoggingEventIdConstants.TcpClient.EventHandlerError &&
                 x.Message.Contains("ErrorOccurred", StringComparison.Ordinal));

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task UdpClient_NothingListensOnThePort_RaisesReceiveError()
    {
        // Arrange - the port is free, so the datagram is answered with an ICMP "port unreachable",
        // which the next receive on the connected socket reports as an error.
        var port = GetFreeUdpPort();
        using var udpClient = new UdpClient(NullLogger.Instance, IPAddress.Loopback, port);

        var errors = new ConcurrentQueue<NetworkErrorEventArgs>();
        udpClient.ErrorOccurred += (_, e) => errors.Enqueue(e);

        Assert.True(await udpClient.Connect());

        // Act
        await udpClient.Send("ping", CancellationToken.None);

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => errors.Any(x => x.Kind == NetworkErrorKind.Receive)),
            "A receive error should be raised.");

        await udpClient.Disconnect();
    }

    [Fact]
    public async Task UdpClient_DataReceivedHandlerThrows_RaisesError()
    {
        // Arrange
        var port = GetFreeUdpPort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        // UdpServer.Send logs socket errors instead of throwing, so this async handler does not throw.
        udpServer.DatagramReceived += async (_, e) => await udpServer.Send(e.RemoteEndPoint, "pong", CancellationToken.None);
        await udpServer.StartAsync(CancellationToken.None);

        using var udpClient = new UdpClient(NullLogger.Instance, IPAddress.Loopback, port);
        udpClient.DataReceived += _ => throw new InvalidOperationException("handler bug");

        var errors = new ConcurrentQueue<NetworkErrorEventArgs>();
        udpClient.ErrorOccurred += (_, e) => errors.Enqueue(e);

        Assert.True(await udpClient.Connect());

        // Act
        await udpClient.Send("ping", CancellationToken.None);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => !errors.IsEmpty), "Error should be raised.");
        var error = Assert.Single(errors);
        Assert.Equal(NetworkErrorKind.EventHandler, error.Kind);
        Assert.Equal("DataReceived", error.EventName);

        await udpClient.Disconnect();
        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_ClientDataReceivedHandlerThrows_RaisesError()
    {
        // Arrange
        var port = GetFreeTcpPort();
        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.Loopback, port);
        tcpServer.ClientDataReceived += (_, _) => throw new InvalidOperationException("handler bug");

        var errors = new ConcurrentQueue<NetworkErrorEventArgs>();
        tcpServer.ErrorOccurred += (_, e) => errors.Enqueue(e);
        await tcpServer.StartAsync(CancellationToken.None);

        // Act
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes("abc"));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => !errors.IsEmpty), "Error should be raised.");
        var error = Assert.Single(errors);
        Assert.Equal(NetworkErrorKind.EventHandler, error.Kind);
        Assert.Equal("ClientDataReceived", error.EventName);

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_TlsHandshakeFails_RaisesClientConnectionError()
    {
        // Arrange
        using var certificate = CertificateHelper.CreateSelfSignedCertificate();
        var port = GetFreeTcpPort();
        using var tcpServer = new TcpServer(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpServerConfig { SslOptions = new SslServerAuthenticationOptions { ServerCertificate = certificate } });

        var errors = new ConcurrentQueue<NetworkErrorEventArgs>();
        tcpServer.ErrorOccurred += (_, e) => errors.Enqueue(e);
        await tcpServer.StartAsync(CancellationToken.None);

        // Act - a plain TCP client sends something that is not a TLS handshake.
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes("not a TLS handshake\r\n"));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => !errors.IsEmpty), "Error should be raised.");
        Assert.Equal(NetworkErrorKind.ClientConnection, Assert.Single(errors).Kind);

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UdpServer_DatagramReceivedHandlerThrows_RaisesError()
    {
        // Arrange
        var port = GetFreeUdpPort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);
        udpServer.DatagramReceived += (_, _) => throw new InvalidOperationException("handler bug");

        var errors = new ConcurrentQueue<NetworkErrorEventArgs>();
        udpServer.ErrorOccurred += (_, e) => errors.Enqueue(e);
        await udpServer.StartAsync(CancellationToken.None);

        // Act
        using var client = new System.Net.Sockets.UdpClient();
        await client.SendAsync(Encoding.ASCII.GetBytes("x"), new IPEndPoint(IPAddress.Loopback, port));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => !errors.IsEmpty), "Error should be raised.");
        var error = Assert.Single(errors);
        Assert.Equal(NetworkErrorKind.EventHandler, error.Kind);
        Assert.Equal("DatagramReceived", error.EventName);

        await udpServer.StopAsync(CancellationToken.None);
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static int GetFreeUdpPort()
    {
        using var probe = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint).Port;
    }
}