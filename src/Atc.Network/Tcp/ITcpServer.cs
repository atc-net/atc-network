namespace Atc.Network.Tcp;

public interface ITcpServer : IHostedService, IDisposable
{
    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    event Action<byte[]>? DataReceived;

    /// <summary>
    /// Event to raise when a message is received, with the client that sent it.
    /// </summary>
    event EventHandler<TcpServerDataReceivedEventArgs>? ClientDataReceived;

    /// <summary>
    /// Event to raise when a client connects.
    /// </summary>
    event EventHandler<TcpServerClientEventArgs>? ClientConnected;

    /// <summary>
    /// Event to raise when a client disconnects or is closed by the server.
    /// </summary>
    event EventHandler<TcpServerClientEventArgs>? ClientDisconnected;

    /// <summary>
    /// Event to raise when an error was handled and logged, so the server keeps running:
    /// an event handler that threw, a client connection that failed, or the listener stopping. A handler of this event that throws is only logged.
    /// </summary>
    event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// IPAddress for server connection.
    /// </summary>
    IPAddress IpAddress { get; }

    /// <summary>
    /// Port number for server connection.
    /// </summary>
    int Port { get; }

    /// <summary>
    /// Is running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Send text to one connected client.
    /// </summary>
    /// <param name="clientId">The client id from ClientConnected or ClientDataReceived.</param>
    /// <param name="data">The text to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns><see langword="true"/> if the data was sent.</returns>
    Task<bool> Send(
        Guid clientId,
        string data,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send data to one connected client.
    /// </summary>
    /// <param name="clientId">The client id from ClientConnected or ClientDataReceived.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns><see langword="true"/> if the data was sent.</returns>
    Task<bool> Send(
        Guid clientId,
        byte[] data,
        CancellationToken cancellationToken);
}