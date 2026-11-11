namespace Atc.Network.Test;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class NetworkTelemetryTests : IDisposable
{
    private readonly TelemetryCollector telemetry = new();

    [Fact]
    public async Task TcpClient_ConnectSendReceiveDisconnect_RecordsConnectionTrafficAndActivities()
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

        var received = 0;
        tcpClient.DataReceived += _ => Interlocked.Increment(ref received);

        var acceptTask = listener.AcceptTcpClientAsync();

        // Act
        Assert.True(await tcpClient.Connect());
        using var serverSide = await acceptTask;
        await tcpClient.Send("hello");
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes("world!"));
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref received) == 1), "The client should receive the data.");
        await tcpClient.Disconnect();

        // Assert
        Assert.Equal([1, -1], telemetry.Values(NetworkTelemetry.ActiveConnectionsInstrumentName, "tcp_client", port));
        Assert.Equal(5, telemetry.Sum(NetworkTelemetry.BytesSentInstrumentName, "tcp_client", port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesSentInstrumentName, "tcp_client", port));
        Assert.Equal(6, telemetry.Sum(NetworkTelemetry.BytesReceivedInstrumentName, "tcp_client", port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesReceivedInstrumentName, "tcp_client", port));

        var connectActivity = Assert.Single(telemetry.Activities("TcpClient.Connect", port));
        Assert.Equal(ActivityStatusCode.Unset, connectActivity.Status);
        Assert.Equal("tcp_client", connectActivity.GetTagItem(NetworkTelemetry.ComponentTag));
        Assert.Equal("127.0.0.1", connectActivity.GetTagItem(NetworkTelemetry.ServerAddressTag));
        Assert.Single(telemetry.Activities("TcpClient.Send", port));
    }

    [Fact]
    public async Task TcpClient_ReceiveLoop_DoesNotRunInsideTheConnectActivity()
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

        var handlerCalled = false;
        Activity? activityInHandler = null;
        tcpClient.DataReceived += _ =>
        {
            activityInHandler = Activity.Current;
            Volatile.Write(ref handlerCalled, value: true);
        };

        var acceptTask = listener.AcceptTcpClientAsync();
        Assert.True(await tcpClient.Connect());
        using var serverSide = await acceptTask;

        // Act
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes("x"));

        // Assert - otherwise every log and span from the receive loop belongs to the long-finished connect span.
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref handlerCalled)), "The client should receive the data.");
        Assert.Null(activityInHandler);

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task UdpClient_ReceiveLoop_DoesNotRunInsideTheConnectActivity()
    {
        // Arrange
        var port = GetFreeUdpPort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        // UdpServer.Send logs socket errors instead of throwing, so this async handler does not throw.
        udpServer.DatagramReceived += async (_, e) => await udpServer.Send(e.RemoteEndPoint, "pong", CancellationToken.None);
        await udpServer.StartAsync(CancellationToken.None);

        using var udpClient = new UdpClient(NullLogger.Instance, IPAddress.Loopback, port);
        var handlerCalled = false;
        Activity? activityInHandler = null;
        udpClient.DataReceived += _ =>
        {
            activityInHandler = Activity.Current;
            Volatile.Write(ref handlerCalled, value: true);
        };

        Assert.True(await udpClient.Connect());

        // Act
        await udpClient.Send("ping", CancellationToken.None);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref handlerCalled)), "The client should receive the reply.");
        Assert.Null(activityInHandler);

        await udpClient.Disconnect();
        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpClient_ConnectFails_RecordsFailureAndErrorActivity()
    {
        // Arrange - nothing listens on the port.
        var port = GetFreeTcpPort();
        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpClientConfig { ConnectTimeout = 1000 },
            new TcpClientReconnectConfig { Enable = false });

        // Act
        var connected = await tcpClient.Connect();

        // Assert
        Assert.False(connected);
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.ConnectionFailuresInstrumentName, "tcp_client", port));
        Assert.Empty(telemetry.Values(NetworkTelemetry.ActiveConnectionsInstrumentName, "tcp_client", port));
        Assert.Equal(ActivityStatusCode.Error, Assert.Single(telemetry.Activities("TcpClient.Connect", port)).Status);
    }

    [Fact]
    public async Task TcpClient_DataReceivedHandlerThrows_RecordsHandlerError()
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
        tcpClient.DataReceived += _ => throw new InvalidOperationException("handler bug");

        var acceptTask = listener.AcceptTcpClientAsync();
        Assert.True(await tcpClient.Connect());
        using var serverSide = await acceptTask;

        // Act
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes("x"));

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => telemetry.Sum(NetworkTelemetry.HandlerErrorsInstrumentName, "tcp_client", port, NetworkTelemetry.EventTag, "DataReceived") == 1),
            "The failing handler should be counted.");

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task TcpClient_ServerComesBack_RecordsSuccessfulReconnect()
    {
        // Arrange
        var port = GetFreeTcpPort();
        var listener = StartReusableListener(port);
        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpClientConfig { ConnectTimeout = 500 },
            new TcpClientReconnectConfig { Enable = true, RetryInterval = 50, RetryMaxAttempts = 50 });

        var acceptTask = listener.AcceptTcpClientAsync();
        Assert.True(await tcpClient.Connect());
        var serverSide = await acceptTask;

        // Act - the server goes away, then comes back on the same port.
        serverSide.Dispose();
        listener.Stop();
        listener.Dispose();
        await Task.Delay(300);
        using var restartedListener = StartReusableListener(port);
        var reacceptTask = restartedListener.AcceptTcpClientAsync();

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(
                () => telemetry.Sum(NetworkTelemetry.ReconnectsInstrumentName, "tcp_client", port, NetworkTelemetry.OutcomeTag, "success") == 1,
                timeoutInMs: 10_000),
            "The reconnect should be counted.");
        using var reaccepted = await reacceptTask;

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task TcpServer_ClientConnectsSendsAndLeaves_RecordsConnectionTrafficAndActivity()
    {
        // Arrange
        var port = GetFreeTcpPort();
        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.Loopback, port);

        Guid? clientId = null;
        tcpServer.ClientDataReceived += (_, e) => clientId = e.ClientId;
        await tcpServer.StartAsync(CancellationToken.None);

        // Act
        using (var client = new System.Net.Sockets.TcpClient())
        {
            await client.ConnectAsync(IPAddress.Loopback, port);
            await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes("abc"));
            Assert.True(await WaitHelper.WaitUntil(() => clientId is not null), "The server should receive the message.");
            Assert.True(await tcpServer.Send(clientId.Value, "xy", CancellationToken.None));
        }

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => telemetry.Values(NetworkTelemetry.ActiveConnectionsInstrumentName, "tcp_server", port).Count == 2),
            "The client should be counted as connected and then as gone.");
        Assert.Equal([1, -1], telemetry.Values(NetworkTelemetry.ActiveConnectionsInstrumentName, "tcp_server", port));
        Assert.Equal(3, telemetry.Sum(NetworkTelemetry.BytesReceivedInstrumentName, "tcp_server", port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesReceivedInstrumentName, "tcp_server", port));
        Assert.Equal(2, telemetry.Sum(NetworkTelemetry.BytesSentInstrumentName, "tcp_server", port));
        Assert.Single(telemetry.Activities("TcpServer.Send", port));

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_ClientDataReceivedHandlerThrows_RecordsHandlerError()
    {
        // Arrange
        var port = GetFreeTcpPort();
        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.Loopback, port);
        tcpServer.ClientDataReceived += (_, _) => throw new InvalidOperationException("handler bug");
        await tcpServer.StartAsync(CancellationToken.None);

        // Act
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes("abc"));

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => telemetry.Sum(NetworkTelemetry.HandlerErrorsInstrumentName, "tcp_server", port, NetworkTelemetry.EventTag, "ClientDataReceived") == 1),
            "The failing handler should be counted.");

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_TlsHandshakeFails_RecordsConnectionFailure()
    {
        // Arrange
        using var certificate = CertificateHelper.CreateSelfSignedCertificate();
        var port = GetFreeTcpPort();
        using var tcpServer = new TcpServer(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpServerConfig { SslOptions = new SslServerAuthenticationOptions { ServerCertificate = certificate } });
        await tcpServer.StartAsync(CancellationToken.None);

        // Act - a plain TCP client sends something that is not a TLS handshake.
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes("not a TLS handshake\r\n"));

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => telemetry.Sum(NetworkTelemetry.ConnectionFailuresInstrumentName, "tcp_server", port) == 1),
            "The failed handshake should be counted.");
        Assert.Empty(telemetry.Values(NetworkTelemetry.ActiveConnectionsInstrumentName, "tcp_server", port));

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UdpClientAndUdpServer_PingAndReply_RecordTrafficAndActivities()
    {
        // Arrange
        var port = GetFreeUdpPort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        // UdpServer.Send logs socket errors instead of throwing, so this async handler does not throw.
        udpServer.DatagramReceived += async (_, e) => await udpServer.Send(e.RemoteEndPoint, "pong!", CancellationToken.None);
        await udpServer.StartAsync(CancellationToken.None);

        using var udpClient = new UdpClient(NullLogger.Instance, IPAddress.Loopback, port);
        var received = 0;
        udpClient.DataReceived += _ => Interlocked.Increment(ref received);

        // Act
        Assert.True(await udpClient.Connect());
        await udpClient.Send("ping", CancellationToken.None);
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref received) == 1), "The client should receive the reply.");
        await udpClient.Disconnect();

        // Assert
        Assert.Equal([1, -1], telemetry.Values(NetworkTelemetry.ActiveConnectionsInstrumentName, "udp_client", port));
        Assert.Equal(4, telemetry.Sum(NetworkTelemetry.BytesSentInstrumentName, "udp_client", port));
        Assert.Equal(5, telemetry.Sum(NetworkTelemetry.BytesReceivedInstrumentName, "udp_client", port));
        Assert.Equal(4, telemetry.Sum(NetworkTelemetry.BytesReceivedInstrumentName, "udp_server", port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesReceivedInstrumentName, "udp_server", port));
        Assert.Equal(5, telemetry.Sum(NetworkTelemetry.BytesSentInstrumentName, "udp_server", port));
        Assert.Single(telemetry.Activities("UdpClient.Connect", port));
        Assert.Single(telemetry.Activities("UdpClient.Send", port));
        Assert.Single(telemetry.Activities("UdpServer.Send", port));

        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UdpServer_DatagramReceivedHandlerThrows_RecordsHandlerError()
    {
        // Arrange
        var port = GetFreeUdpPort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);
        udpServer.DatagramReceived += (_, _) => throw new InvalidOperationException("handler bug");
        await udpServer.StartAsync(CancellationToken.None);

        // Act
        using var client = new System.Net.Sockets.UdpClient();
        await client.SendAsync(Encoding.ASCII.GetBytes("x"), new IPEndPoint(IPAddress.Loopback, port));

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => telemetry.Sum(NetworkTelemetry.HandlerErrorsInstrumentName, "udp_server", port, NetworkTelemetry.EventTag, "DatagramReceived") == 1),
            "The failing handler should be counted.");

        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task VncClient_ConnectWithThrowingHandlerAndDisconnect_RecordsConnectionAndHandlerError()
    {
        // Arrange
        using var server = new FakeVncServer();
        using var vncClient = new VncClient(
            NullLogger.Instance,
            IPAddress.Loopback,
            server.Port,
            new VncClientConfig { ReceiveTimeout = 10_000, ConnectTimeout = 500 },
            new VncClientReconnectConfig { Enable = false });
        vncClient.Connected += () => throw new InvalidOperationException("handler bug");

        // Act
        var sessionTask = server.AcceptSession();
        Assert.True(await vncClient.Connect());
        Assert.True(await vncClient.Authenticate(string.Empty));
        await vncClient.Initialize();
        using var session = await sessionTask;
        await vncClient.Disconnect();

        // Assert
        Assert.Equal([1, -1], telemetry.Values(NetworkTelemetry.ActiveConnectionsInstrumentName, "vnc_client", server.Port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.HandlerErrorsInstrumentName, "vnc_client", server.Port, NetworkTelemetry.EventTag, "Connected"));
        Assert.Single(telemetry.Activities("VncClient.Connect", server.Port));
    }

    public void Dispose()
        => telemetry.Dispose();

    private static System.Net.Sockets.TcpListener StartReusableListener(
        int port)
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, port);

        // Allow restarting on the same port while the old connection is in TIME_WAIT.
        listener.Server.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, optionValue: true);
        listener.Start();
        return listener;
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