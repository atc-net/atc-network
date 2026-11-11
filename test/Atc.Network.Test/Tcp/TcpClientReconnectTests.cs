namespace Atc.Network.Test.Tcp;

/// <summary>
/// Reconnect behaviour of <see cref="TcpClient"/> against a local listener that is stopped and restarted.
/// </summary>
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class TcpClientReconnectTests
{
    [Fact]
    public async Task ServerDropsConnectionAndComesBack_Reconnects()
    {
        // Arrange
        var port = GetFreePort();
        var listener = StartListener(port);
        var states = new ConcurrentQueue<ConnectionState>();

        using var tcpClient = CreateClient(port, retryMaxAttempts: 50);
        tcpClient.ConnectionStateChanged += (_, e) => states.Enqueue(e.State);

        var acceptTask = listener.AcceptTcpClientAsync();
        await tcpClient.Connect();
        var serverSide = await acceptTask;

        // Act - the server goes away, then comes back on the same port.
        serverSide.Dispose();
        listener.Stop();
        listener.Dispose();
        Assert.True(await WaitHelper.WaitUntil(() => states.Contains(ConnectionState.Reconnecting)), "The client should start reconnecting.");

        using var restartedListener = StartListener(port);
        var reacceptTask = restartedListener.AcceptTcpClientAsync();

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => states.Contains(ConnectionState.Reconnected), timeoutInMs: 10_000), "The client should reconnect.");
        Assert.True(tcpClient.IsConnected);
        using var reaccepted = await reacceptTask;

        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task ServerNeverComesBack_StopsAfterRetryMaxAttempts()
    {
        // Arrange
        var port = GetFreePort();
        var listener = StartListener(port);
        var states = new ConcurrentQueue<ConnectionState>();

        using var tcpClient = CreateClient(port, retryMaxAttempts: 2);
        tcpClient.ConnectionStateChanged += (_, e) => states.Enqueue(e.State);

        var acceptTask = listener.AcceptTcpClientAsync();
        await tcpClient.Connect();
        var serverSide = await acceptTask;

        // Act - the server goes away for good.
        serverSide.Dispose();
        listener.Stop();
        listener.Dispose();

        // Assert - one first attempt plus RetryMaxAttempts retries, each reported as ReconnectionFailed.
        Assert.True(
            await WaitHelper.WaitUntil(() => states.Count(x => x == ConnectionState.ReconnectionFailed) == 3, timeoutInMs: 15_000),
            $"Expected 3 ReconnectionFailed, got {states.Count(x => x == ConnectionState.ReconnectionFailed)}.");

        await Task.Delay(1500);
        Assert.Equal(3, states.Count(x => x == ConnectionState.ReconnectionFailed));
        Assert.Equal(3, states.Count(x => x == ConnectionState.Reconnecting));
        Assert.False(tcpClient.IsConnected);
    }

    private static TcpClient CreateClient(
        int port,
        int retryMaxAttempts)
        => new(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpClientConfig { ConnectTimeout = 500 },
            new TcpClientReconnectConfig { Enable = true, RetryInterval = 50, RetryMaxAttempts = retryMaxAttempts });

    private static System.Net.Sockets.TcpListener StartListener(int port)
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, port);

        // Allow restarting on the same port while the old connection is in TIME_WAIT.
        listener.Server.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, optionValue: true);
        listener.Start();
        return listener;
    }

    private static int GetFreePort()
    {
        using var probe = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }
}