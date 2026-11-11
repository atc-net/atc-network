namespace Atc.Network.Test.Tcp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class TcpServerTests
{
    [Fact]
    public async Task TcpServer_ClientResetsConnection_KeepsAcceptingClients()
    {
        // Arrange
        var port = GetFreePort();
        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.Loopback, port);

        var received = new ConcurrentQueue<string>();
        tcpServer.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        await tcpServer.StartAsync(CancellationToken.None);

        // Act - the first client resets its connection (RST) while the server is reading.
        using (var resettingClient = new System.Net.Sockets.TcpClient())
        {
            await resettingClient.ConnectAsync(IPAddress.Loopback, port);
            await Task.Delay(200);
            resettingClient.LingerState = new System.Net.Sockets.LingerOption(enable: true, seconds: 0);
            resettingClient.Close();
        }

        using var secondClient = new System.Net.Sockets.TcpClient();
        await secondClient.ConnectAsync(IPAddress.Loopback, port);
        await secondClient.GetStream().WriteAsync(Encoding.ASCII.GetBytes("B"));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("B", StringComparer.Ordinal)), "The second client's data should be received.");
        Assert.True(tcpServer.IsRunning);

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_DataReceived_HandlerThrows_KeepsReceiving()
    {
        // Arrange
        var port = GetFreePort();
        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.Loopback, port);

        var received = 0;
        tcpServer.DataReceived += _ =>
        {
            if (Interlocked.Increment(ref received) == 1)
            {
                throw new InvalidOperationException("handler bug");
            }
        };

        await tcpServer.StartAsync(CancellationToken.None);

        // Act
        await SendTwoMessages(port, () => Volatile.Read(ref received) >= 1);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref received) == 2), $"Expected 2 calls, got {received}.");
        Assert.True(tcpServer.IsRunning);

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_DataReceived_HandlerThrows_IsLogged()
    {
        // Arrange
        var port = GetFreePort();
        var logger = new CapturingLogger();
        using var tcpServer = new TcpServer(logger, IPAddress.Loopback, port);

        var handlerException = new InvalidOperationException("handler bug");
        tcpServer.DataReceived += _ => throw handlerException;

        await tcpServer.StartAsync(CancellationToken.None);

        // Act
        await SendTwoMessages(port, () => logger.Entries.Any(x => x.Level == LogLevel.Error));

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => logger.Entries.Any(x =>
                x.Level == LogLevel.Error &&
                x.EventId.Id == LoggingEventIdConstants.TcpServer.DataReceivedHandlerError &&
                ReferenceEquals(x.Exception, handlerException))),
            "Expected the handler exception to be logged as an error.");

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_PortInUse_Throws()
    {
        // Arrange - another listener already owns the port.
        using var occupyingListener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        occupyingListener.Start();
        var port = ((IPEndPoint)occupyingListener.LocalEndpoint).Port;

        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.Loopback, port);

        // Act & Assert
        await Assert.ThrowsAsync<System.Net.Sockets.SocketException>(() => tcpServer.StartAsync(CancellationToken.None));
        Assert.False(tcpServer.IsRunning);
    }

    [Fact]
    public async Task StartAsync_WhenItReturns_IsRunningAndAcceptsClients()
    {
        // Arrange
        var port = GetFreePort();
        using var tcpServer = new TcpServer(NullLogger.Instance, IPAddress.Loopback, port);

        // Act
        await tcpServer.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(tcpServer.IsRunning);
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        Assert.True(client.Connected);

        await tcpServer.StopAsync(CancellationToken.None);
    }

    private static int GetFreePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>
    /// Sends "A", waits until the server has handled it, then sends "B".
    /// Waiting keeps the two messages from arriving in one TCP read.
    /// </summary>
    private static async Task SendTwoMessages(
        int port,
        Func<bool> firstMessageHandled)
    {
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        var stream = client.GetStream();

        await stream.WriteAsync(Encoding.ASCII.GetBytes("A"));
        await WaitHelper.WaitUntil(firstMessageHandled);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("B"));

        // Keep the connection open until the server has had time to read "B".
        await Task.Delay(300);
    }
}