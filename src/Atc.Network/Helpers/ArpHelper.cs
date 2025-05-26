namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for fetching and parsing ARP (Address Resolution Protocol) table results.
/// </summary>
public static class ArpHelper
{
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