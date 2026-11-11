namespace Atc.Network.Tcp;

/// <summary>
/// One client connected to a <see cref="TcpServer"/>.
/// </summary>
internal sealed class TcpServerConnection : IDisposable
{
    public TcpServerConnection(
        Guid clientId,
        System.Net.Sockets.TcpClient client,
        Stream stream,
        int receiveBufferSize)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(stream);

        ClientId = clientId;
        Client = client;
        Stream = stream;
        RemoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;
        ReceiveBuffer = new byte[receiveBufferSize];
    }

    public Guid ClientId { get; }

    public System.Net.Sockets.TcpClient Client { get; }

    /// <summary>
    /// Gets the stream to read from and write to: the network stream, or the TLS stream when TLS is enabled.
    /// </summary>
    public Stream Stream { get; }

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
        Stream.Dispose();
        Client.Dispose();
        SendLock.Dispose();
    }
}