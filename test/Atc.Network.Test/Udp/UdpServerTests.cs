namespace Atc.Network.Test.Udp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class UdpServerTests
{
    private const int DelayBetweenMessagesInMs = 150;

    [Fact]
    public void UdpServer_WithoutConfig_UsesDefaultConfig()
    {
        // Arrange
        var port = GetFreePort();

        // Act
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        // Assert
        Assert.False(udpServer.IsRunning);
    }

    [Fact]
    public async Task UdpServer_DataReceived_HandlerThrows_KeepsReceiving()
    {
        // Arrange
        var port = GetFreePort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        var received = 0;
        udpServer.DataReceived += _ =>
        {
            if (Interlocked.Increment(ref received) == 1)
            {
                throw new InvalidOperationException("handler bug");
            }
        };

        await udpServer.StartAsync(CancellationToken.None);

        // Act
        await SendDatagrams(port, "A", "B");

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref received) == 2), $"Expected 2 calls, got {received}.");

        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UdpServer_DataReceived_HandlerThrows_IsLogged()
    {
        // Arrange
        var port = GetFreePort();
        var logger = new CapturingLogger();
        using var udpServer = new UdpServer(logger, IPAddress.Loopback, port);

        var handlerException = new InvalidOperationException("handler bug");
        udpServer.DataReceived += _ => throw handlerException;

        await udpServer.StartAsync(CancellationToken.None);

        // Act
        await SendDatagrams(port, "A");

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => logger.Entries.Any(x =>
                x.Level == LogLevel.Error &&
                x.EventId.Id == LoggingEventIdConstants.UdpServer.DataReceivedHandlerError &&
                ReferenceEquals(x.Exception, handlerException))),
            "Expected the handler exception to be logged as an error.");

        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UdpServer_AfterSendingToClosedPort_KeepsReceiving()
    {
        // Arrange
        var port = GetFreePort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        var received = new ConcurrentQueue<string>();
        udpServer.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        await udpServer.StartAsync(CancellationToken.None);

        // Act - nobody listens on the closed port, so the OS answers with ICMP "port unreachable".
        // On Windows that surfaces as a ConnectionReset on the server's next receive.
        await udpServer.Send(new IPEndPoint(IPAddress.Loopback, GetFreePort()), "x", CancellationToken.None);
        await Task.Delay(DelayBetweenMessagesInMs);
        await SendDatagrams(port, "B");

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("B", StringComparer.Ordinal)), "The datagram sent after the failed send should be received.");

        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UdpServer_PingDatagram_NoReplyByDefault()
    {
        // Arrange
        var port = GetFreePort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);
        await udpServer.StartAsync(CancellationToken.None);

        // Act
        var reply = await SendAndReceiveReply(port, "ping");

        // Assert
        Assert.Null(reply);

        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task UdpServer_PingDatagram_RepliesPong_WhenRespondToPingIsEnabled()
    {
        // Arrange
        var port = GetFreePort();
        using var udpServer = new UdpServer(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new UdpServerConfig { RespondToPing = true });
        await udpServer.StartAsync(CancellationToken.None);

        // Act
        var reply = await SendAndReceiveReply(port, "ping");

        // Assert
        Assert.Equal("pong", reply);

        await udpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task DatagramReceived_CarriesSenderEndPoint_SoTheServerCanReply()
    {
        // Arrange
        var port = GetFreePort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        string? receivedText = null;
        udpServer.DatagramReceived += async (_, e) =>
        {
            receivedText = Encoding.ASCII.GetString(e.Data.Span);
            await udpServer.Send(e.RemoteEndPoint, "reply", CancellationToken.None);
        };

        await udpServer.StartAsync(CancellationToken.None);

        // Act
        var reply = await SendAndReceiveReply(port, "hello");

        // Assert
        Assert.Equal("hello", receivedText);
        Assert.Equal("reply", reply);

        await udpServer.StopAsync(CancellationToken.None);
    }

    private static int GetFreePort()
    {
        using var probe = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint).Port;
    }

    /// <summary>
    /// Sends one datagram and returns the server's reply, or null when none arrives within 500 ms.
    /// </summary>
    private static async Task<string?> SendAndReceiveReply(
        int port,
        string message)
    {
        using var client = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        await client.SendAsync(Encoding.ASCII.GetBytes(message), new IPEndPoint(IPAddress.Loopback, port));

        using var timeout = new CancellationTokenSource(500);
        try
        {
            var result = await client.ReceiveAsync(timeout.Token);
            return Encoding.ASCII.GetString(result.Buffer);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static async Task SendDatagrams(
        int port,
        params string[] messages)
    {
        using var client = new System.Net.Sockets.UdpClient();
        var serverEndPoint = new IPEndPoint(IPAddress.Loopback, port);
        foreach (var message in messages)
        {
            await client.SendAsync(Encoding.ASCII.GetBytes(message), serverEndPoint);
            await Task.Delay(DelayBetweenMessagesInMs);
        }
    }
}