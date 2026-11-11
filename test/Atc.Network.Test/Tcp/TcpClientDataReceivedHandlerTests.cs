namespace Atc.Network.Test.Tcp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class TcpClientDataReceivedHandlerTests : IDisposable
{
    private readonly System.Net.Sockets.TcpListener listener;
    private readonly List<System.Net.Sockets.TcpClient> serverSides = new();

    public TcpClientDataReceivedHandlerTests()
    {
        listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
    }

    private int ListenerPort => ((IPEndPoint)listener.LocalEndpoint).Port;

    [Fact]
    public async Task TcpClient_DataReceived_HandlerThrows_KeepsReceiving()
    {
        // Arrange
        var received = 0;
        var serverTask = SendTwoMessagesToFirstClient(() => Volatile.Read(ref received) >= 1);
        using var tcpClient = new TcpClient(NullLogger.Instance, IPAddress.Loopback, ListenerPort);

        tcpClient.DataReceived += _ =>
        {
            if (Interlocked.Increment(ref received) == 1)
            {
                throw new InvalidOperationException("handler bug");
            }
        };

        // Act
        await tcpClient.Connect();
        await serverTask;

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref received) == 2), $"Expected 2 calls, got {received}.");
        Assert.True(tcpClient.IsConnected);

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task TcpClient_DataReceived_HandlerThrows_OtherHandlersStillCalled()
    {
        // Arrange
        var secondHandlerCalls = 0;
        var serverTask = SendTwoMessagesToFirstClient(() => Volatile.Read(ref secondHandlerCalls) >= 1);
        using var tcpClient = new TcpClient(NullLogger.Instance, IPAddress.Loopback, ListenerPort);

        tcpClient.DataReceived += _ => throw new InvalidOperationException("handler bug");
        tcpClient.DataReceived += _ => Interlocked.Increment(ref secondHandlerCalls);

        // Act
        await tcpClient.Connect();
        await serverTask;

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref secondHandlerCalls) == 2), $"Expected 2 calls, got {secondHandlerCalls}.");

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task TcpClient_OnDataReceivedOverrideThrows_KeepsReceiving()
    {
        // Arrange
        using var tcpClient = new ThrowingOnFirstDataTcpClient(ListenerPort);
        var serverTask = SendTwoMessagesToFirstClient(() => tcpClient.Calls >= 1);

        // Act
        await tcpClient.Connect();
        await serverTask;

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => tcpClient.Calls == 2), $"Expected 2 calls, got {tcpClient.Calls}.");
        Assert.True(tcpClient.IsConnected);

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task TcpClient_DataReceived_HandlerThrows_IsLogged()
    {
        // Arrange
        var logger = new CapturingLogger();
        var serverTask = SendTwoMessagesToFirstClient(() => logger.Entries.Any(x => x.Level == LogLevel.Error));
        using var tcpClient = new TcpClient(logger, IPAddress.Loopback, ListenerPort);

        var handlerException = new InvalidOperationException("handler bug");
        tcpClient.DataReceived += _ => throw handlerException;

        // Act
        await tcpClient.Connect();
        await serverTask;

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => logger.Entries.Any(x =>
                x.Level == LogLevel.Error &&
                x.EventId.Id == LoggingEventIdConstants.TcpClient.DataReceivedHandlerError &&
                ReferenceEquals(x.Exception, handlerException))),
            "Expected the handler exception to be logged as an error.");

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task TcpClient_ServerClosesAndNoDataReceivedHandlerThrows_SetsDisconnected()
    {
        // Arrange - the server closes the connection, and a NoDataReceived handler throws.
        // The handler is isolated, so the client still goes through its normal disconnect.
        var serverTask = AcceptAndCloseFirstClient();
        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            IPAddress.Loopback,
            ListenerPort,
            reconnectConfig: new TcpClientReconnectConfig { Enable = false });

        var disconnectedRaised = false;
        tcpClient.Disconnected += () => disconnectedRaised = true;
        tcpClient.NoDataReceived += () => throw new InvalidOperationException("handler bug");

        // Act
        await tcpClient.Connect();
        await serverTask;

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => !tcpClient.IsConnected), "IsConnected should be false.");
        Assert.True(await WaitHelper.WaitUntil(() => disconnectedRaised), "Disconnected should be raised.");
    }

    [Fact]
    public async Task TcpClient_DataReceived_DeliversExactlyTheBytesRead()
    {
        // Arrange
        var received = new ConcurrentQueue<string>();
        var serverTask = SendMessagesToFirstClient(["a long message", "ok"], () => !received.IsEmpty);
        using var tcpClient = new TcpClient(NullLogger.Instance, IPAddress.Loopback, ListenerPort);
        tcpClient.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        // Act
        await tcpClient.Connect();
        await serverTask;

        // Assert - the short read must not carry leftover bytes from the long one.
        Assert.True(await WaitHelper.WaitUntil(() => received.Count == 2), $"Expected 2 reads, got {received.Count}.");
        Assert.Equal(["a long message", "ok"], received.ToArray());

        await tcpClient.Disconnect();
    }

    public void Dispose()
    {
        foreach (var serverSide in serverSides)
        {
            serverSide.Dispose();
        }

        listener.Stop();
        listener.Dispose();
    }

    /// <summary>
    /// Sends "A", waits until the client has received it, then sends "B".
    /// Waiting keeps the two messages from arriving in one TCP read.
    /// </summary>
    private async Task SendTwoMessagesToFirstClient(
        Func<bool> firstMessageReceived)
    {
        var serverSide = await listener.AcceptTcpClientAsync();
        serverSides.Add(serverSide);
        var stream = serverSide.GetStream();

        await stream.WriteAsync(Encoding.ASCII.GetBytes("A"));
        await WaitHelper.WaitUntil(firstMessageReceived);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("B"));
    }

    /// <summary>
    /// Sends the first message, waits until the client has received it, then sends the second.
    /// </summary>
    private async Task SendMessagesToFirstClient(
        string[] messages,
        Func<bool> firstMessageReceived)
    {
        var serverSide = await listener.AcceptTcpClientAsync();
        serverSides.Add(serverSide);
        var stream = serverSide.GetStream();

        await stream.WriteAsync(Encoding.ASCII.GetBytes(messages[0]));
        await WaitHelper.WaitUntil(firstMessageReceived);
        await stream.WriteAsync(Encoding.ASCII.GetBytes(messages[1]));
    }

    private async Task AcceptAndCloseFirstClient()
    {
        using var serverSide = await listener.AcceptTcpClientAsync();
        serverSide.Close();
    }

    private sealed class ThrowingOnFirstDataTcpClient : TcpClient
    {
        private int calls;

        public ThrowingOnFirstDataTcpClient(int port)
            : base(NullLogger.Instance, IPAddress.Loopback, port)
        {
        }

        public int Calls => Volatile.Read(ref calls);

        protected override void OnDataReceived(byte[] bytes)
        {
            if (Interlocked.Increment(ref calls) == 1)
            {
                throw new InvalidOperationException("override bug");
            }
        }
    }
}