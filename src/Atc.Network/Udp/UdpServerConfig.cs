namespace Atc.Network.Udp;

/// <summary>
/// Configurations for <see cref="UdpServer"/>.
/// </summary>
public class UdpServerConfig : UdpConfigBase
{
    /// <summary>
    /// Gets or sets the echo on received data.
    /// </summary>
    public bool EchoOnReceivedData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the server answers a datagram that starts with "ping" with "pong".
    /// </summary>
    public bool RespondToPing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the server, bound to an IPv6 address such as <see cref="IPAddress.IPv6Any"/>,
    /// also accepts IPv4 datagrams on the same socket (dual mode). IPv4 senders then appear as IPv4 addresses.
    /// The default is <see langword="false"/>. It needs an IPv6 address.
    /// </summary>
    public bool DualMode { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => $"{base.ToString()}, {nameof(EchoOnReceivedData)}: {EchoOnReceivedData}, {nameof(RespondToPing)}: {RespondToPing}, {nameof(DualMode)}: {DualMode}";
}