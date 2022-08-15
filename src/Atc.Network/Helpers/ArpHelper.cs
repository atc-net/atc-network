namespace Atc.Network.Helpers;

public static class ArpHelper
{
    private static DateTimeOffset lastLookup = DateTimeOffset.MinValue;
    private static ArpEntity[]? arpEntities;

    public static ArpEntity[] GetArpResult()
    {
        var timeSpan = DateTimeOffset.Now - lastLookup;
        if (arpEntities is not null && timeSpan.Seconds < 90)
        {
            return arpEntities;
        }

        if (!NetworkInformationHelper.HasConnection())
        {
            return Array.Empty<ArpEntity>();
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
            return arpEntities ?? Array.Empty<ArpEntity>();
        }

        return ParseArpResult(output).ToArray();
    }

    private static IEnumerable<ArpEntity> ParseArpResult(
        string output)
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