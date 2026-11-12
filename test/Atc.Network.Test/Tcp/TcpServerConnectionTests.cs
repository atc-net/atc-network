namespace Atc.Network.Test.Tcp;

/// <summary>
/// Tests for <see cref="TcpServerConnection"/>, one client of a <see cref="TcpServer"/>.
/// </summary>
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class TcpServerConnectionTests
{
    [Fact]
    public async Task Dispose_WhileSendsAreQueued_QueuedSendsStillGetTheLock()
    {
        // Arrange - one send holds the lock and another waits for it, as when the server sends to a busy client.
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var peer = new System.Net.Sockets.TcpClient();
        await peer.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
        var serverSide = await listener.AcceptTcpClientAsync();
        var connection = new TcpServerConnection(Guid.NewGuid(), serverSide, serverSide.GetStream(), receiveBufferSize: 1024);

        await connection.SendLock.WaitAsync();
        var queuedSend = connection.SendLock.WaitAsync();

        // Act - the client disconnects, so the receive task disposes the connection, and the first send then
        // releases the lock the same way TcpServer does.
        connection.Dispose();
        try
        {
            connection.SendLock.Release();
        }
        catch (ObjectDisposedException)
        {
            // TcpServer ignores this, so a queued send would never be woken.
        }

        // Assert - the queued send gets the lock, finds the stream closed and returns, instead of waiting forever.
        var completed = await WaitHelper.WaitUntil(() => queuedSend.IsCompleted);
        Assert.True(completed, "The queued send should get the lock after the connection is disposed.");
    }
}