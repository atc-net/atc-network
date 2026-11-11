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

    /// <summary>
    /// Gets or sets a value indicating whether the server, bound to an IPv6 address such as <see cref="IPAddress.IPv6Any"/>,
    /// also accepts IPv4 clients on the same socket (dual mode). IPv4 senders then appear as IPv4 addresses.
    /// The default is <see langword="false"/>. It needs an IPv6 address.
    /// </summary>
    public bool DualMode { get; set; }
}