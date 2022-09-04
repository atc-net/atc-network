namespace Atc.Network.Udp;

public class UdpClientConfig : UdpConfigBase
{
    /// <summary>
    /// Gets or sets the IP protection level on the socket.
    /// </summary>
    /// <remarks>
    /// Only used for Windows OS.
    /// </remarks>
    public IPProtectionLevel IPProtectionLevel { get; set; } = IPProtectionLevel.Unrestricted;
}