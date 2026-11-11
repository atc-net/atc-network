namespace Atc.Network.Test;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class ReadAllAsyncTests : IDisposable
{
    private readonly System.Net.Sockets.TcpListener listener;

    public ReadAllAsyncTests()
    {
        listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
    }

    private int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    [Fact]
    public async Task TcpClient_ReadAllAsync_YieldsTheMessagesInOrder()
    {
        // Arrange
        using var tcpClient = CreateTcpClient(new TcpClientConfig
        {
            TerminationType = TerminationType.LineFeed,
            MessageFraming = TcpMessageFraming.TerminationType,
        });
        using var serverSide = await Connect(tcpClient);
        using var cts = new CancellationTokenSource(5000);
        await using var reader = tcpClient.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        var first = reader.MoveNextAsync();

        // Act
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes("x\ny\n"));

        // Assert
        Assert.True(await first);
        Assert.Equal("x\n", Encoding.ASCII.GetString(reader.Current.Span));
        Assert.True(await reader.MoveNextAsync());
        Assert.Equal("y\n", Encoding.ASCII.GetString(reader.Current.Span));
    }

    [Fact]
    public async Task TcpClient_ReadAllAsync_SlowReader_StopsTheClientFromReadingAhead()
    {
        // Arrange
        const int messageCount = 600;
        using var tcpClient = CreateTcpClient(new TcpClientConfig
        {
            TerminationType = TerminationType.LineFeed,
            MessageFraming = TcpMessageFraming.TerminationType,
        });
        var raised = 0;
        tcpClient.DataReceived += _ => Interlocked.Increment(ref raised);

        using var serverSide = await Connect(tcpClient);
        using var cts = new CancellationTokenSource(10_000);
        await using var reader = tcpClient.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        var first = reader.MoveNextAsync();

        // Act - send many messages while the reader takes only the first one.
        var payload = string.Concat(Enumerable.Range(0, messageCount).Select(i => $"{i}\n"));
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes(payload));
        Assert.True(await first);
        await Task.Delay(500);
        var raisedWhileReaderWaits = Volatile.Read(ref raised);

        var read = 1;
        while (read < messageCount && await reader.MoveNextAsync())
        {
            read++;
        }

        // Assert - the client stopped at the reader's buffer, then caught up once the reader read.
        Assert.True(raisedWhileReaderWaits < messageCount, $"The client should wait for the reader, but raised {raisedWhileReaderWaits} events.");
        Assert.Equal(messageCount, read);
        Assert.Equal(messageCount, Volatile.Read(ref raised));
    }

    [Fact]
    public async Task TcpClient_ReadAllAsync_ClientDisposed_EndsTheStream()
    {
        // Arrange
        var tcpClient = CreateTcpClient(new TcpClientConfig());
        using var serverSide = await Connect(tcpClient);
        using var cts = new CancellationTokenSource(5000);
        await using var reader = tcpClient.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        var next = reader.MoveNextAsync();

        // Act
        tcpClient.Dispose();

        // Assert
        Assert.False(await next);
    }

    [Fact]
    public async Task TcpClient_ReadAllAsync_Cancelled_Throws()
    {
        // Arrange
        using var tcpClient = CreateTcpClient(new TcpClientConfig());
        using var serverSide = await Connect(tcpClient);
        using var cts = new CancellationTokenSource();
        await using var reader = tcpClient.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        var next = reader.MoveNextAsync();

        // Act
        await cts.CancelAsync();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await next);
    }

    [Fact]
    public async Task UdpClient_ReadAllAsync_YieldsTheReplies()
    {
        // Arrange
        var port = GetFreeUdpPort();
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        // UdpServer.Send logs socket errors instead of throwing, so this async handler does not throw.
        udpServer.DatagramReceived += async (_, e) => await udpServer.Send(e.RemoteEndPoint, "pong", CancellationToken.None);
        await udpServer.StartAsync(CancellationToken.None);

        using var udpClient = new UdpClient(NullLogger.Instance, IPAddress.Loopback, port);
        Assert.True(await udpClient.Connect());
        using var cts = new CancellationTokenSource(5000);
        await using var reader = udpClient.ReadAllAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        var first = reader.MoveNextAsync();

        // Act
        await udpClient.Send("ping", CancellationToken.None);

        // Assert
        Assert.True(await first);
        Assert.Equal("pong", Encoding.ASCII.GetString(reader.Current.Span));

        await udpClient.Disconnect();
        await udpServer.StopAsync(CancellationToken.None);
    }

    public void Dispose()
    {
        listener.Stop();
        listener.Dispose();
    }

    private static int GetFreeUdpPort()
    {
        using var probe = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint).Port;
    }

    private TcpClient CreateTcpClient(TcpClientConfig config)
        => new(
            NullLogger.Instance,
            IPAddress.Loopback,
            Port,
            config,
            new TcpClientReconnectConfig { Enable = false });

    private async Task<System.Net.Sockets.TcpClient> Connect(
        TcpClient tcpClient)
    {
        var acceptTask = listener.AcceptTcpClientAsync();
        Assert.True(await tcpClient.Connect());
        return await acceptTask;
    }
}