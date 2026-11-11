namespace Atc.Network.Test.Udp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class UdpClientTests : IDisposable
{
    private readonly System.Net.Sockets.UdpClient server;

    public UdpClientTests()
    {
        server = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
    }

    private IPEndPoint ServerEndPoint
        => (IPEndPoint)server.Client.LocalEndPoint;

    [Fact]
    public async Task Connect_SetsIsConnectedAndRaisesConnected()
    {
        // Arrange
        using var udpClient = new UdpClient(NullLogger.Instance, ServerEndPoint);
        var connectedRaised = false;
        var states = new List<ConnectionState>();
        udpClient.Connected += () => connectedRaised = true;
        udpClient.ConnectionStateChanged += (_, e) => states.Add(e.State);

        // Act
        var actual = await udpClient.Connect();

        // Assert
        Assert.True(actual);
        Assert.True(udpClient.IsConnected);
        Assert.True(connectedRaised);
        Assert.Equal([ConnectionState.Connecting, ConnectionState.Connected], states);

        await udpClient.Disconnect();
    }

    [Fact]
    public async Task Connect_WhenAlreadyConnected_ReturnsFalse()
    {
        // Arrange
        using var udpClient = new UdpClient(NullLogger.Instance, ServerEndPoint);
        await udpClient.Connect();

        // Act
        var actual = await udpClient.Connect();

        // Assert
        Assert.False(actual);
        Assert.True(udpClient.IsConnected);

        await udpClient.Disconnect();
    }

    [Fact]
    public async Task Send_String_ServerReceivesData()
    {
        // Arrange
        using var udpClient = new UdpClient(NullLogger.Instance, ServerEndPoint);
        await udpClient.Connect();

        // Act
        await udpClient.Send("hello", CancellationToken.None);

        // Assert
        Assert.Equal("hello", await ReceiveOnServer());

        await udpClient.Disconnect();
    }

    [Fact]
    public async Task Send_WithTerminationType_AppendsTerminator()
    {
        // Arrange
        using var udpClient = new UdpClient(
            NullLogger.Instance,
            ServerEndPoint,
            new UdpClientConfig { TerminationType = TerminationType.LineFeed });
        await udpClient.Connect();

        // Act
        await udpClient.Send("hello", CancellationToken.None);

        // Assert
        Assert.Equal("hello\n", await ReceiveOnServer());

        await udpClient.Disconnect();
    }

    [Fact]
    public async Task DataReceived_RaisedForDatagramFromServer()
    {
        // Arrange
        using var udpClient = new UdpClient(NullLogger.Instance, ServerEndPoint);
        var received = new ConcurrentQueue<string>();
        udpClient.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));
        await udpClient.Connect();

        // The server learns the client's endpoint from the first datagram.
        await udpClient.Send("hello", CancellationToken.None);
        var hello = await server.ReceiveAsync();

        // Act
        await server.SendAsync(Encoding.ASCII.GetBytes("reply"), hello.RemoteEndPoint);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("reply", StringComparer.Ordinal)), "The client should receive the server's datagram.");

        await udpClient.Disconnect();
    }

    [Fact]
    public async Task Disconnect_ClearsIsConnectedAndRaisesDisconnected()
    {
        // Arrange
        using var udpClient = new UdpClient(NullLogger.Instance, ServerEndPoint);
        var disconnectedRaised = false;
        udpClient.Disconnected += () => disconnectedRaised = true;
        await udpClient.Connect();

        // Act
        await udpClient.Disconnect();

        // Assert
        Assert.False(udpClient.IsConnected);
        Assert.True(disconnectedRaised);
    }

    [Fact]
    public async Task Send_WhenNotConnected_SendsNothing()
    {
        // Arrange
        using var udpClient = new UdpClient(NullLogger.Instance, ServerEndPoint);

        // Act
        await udpClient.Send("hello", CancellationToken.None);

        // Assert
        Assert.Null(await ReceiveOnServer(timeoutInMs: 300));
    }

    public void Dispose()
        => server.Dispose();

    /// <summary>
    /// Returns the next datagram the server receives as text, or null when none arrives in time.
    /// </summary>
    private async Task<string?> ReceiveOnServer(int timeoutInMs = 3000)
    {
        using var timeout = new CancellationTokenSource(timeoutInMs);
        try
        {
            var result = await server.ReceiveAsync(timeout.Token);
            return Encoding.ASCII.GetString(result.Buffer);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}