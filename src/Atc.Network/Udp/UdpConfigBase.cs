namespace Atc.Network.Udp;

public abstract class UdpConfigBase
{
    /// <summary>
    /// Gets or sets the size of the send buffer in bytes.
    /// </summary>
    /// <returns>
    /// The size of the send buffer, in bytes. The default value is 8192 bytes.
    /// </returns>
    public int SendBufferSize { get; set; } = TcpConstants.DefaultBufferSize;

    /// <summary>
    ///  Gets or sets the size of the receive buffer in bytes.
    /// </summary>
    /// <returns>
    /// The size of the receive buffer, in bytes. The default value is 8192 bytes.
    /// </returns>
    public int ReceiveBufferSize { get; set; } = TcpConstants.DefaultBufferSize;

    /// <summary>
    /// Gets or sets the default encoding.
    /// </summary>
    public Encoding DefaultEncoding { get; set; } = Encoding.ASCII;

    /// <summary>
    /// Gets or sets the TerminationType.
    /// </summary>
    public TerminationType TerminationType { get; set; } = TerminationType.None;
}