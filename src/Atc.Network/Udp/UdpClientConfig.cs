// ReSharper disable InconsistentNaming
namespace Atc.Network.Udp;

/// <summary>
/// Configurations for <see cref="UdpClient"/>.
/// </summary>
public class UdpClientConfig : UdpConfigBase
{
    /// <summary>
    /// Gets or sets the IP protection level on the socket.
    /// </summary>
    /// <remarks>
    /// Only used for Windows OS.
    /// </remarks>
    public IPProtectionLevel IPProtectionLevel { get; set; } = IPProtectionLevel.Unrestricted;

    /// <inheritdoc />
    public override string ToString()
        => $"{base.ToString()}, {nameof(IPProtectionLevel)}: {IPProtectionLevel}";
}