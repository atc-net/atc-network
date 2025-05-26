// ReSharper disable MergeIntoLogicalPattern
namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for fetching and parsing ARP (Address Resolution Protocol) table results.
/// </summary>
public static class ArpHelper
{
    /// <summary>
    /// Well-known MAC address for loopback interface.
    /// </summary>
    public const string LoopbackMacAddress = "00-00-00-00-00-00";

    /// <summary>
    /// Type used for loopback interface entries.
    /// </summary>
    public const string LoopbackType = "static";

    private static DateTimeOffset lastLookup = DateTimeOffset.MinValue;
    private static ArpEntity[]? arpEntities;

    /// <summary>
    /// Retrieves the ARP table results, caching them for 90 seconds to limit frequent lookups.
    /// </summary>
    /// <returns>
    /// An array of <see cref="ArpEntity"/> representing the current ARP table entries.
    /// Returns an empty array if no connection is available or if the ARP lookup fails.
    /// </returns>
    /// <remarks>
    /// This method first checks if the results are cached and valid (less than 90 seconds old). If valid, cached results are returned.
    /// Otherwise, it performs a new ARP lookup using the system's 'arp' command. The results are parsed, cached, and then returned.
    /// If there's no network connection, an empty array is returned.
    /// </remarks>
    public static ArpEntity[] GetArpResult()
    {
        var timeSpan = DateTimeOffset.Now - lastLookup;
        if (arpEntities is not null && timeSpan.Seconds < 90)
        {
            return arpEntities;
        }

        if (!NetworkInformationHelper.HasConnection())
        {
            return [];
        }

        lastLookup = DateTimeOffset.Now;
        var process = Process.Start(
            new ProcessStartInfo("arp", "-a")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });

        var output = process?.StandardOutput.ReadToEnd();
        process?.Close();

        if (string.IsNullOrEmpty(output))
        {
            return arpEntities ?? [];
        }

        return ParseArpResult(output).ToArray();
    }

    /// <summary>
    /// Creates an ArpEntity for a loopback address (127.0.0.1).
    /// </summary>
    /// <param name="ipAddress">The loopback IP address.</param>
    /// <returns>An ArpEntity with a standard loopback MAC address (00-00-00-00-00-00).</returns>
    public static ArpEntity GetLoopbackArpEntity(IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        return new ArpEntity(ipAddress, LoopbackMacAddress, LoopbackType);
    }

    /// <summary>
    /// Checks if the provided IP address is a loopback address (127.x.x.x).
    /// </summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <returns>True if the IP address is a loopback address, otherwise false.</returns>
    public static bool IsLoopbackAddress(IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        return IPAddress.IsLoopback(ipAddress);
    }

    /// <summary>
    /// Checks if the provided IP address is the local machine's IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <returns>True if the IP address is the local machine's IP address, otherwise false.</returns>
    public static bool IsLocalMachineAddress(IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var localAddress = IPv4AddressHelper.GetLocalAddress();
        return localAddress != null && ipAddress.Equals(localAddress);
    }

    /// <summary>
    /// Creates an ArpEntity for the local machine's IP address.
    /// </summary>
    /// <param name="ipAddress">The local machine's IP address.</param>
    /// <returns>An ArpEntity for the local machine with a standard MAC address.</returns>
    public static ArpEntity GetLocalMachineArpEntity(IPAddress ipAddress)
    {
        // Get the MAC address of the local machine's network adapter
        var macAddress = GetLocalMacAddress() ?? LoopbackMacAddress;
        return new ArpEntity(ipAddress, macAddress, LoopbackType);
    }

    /// <summary>
    /// Gets the MAC address of the local machine's main network adapter.
    /// </summary>
    /// <returns>The MAC address of the local machine's network adapter, or null if not found.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
    private static string? GetLocalMacAddress()
    {
        try
        {
            // Get the network adapters excluding virtual, loopback, and disconnected ones
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                            (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                             ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
                            !ni.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase) &&
                            !ni.Description.Contains("Pseudo", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Try to find the main network adapter - the one that's being used for internet access
            var mainAdapter = networkInterfaces.FirstOrDefault();

            if (mainAdapter is not null)
            {
                var macBytes = mainAdapter.GetPhysicalAddress().GetAddressBytes();
                if (macBytes.Length > 0)
                {
                    return string.Join("-", macBytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
                }
            }
        }
        catch
        {
            // Silently fail and return null if any errors occur
        }

        return null;
    }

    /// <summary>
    /// Parses the output from the ARP command into a collection of <see cref="ArpEntity"/>.
    /// </summary>
    /// <param name="output">The raw string output from the ARP command.</param>
    /// <returns>
    /// An enumerable of <see cref="ArpEntity"/> parsed from the command output.
    /// </returns>
    /// <remarks>
    /// This method splits the command output into lines, then splits each line into parts based on whitespace.
    /// It expects each line to have exactly three parts: IP address, physical address, and type. Lines not matching this format are ignored.
    /// Parsed entries are cached for use by subsequent calls to GetArpResult within the cache period.
    /// </remarks>
    private static IEnumerable<ArpEntity> ParseArpResult(string output)
    {
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var result =
            from line in lines
            select Regex.Split(line, @"\s+", RegexOptions.None, TimeSpan.FromSeconds(1))
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList()
            into items
            where items.Count == 3
            select new ArpEntity(
                IPAddress.Parse(items[0]),
                items[1],
                items[2]);

        arpEntities = result.ToArray();
        return arpEntities;
    }
}