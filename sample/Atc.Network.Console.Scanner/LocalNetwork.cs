namespace Atc.Network.Console.Scanner;

/// <summary>
/// An IPv4 network this machine is on, from one of its network adapters.
/// </summary>
/// <param name="InterfaceName">The adapter's name, such as "Ethernet" or "Wi-Fi".</param>
/// <param name="Address">This machine's address on the network.</param>
/// <param name="PrefixLength">The network's prefix length, such as 24 for 255.255.255.0.</param>
public sealed record LocalNetwork(
    string InterfaceName,
    IPAddress Address,
    int PrefixLength)
{
    /// <summary>
    /// The largest network scanned as a whole. Bigger networks (such as a /16 with 65,536 addresses) are scanned
    /// as the /24 around this machine's address.
    /// </summary>
    public const int SmallestScannedPrefixLength = 22;

    /// <summary>
    /// Gets the prefix length that is scanned.
    /// </summary>
    public int ScannedPrefixLength
        => PrefixLength < SmallestScannedPrefixLength ? 24 : PrefixLength;

    /// <summary>
    /// Gets the first and last address to scan.
    /// </summary>
    public (IPAddress StartIpAddress, IPAddress EndIpAddress) Range
        => IPv4AddressHelper.GetFirstAndLastAddressInRange(Address, ScannedPrefixLength);

    /// <summary>
    /// Returns the IPv4 networks of the adapters that are up, without loopback and link-local (169.254.x.x) addresses.
    /// </summary>
    /// <returns>The networks.</returns>
    public static IReadOnlyList<LocalNetwork> GetAll()
        => NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(x => x.OperationalStatus == OperationalStatus.Up &&
                        x.NetworkInterfaceType is not (NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel))
            .SelectMany(x => x.GetIPProperties().UnicastAddresses
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork &&
                            a.Address.GetAddressBytes()[0] != 169)
                .Select(a => new LocalNetwork(x.Name, a.Address, a.PrefixLength)))
            .ToList();

    /// <inheritdoc />
    public override string ToString()
    {
        var (start, end) = Range;
        var note = ScannedPrefixLength == PrefixLength ? string.Empty : $" (the /{ScannedPrefixLength} of a /{PrefixLength})";
        return $"{start} - {end}  {InterfaceName}, this machine is {Address}{note}";
    }
}