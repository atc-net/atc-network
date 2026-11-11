// ReSharper disable CheckNamespace
namespace Atc.Network;

/// <summary>
/// Event data for a message a TCP server received from one client.
/// </summary>
/// <seealso cref="TcpServerClientEventArgs" />
public class TcpServerDataReceivedEventArgs : TcpServerClientEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TcpServerDataReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="clientId">The id the server gave the client when it connected.</param>
    /// <param name="remoteEndPoint">The client's endpoint.</param>
    /// <param name="data">The received data.</param>
    public TcpServerDataReceivedEventArgs(
        Guid clientId,
        IPEndPoint remoteEndPoint,
        ReadOnlyMemory<byte> data)
        : base(clientId, remoteEndPoint)
    {
        Data = data;
    }

    /// <summary>
    /// Gets the received data.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{base.ToString()}, Length: {Data.Length}";
}