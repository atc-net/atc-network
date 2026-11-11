namespace Atc.Network.Tcp;

/// <summary>
/// Base configurations for <see cref="TcpClient"/>.
/// </summary>
public class TcpClientConfig
{
    /// <summary>
    /// Gets or sets the connect timeout value of the connection in milliseconds.
    /// </summary>
    /// <returns>
    /// The connect time-out value, in milliseconds. The default is 10000 (10 sec);
    /// </returns>
    public int ConnectTimeout { get; set; } = TcpConstants.DefaultConnectTimeout;

    /// <summary>
    /// Gets or sets the send timeout value in milliseconds.
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
    /// Gets or sets the receive timeout value in milliseconds.
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

    /// <summary>
    /// Gets or sets the TLS options. When set, the connection is encrypted with TLS
    /// and the server is authenticated before the client counts as connected.
    /// When <see langword="null"/> (the default), plain TCP is used.
    /// </summary>
    /// <remarks>
    /// When <see cref="SslClientAuthenticationOptions.TargetHost"/> is empty, the hostname or IP address
    /// the client connects to is used. The TLS handshake must finish within <see cref="ConnectTimeout"/>.
    /// </remarks>
    public SslClientAuthenticationOptions? SslOptions { get; set; }

    /// <summary>
    /// Gets or sets how the received byte stream is split into messages.
    /// The default, <see cref="TcpMessageFraming.None"/>, raises <c>DataReceived</c> once per read.
    /// </summary>
    public TcpMessageFraming MessageFraming { get; set; } = TcpMessageFraming.None;

    /// <summary>
    /// Gets or sets the largest message, in bytes, that <see cref="MessageFraming"/> accepts.
    /// A longer message is reported as a receive error and the connection is closed.
    /// The default is 16 MB.
    /// </summary>
    public int MaxMessageSize { get; set; } = TcpConstants.DefaultMaxMessageSize;

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(ConnectTimeout)}: {ConnectTimeout}, {nameof(SendTimeout)}: {SendTimeout}, {nameof(SendBufferSize)}: {SendBufferSize}, {nameof(ReceiveTimeout)}: {ReceiveTimeout}, {nameof(ReceiveBufferSize)}: {ReceiveBufferSize}, {nameof(DefaultEncoding)}: {DefaultEncoding}, {nameof(TerminationType)}: {TerminationType}, {nameof(SslOptions)}: {(SslOptions is null ? "None" : "Set")}, {nameof(MessageFraming)}: {MessageFraming}, {nameof(MaxMessageSize)}: {MaxMessageSize}";
}