namespace Atc.Network.Test.Vnc;

/// <summary>
/// End-to-end tests of <see cref="VncClient"/> against an in-process RFB server (<see cref="FakeVncServer"/>).
/// </summary>
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class VncClientEndToEndTests
{
    private const string Password = "secret";

    [Fact]
    public async Task Handshake_WithoutPassword_InitializesFramebuffer()
    {
        // Arrange
        using var server = new FakeVncServer();
        using var vncClient = CreateClient(server);

        // Act
        using var session = await ConnectAndStartUpdates(vncClient, server, password: string.Empty);

        // Assert
        Assert.True(vncClient.IsConnected);
        Assert.NotNull(vncClient.Framebuffer);
        Assert.Equal(FakeVncServer.Width, vncClient.Framebuffer.Width);
        Assert.Equal(FakeVncServer.Height, vncClient.Framebuffer.Height);
        Assert.Equal(FakeVncServer.DesktopName, vncClient.Framebuffer.DesktopName);

        await vncClient.Disconnect();
    }

    [Fact]
    public async Task Authenticate_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        using var server = new FakeVncServer(Password);
        using var vncClient = CreateClient(server);
        var sessionTask = server.AcceptSession();
        await vncClient.Connect();

        // Act
        var actual = await vncClient.Authenticate(Password);

        // Assert
        Assert.True(actual);

        await vncClient.Initialize();
        using var session = await sessionTask;
        Assert.True(session.AuthenticationSucceeded);

        await vncClient.Disconnect();
    }

    [Fact]
    public async Task Authenticate_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        using var server = new FakeVncServer(Password);
        using var vncClient = CreateClient(server);
        var sessionTask = server.AcceptSession();
        await vncClient.Connect();

        // Act
        var actual = await vncClient.Authenticate("wrong");

        // Assert
        Assert.False(actual);
        using var session = await sessionTask;
        Assert.False(session.AuthenticationSucceeded);

        await vncClient.Disconnect();
    }

    [Fact]
    public async Task FramebufferUpdate_Raw_UpdatesFramebufferAndRaisesEvent()
    {
        // Arrange
        using var server = new FakeVncServer();
        using var vncClient = CreateClient(server);
        var updates = new ConcurrentQueue<VncRectangle>();
        vncClient.FramebufferUpdated += (_, e) => updates.Enqueue(e.Rectangle);
        using var session = await ConnectAndStartUpdates(vncClient, server, password: string.Empty);

        // Act - a red 2x1 rectangle at (1, 1).
        await session.SendRawRectangle(1, 1, 2, 1, red: 255, green: 0, blue: 0);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => !updates.IsEmpty), "FramebufferUpdated should be raised.");
        Assert.Equal(new VncRectangle(1, 1, 2, 1), updates.First());
        Assert.Equal(unchecked((int)0xFFFF0000), vncClient.Framebuffer[1, 1]);
        Assert.Equal(unchecked((int)0xFFFF0000), vncClient.Framebuffer[2, 1]);
        Assert.NotEqual(unchecked((int)0xFFFF0000), vncClient.Framebuffer[0, 0]);

        await vncClient.Disconnect();
    }

    [Fact]
    public async Task BellAndServerCutText_RaiseEvents()
    {
        // Arrange
        using var server = new FakeVncServer();
        using var vncClient = CreateClient(server);
        var bellReceived = false;
        string? cutText = null;
        vncClient.BellReceived += () => bellReceived = true;
        vncClient.ServerCutText += text => cutText = text;
        using var session = await ConnectAndStartUpdates(vncClient, server, password: string.Empty);

        // Act
        await session.SendBell();
        await session.SendServerCutText("copied text");

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => bellReceived), "BellReceived should be raised.");
        Assert.True(await WaitHelper.WaitUntil(() => cutText is not null), "ServerCutText should be raised.");
        Assert.Equal("copied text", cutText);

        await vncClient.Disconnect();
    }

    [Fact]
    public async Task FramebufferUpdatedHandlerThrows_KeepsTheSession()
    {
        // Arrange - reconnect is off, so a dropped session would stay dropped.
        using var server = new FakeVncServer();
        using var vncClient = CreateClient(server, reconnect: false);
        var bellReceived = false;
        vncClient.FramebufferUpdated += (_, _) => throw new InvalidOperationException("handler bug");
        vncClient.BellReceived += () => bellReceived = true;
        using var session = await ConnectAndStartUpdates(vncClient, server, password: string.Empty);

        // Act
        await session.SendRawRectangle(0, 0, 1, 1, red: 0, green: 255, blue: 0);
        await session.SendBell();

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => bellReceived), "The session should survive a throwing handler.");
        Assert.True(vncClient.IsConnected);

        await vncClient.Disconnect();
    }

    [Fact]
    public async Task ConnectionDropped_WithReconnect_ReconnectsAndResumesUpdates()
    {
        // Arrange
        using var server = new FakeVncServer(Password);
        using var vncClient = CreateClient(server, reconnect: true);
        var states = new ConcurrentQueue<ConnectionState>();
        var bellReceived = false;
        vncClient.ConnectionStateChanged += (_, e) => states.Enqueue(e.State);
        vncClient.BellReceived += () => bellReceived = true;

        var firstSession = await ConnectAndStartUpdates(vncClient, server, Password);

        // Act - the server drops the connection; the client reconnects and authenticates again.
        var secondSessionTask = server.AcceptSession();
        firstSession.Dispose();
        using var secondSession = await secondSessionTask;
        await secondSession.SendBell();

        // Assert
        Assert.True(secondSession.AuthenticationSucceeded);
        Assert.True(await WaitHelper.WaitUntil(() => states.Contains(ConnectionState.Reconnected)), "The client should report Reconnected.");
        Assert.True(await WaitHelper.WaitUntil(() => bellReceived), "Updates should resume on the new connection.");

        await vncClient.Disconnect();
    }

    [Fact]
    public async Task ConnectionDropped_WithoutReconnect_RaisesConnectionLost()
    {
        // Arrange
        using var server = new FakeVncServer();
        using var vncClient = CreateClient(server, reconnect: false);
        var connectionLost = false;
        vncClient.ConnectionLost += () => connectionLost = true;
        var session = await ConnectAndStartUpdates(vncClient, server, password: string.Empty);

        // Act
        session.Dispose();

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => connectionLost), "ConnectionLost should be raised.");
        Assert.False(vncClient.IsConnected);
    }

    private static VncClient CreateClient(
        FakeVncServer server,
        bool reconnect = false)
        => new(
            NullLogger.Instance,
            IPAddress.Loopback,
            server.Port,
            new VncClientConfig { ReceiveTimeout = 10_000 },
            new VncClientReconnectConfig { Enable = reconnect, RetryInterval = 100, RetryMaxAttempts = 20 });

    private static async Task<FakeVncSession> ConnectAndStartUpdates(
        VncClient vncClient,
        FakeVncServer server,
        string password)
    {
        var sessionTask = server.AcceptSession();
        Assert.True(await vncClient.Connect());
        Assert.True(await vncClient.Authenticate(password));
        await vncClient.Initialize();
        var session = await sessionTask;
        await vncClient.StartUpdates();
        return session;
    }
}