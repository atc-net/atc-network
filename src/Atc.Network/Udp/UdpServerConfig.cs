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

    /// <inheritdoc />
    public override string ToString()
        => $"{base.ToString()}, {nameof(EchoOnReceivedData)}: {EchoOnReceivedData}, {nameof(RespondToPing)}: {RespondToPing}";
}