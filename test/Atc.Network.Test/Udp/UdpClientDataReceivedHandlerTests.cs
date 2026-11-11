namespace Atc.Network.Test.Udp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class UdpClientDataReceivedHandlerTests : IDisposable
{
    private const int DelayBetweenMessagesInMs = 150;

    private readonly System.Net.Sockets.UdpClient server;

    public UdpClientDataReceivedHandlerTests()
    {
        server = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
    }

    private IPEndPoint ServerEndPoint
        => (IPEndPoint)server.Client.LocalEndPoint!;

    [Fact]
    public async Task UdpClient_DataReceived_HandlerThrows_KeepsReceiving()
    {
        // Arrange
        using var udpClient = new UdpClient(NullLogger.Instance, ServerEndPoint);

        var received = 0;
        udpClient.DataReceived += _ =>
        {
            if (Interlocked.Increment(ref received) == 1)
            {
                throw new InvalidOperationException("handler bug");
            }
        };

        // Act
        await ConnectAndReceiveTwoDatagrams(udpClient);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => Volatile.Read(ref received) == 2), $"Expected 2 calls, got {received}.");
        Assert.True(udpClient.IsConnected);

        await udpClient.Disconnect();
    }

    [Fact]
    public async Task UdpClient_OnDataReceivedOverrideThrows_KeepsReceiving()
    {
        // Arrange
        using var udpClient = new ThrowingOnFirstDataUdpClient(ServerEndPoint);

        // Act
        await ConnectAndReceiveTwoDatagrams(udpClient);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => udpClient.Calls == 2), $"Expected 2 calls, got {udpClient.Calls}.");
        Assert.True(udpClient.IsConnected);

        await udpClient.Disconnect();
    }

    [Fact]
    public async Task UdpClient_DataReceived_HandlerThrows_IsLogged()
    {
        // Arrange
        var logger = new CapturingLogger();
        using var udpClient = new UdpClient(logger, ServerEndPoint);

        var handlerException = new InvalidOperationException("handler bug");
        udpClient.DataReceived += _ => throw handlerException;

        // Act
        await ConnectAndReceiveTwoDatagrams(udpClient);

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => logger.Entries.Any(x =>
                x.Level == LogLevel.Error &&
                x.EventId.Id == LoggingEventIdConstants.UdpClient.DataReceivedHandlerError &&
                ReferenceEquals(x.Exception, handlerException))),
            "Expected the handler exception to be logged as an error.");

        await udpClient.Disconnect();
    }

    public void Dispose()
    {
        server.Dispose();
    }

    private async Task ConnectAndReceiveTwoDatagrams(UdpClient udpClient)
    {
        await udpClient.Connect();

        // The server learns the client's endpoint from the first datagram.
        await udpClient.Send("hello", CancellationToken.None);
        var hello = await server.ReceiveAsync();

        await server.SendAsync(Encoding.ASCII.GetBytes("A"), hello.RemoteEndPoint);
        await Task.Delay(DelayBetweenMessagesInMs);
        await server.SendAsync(Encoding.ASCII.GetBytes("B"), hello.RemoteEndPoint);
    }

    private sealed class ThrowingOnFirstDataUdpClient : UdpClient
    {
        private int calls;

        public ThrowingOnFirstDataUdpClient(IPEndPoint endpoint)
            : base(NullLogger.Instance, endpoint)
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