// ReSharper disable InconsistentNaming
namespace Atc.Network.Tcp;

/// <summary>
/// This is a interface for <see cref="TcpClient"/>.
/// </summary>
public interface ITcpClient
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
    /// Event to raise when no data received.
    /// </summary>
    event Action? NoDataReceived;

    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    event Action<byte[]>? DataReceived;

    /// <summary>
    /// IPAddress or hostname for server connection.
    /// </summary>
    string IPAddressOrHostname { get; }

    /// <summary>
    /// Port number for server connection.
    /// </summary>
    int Port { get; }

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
    /// <remarks>
    /// Data will be encoded as client-config default encoding.
    /// </remarks>
    Task Send(
        string data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="encoding">The encoding.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        Encoding encoding,
        string data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <remarks>
    /// TerminationType is resolved from TcpClientConfig.
    /// </remarks>
    Task Send(
        byte[] data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="terminationType">The terminationType.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        byte[] data,
        TerminationType terminationType,
        CancellationToken cancellationToken = default);
}