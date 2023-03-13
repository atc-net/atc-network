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

    /// <inheritdoc />
    public override string ToString()
        => $"{base.ToString()}, {nameof(EchoOnReceivedData)}: {EchoOnReceivedData}";
}