namespace Atc.Network.Udp;

/// <summary>
/// This is a interface for <see cref="UdpServer"/>.
/// </summary>
public interface IUdpServer : IHostedService, IDisposable
{
    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    event Action<byte[]>? DataReceived;

    /// <summary>
    /// Event to raise when a datagram is received, with the endpoint that sent it.
    /// </summary>
    event EventHandler<UdpDatagramReceivedEventArgs>? DatagramReceived;

    /// <summary>
    /// Event to raise when an error was handled and logged, so the server keeps running:
    /// an event handler that threw, or a failed receive. A handler of this event that throws is only logged.
    /// </summary>
    event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// Is running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="recipient">The recipient endpoint.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        EndPoint recipient,
        string data,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="recipient">The recipient endpoint.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        EndPoint recipient,
        Encoding encoding,
        string data,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="recipient">The recipient endpoint.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        EndPoint recipient,
        byte[] data,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="recipient">The recipient endpoint.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="terminationType">The terminationType.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Send(
        EndPoint recipient,
        byte[] data,
        TerminationType terminationType,
        CancellationToken cancellationToken);
}