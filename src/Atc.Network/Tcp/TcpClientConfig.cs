namespace Atc.Network.Tcp;

public class TcpClientConfig
{
    /// <summary>
    /// Gets or sets the send timeout value of the connection in milliseconds.
    /// </summary>
    /// <returns>
    /// The connect time-out value, in milliseconds. The default is 10000 (10 sec);
    /// </returns>
    public int ConnectTimeout { get; set; } = TcpConstants.DefaultConnectTimeout;

    /// <summary>
    /// Gets or sets the send timeout value of the connection in milliseconds.
    /// </summary>
    /// <returns>
    /// The send time-out value, in milliseconds. The default is 0;
    /// </returns>
    public int SendTimeout { get; set; } = TcpConstants.DefaultSendReceiveTimeout;

    /// <summary>
    /// Gets or sets the size of the send buffer in bytes.
    /// </summary>
    /// <returns>
    /// The size of the send buffer, in bytes. The default value is 8192 bytes.
    /// </returns>
    public int SendBufferSize { get; set; } = TcpConstants.DefaultBufferSize;

    /// <summary>
    /// Gets or sets the receive timeout value of the connection in milliseconds.
    /// </summary>
    /// <returns>
    /// The receive time-out value, in milliseconds. The default is 0;
    /// </returns>
    public int ReceiveTimeout { get; set; } = TcpConstants.DefaultSendReceiveTimeout;

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