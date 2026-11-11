namespace Atc.Network.Tcp;

/// <summary>
/// One client connected to a <see cref="TcpServer"/>.
/// </summary>
internal sealed class TcpServerConnection : IDisposable
{
    public TcpServerConnection(
        Guid clientId,
        System.Net.Sockets.TcpClient client,
        int receiveBufferSize)
    {
        ArgumentNullException.ThrowIfNull(client);

        ClientId = clientId;
        Client = client;
        RemoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;
        ReceiveBuffer = new byte[receiveBufferSize];
    }

    public Guid ClientId { get; }

    public System.Net.Sockets.TcpClient Client { get; }

    public IPEndPoint RemoteEndPoint { get; }

    /// <summary>
    /// Gets the read buffer, reused for every read from this client.
    /// </summary>
    public byte[] ReceiveBuffer { get; }

    /// <summary>
    /// Gets the lock that keeps sends to this client from interleaving.
    /// </summary>
    public SemaphoreSlim SendLock { get; } = new(1, 1);

    public void Dispose()
    {
        Client.Dispose();
        SendLock.Dispose();
    }
}