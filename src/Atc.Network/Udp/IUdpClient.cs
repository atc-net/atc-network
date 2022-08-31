namespace Atc.Network.Udp;

public interface IUdpClient : IDisposable
{
    /// <summary>
    /// Event to raise when connection is established.
    /// </summary>
    event Action? Connected;

    /// <summary>
    /// Event to raise when connection is destroyed.
    /// </summary>
    event Action? Disconnected;

    /// <summary>
    /// Event to raise when connection state is changed.
    /// </summary>
    event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    event Action<byte[]>? DataReceived;

    /// <summary>
    /// Is client connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connect.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task<bool> Connect(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect.
    /// </summary>
    Task Disconnect();

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        string data,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="encoding">The encoding.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        Encoding encoding,
        string data,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        byte[] data,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="terminationType">The terminationType.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        byte[] data,
        TerminationType terminationType,
        CancellationToken cancellationToken);
}