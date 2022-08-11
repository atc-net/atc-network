// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable StringLiteralTypo
namespace Atc.Network.Helpers;

public static class MacAddressVendorLookupHelper
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static readonly Uri MacVendorsApiUrl = new("http://api.macvendors.com/");
    private static DateTimeOffset lastLookup = DateTimeOffset.MinValue;

    public static async Task<string> LookupVendorNameFromMacAddress(
        string macAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(macAddress);

        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            macAddress = macAddress.ToUpper(GlobalizationConstants.EnglishCultureInfo);
            var tempPath = Path.Combine(Path.GetTempPath(), "AtcCache");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var cacheFile = Path.Combine(tempPath, "macvendors.txt");
            var cacheFileLines = new List<string>();
            if (File.Exists(cacheFile))
            {
                cacheFileLines = (await File.ReadAllLinesAsync(cacheFile, cancellationToken)).ToList();
                foreach (var cacheFileLine in cacheFileLines)
                {
                    if (!cacheFileLine.StartsWith(macAddress, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var sa = cacheFileLine.Split('=');
                    if (sa.Length == 2)
                    {
                        return sa[1];
                    }
                }
            }

            var vendorName = await CallMacVendorAsync(macAddress);

            cacheFileLines.Add($"{macAddress}={vendorName}");
            await File.WriteAllLinesAsync(cacheFile, cacheFileLines, cancellationToken);

            return vendorName;
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static async Task<string> CallMacVendorAsync(
        string macAddress)
    {
        var timeSpan = DateTimeOffset.Now - lastLookup;
        if (timeSpan.TotalMilliseconds < 1200)
        {
            await Task.Delay(1200 - (int)timeSpan.TotalMilliseconds);
        }

        lastLookup = DateTimeOffset.Now;

        var uri = new Uri(MacVendorsApiUrl.AbsoluteUri + WebUtility.UrlEncode(macAddress));

        using var httpClient = new HttpClient();
        return await httpClient.GetStringAsync(uri);
    }
}