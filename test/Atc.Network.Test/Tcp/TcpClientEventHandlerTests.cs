namespace Atc.Network.Test.Tcp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class TcpClientEventHandlerTests : IDisposable
{
    private readonly System.Net.Sockets.TcpListener listener;

    public TcpClientEventHandlerTests()
    {
        listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
    }

    private int ListenerPort => ((IPEndPoint)listener.LocalEndpoint).Port;

    [Fact]
    public async Task ConnectionEventHandlersThatThrow_DoNotBreakConnectOrDisconnect()
    {
        // Arrange
        var logger = new CapturingLogger();
        using var tcpClient = new TcpClient(
            logger,
            IPAddress.Loopback,
            ListenerPort,
            reconnectConfig: new TcpClientReconnectConfig { Enable = false });

        tcpClient.ConnectionStateChanged += (_, _) => throw new InvalidOperationException("handler bug");
        tcpClient.Connected += () => throw new InvalidOperationException("handler bug");
        tcpClient.Disconnected += () => throw new InvalidOperationException("handler bug");

        var acceptTask = listener.AcceptTcpClientAsync();

        // Act
        var connected = await tcpClient.Connect();
        using var serverSide = await acceptTask;
        var isConnectedAfterConnect = tcpClient.IsConnected;
        await tcpClient.Disconnect();

        // Assert
        Assert.True(connected);
        Assert.True(isConnectedAfterConnect);
        Assert.False(tcpClient.IsConnected);
        Assert.Contains(logger.Entries, x => x.EventId.Id == LoggingEventIdConstants.TcpClient.EventHandlerError);
    }

    public void Dispose()
    {
        listener.Stop();
        listener.Dispose();
    }
}