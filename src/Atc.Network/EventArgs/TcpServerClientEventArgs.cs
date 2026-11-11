// ReSharper disable CheckNamespace
namespace Atc.Network;

/// <summary>
/// Event data that identifies one client connected to a TCP server.
/// </summary>
/// <seealso cref="EventArgs" />
public class TcpServerClientEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TcpServerClientEventArgs"/> class.
    /// </summary>
    /// <param name="clientId">The id the server gave the client when it connected.</param>
    /// <param name="remoteEndPoint">The client's endpoint.</param>
    public TcpServerClientEventArgs(
        Guid clientId,
        IPEndPoint remoteEndPoint)
    {
        ArgumentNullException.ThrowIfNull(remoteEndPoint);

        ClientId = clientId;
        RemoteEndPoint = remoteEndPoint;
    }

    /// <summary>
    /// Gets the id the server gave the client when it connected. Use it to send to this client.
    /// </summary>
    public Guid ClientId { get; }

    /// <summary>
    /// Gets the client's endpoint.
    /// </summary>
    public IPEndPoint RemoteEndPoint { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(ClientId)}: {ClientId}, {nameof(RemoteEndPoint)}: {RemoteEndPoint}";
}