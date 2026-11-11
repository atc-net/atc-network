namespace Atc.Network.Tcp;

/// <summary>
/// Configurations for <see cref="TcpServer"/>.
/// </summary>
public class TcpServerConfig : TcpConfigBase
{
    /// <summary>
    /// Gets or sets the TLS options. When set, every client connection is encrypted with TLS,
    /// using <see cref="SslServerAuthenticationOptions.ServerCertificate"/>.
    /// When <see langword="null"/> (the default), plain TCP is used.
    /// </summary>
    /// <remarks>
    /// A client counts as connected only after its TLS handshake succeeds.
    /// A client that fails the handshake, or does not finish it within 10 seconds, is closed
    /// without raising <see cref="TcpServer.ClientConnected"/>.
    /// </remarks>
    public SslServerAuthenticationOptions? SslOptions { get; set; }
}