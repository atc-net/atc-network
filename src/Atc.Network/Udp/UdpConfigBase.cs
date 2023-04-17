namespace Atc.Network.Udp;

/// <summary>
/// Base configurations for <see cref="UdpClient"/> and <see cref="UdpServer"/>.
/// </summary>
public abstract class UdpConfigBase
{
    /// <summary>
    /// Gets or sets the send timeout value in milliseconds.
    /// </summary>
    /// <returns>
    /// The send time-out value, in milliseconds. The default is 0;
    /// </returns>
    public int SendTimeout { get; set; } = UdpConstants.DefaultSendReceiveTimeout;

    /// <summary>
    /// Gets or sets the size of the send buffer in bytes.
    /// </summary>
    /// <returns>
    /// The size of the send buffer, in bytes. The default value is 8192 bytes.
    /// </returns>
    public int SendBufferSize { get; set; } = UdpConstants.DefaultBufferSize;

    /// <summary>
    /// Gets or sets the receive timeout value in milliseconds.
    /// </summary>
    /// <returns>
    /// The receive time-out value, in milliseconds. The default is 0;
    /// </returns>
    public int ReceiveTimeout { get; set; } = UdpConstants.DefaultSendReceiveTimeout;

    /// <summary>
    ///  Gets or sets the size of the receive buffer in bytes.
    /// </summary>
    /// <returns>
    /// The size of the receive buffer, in bytes. The default value is 8192 bytes.
    /// </returns>
    public int ReceiveBufferSize { get; set; } = UdpConstants.DefaultBufferSize;

    /// <summary>
    /// Gets or sets the default encoding.
    /// </summary>
    public Encoding DefaultEncoding { get; set; } = Encoding.ASCII;

    /// <summary>
    /// Gets or sets the TerminationType.
    /// </summary>
    public TerminationType TerminationType { get; set; } = TerminationType.None;

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(SendTimeout)}: {SendTimeout}, {nameof(SendBufferSize)}: {SendBufferSize}, {nameof(ReceiveTimeout)}: {ReceiveTimeout}, {nameof(ReceiveBufferSize)}: {ReceiveBufferSize}, {nameof(DefaultEncoding)}: {DefaultEncoding}, {nameof(TerminationType)}: {TerminationType}";
}