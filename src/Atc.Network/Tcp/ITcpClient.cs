// ReSharper disable InconsistentNaming
namespace Atc.Network.Tcp;

/// <summary>
/// This is a interface for <see cref="TcpClient"/>.
/// </summary>
public interface ITcpClient : IDisposable
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
    /// Event to raise when an error was handled and logged, so the client keeps running:
    /// an event handler that threw, or a failed receive. A handler of this event that throws is only logged.
    /// </summary>
    event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

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
    Task<bool> Connect(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect.
    /// </summary>
    Task Disconnect();

    /// <summary>
    /// Reads received data as an async stream, as an alternative to <see cref="DataReceived"/>: one item per
    /// <see cref="DataReceived"/> event, in the same order. The stream ends when the cancellationToken is cancelled
    /// or the client is disposed; it continues across reconnects.
    /// </summary>
    /// <remarks>
    /// Data received before the enumeration starts is not included. Each enumeration buffers up to 256 items;
    /// when it falls behind, the client waits before reading more, so a slow reader slows down the sender
    /// instead of using more memory.
    /// </remarks>
    /// <param name="cancellationToken">Ends the stream.</param>
    /// <returns>The received data.</returns>
    IAsyncEnumerable<ReadOnlyMemory<byte>> ReadAllAsync(
        CancellationToken cancellationToken = default);

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