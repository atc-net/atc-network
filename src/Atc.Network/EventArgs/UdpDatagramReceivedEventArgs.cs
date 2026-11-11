// ReSharper disable CheckNamespace
namespace Atc.Network;

/// <summary>
/// Event data for a datagram received by a UDP server, including who sent it.
/// </summary>
/// <seealso cref="EventArgs" />
public class UdpDatagramReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UdpDatagramReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="data">The received data.</param>
    /// <param name="remoteEndPoint">The endpoint that sent the datagram.</param>
    public UdpDatagramReceivedEventArgs(
        ReadOnlyMemory<byte> data,
        IPEndPoint remoteEndPoint)
    {
        ArgumentNullException.ThrowIfNull(remoteEndPoint);

        Data = data;
        RemoteEndPoint = remoteEndPoint;
    }

    /// <summary>
    /// Gets the received data.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    /// Gets the endpoint that sent the datagram. Use it as the recipient to reply.
    /// </summary>
    public IPEndPoint RemoteEndPoint { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(RemoteEndPoint)}: {RemoteEndPoint}, Length: {Data.Length}";
}