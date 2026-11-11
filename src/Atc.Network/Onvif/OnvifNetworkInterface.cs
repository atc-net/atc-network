namespace Atc.Network.Onvif;

/// <summary>
/// A network interface of an ONVIF device (GetNetworkInterfaces).
/// </summary>
/// <param name="Token">The interface token.</param>
/// <param name="Name">The interface name, such as "eth0".</param>
/// <param name="Enabled">Whether the interface is enabled.</param>
/// <param name="MacAddress">The MAC address, as the device wrote it.</param>
/// <param name="IPv4Addresses">The IPv4 addresses, manual or from DHCP.</param>
/// <param name="IsWireless">Whether it is a Wi-Fi interface (IEEE 802.11).</param>
public sealed record OnvifNetworkInterface(
    string Token,
    string? Name,
    bool Enabled,
    string? MacAddress,
    IReadOnlyList<IPAddress> IPv4Addresses,
    bool IsWireless);