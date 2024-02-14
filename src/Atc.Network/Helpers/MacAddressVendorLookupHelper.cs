// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable StringLiteralTypo
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.Network.Helpers;

public static class MacAddressVendorLookupHelper
{
    private const string NotFound = "NotFound";
    private const string AtcCacheFolder = "AtcCache";
    private const string AtcCacheFile = "macvendors.txt";

    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static readonly Uri MacVendorsApiUrl = new("http://api.macvendors.com/");
    private static DateTimeOffset lastLookup = DateTimeOffset.MinValue;
    private static List<string> cacheFileLines = new();

    public static async Task<string?> LookupVendorNameFromMacAddress(
        string macAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(macAddress);

        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            macAddress = macAddress.ToUpper(GlobalizationConstants.EnglishCultureInfo);
            var cacheVendorName = GetVendorFromCacheFileLines(macAddress);
            if (!string.IsNullOrEmpty(cacheVendorName))
            {
                return NotFound.Equals(cacheVendorName, StringComparison.Ordinal)
                    ? null
                    : cacheVendorName;
            }

            var tempPath = Path.Combine(Path.GetTempPath(), AtcCacheFolder);
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var cacheFile = Path.Combine(tempPath, AtcCacheFile);
            if (File.Exists(cacheFile))
            {
                cacheFileLines = (await File.ReadAllLinesAsync(cacheFile, cancellationToken)).ToList();
                cacheVendorName = GetVendorFromCacheFileLines(macAddress);
                if (!string.IsNullOrEmpty(cacheVendorName))
                {
                    return NotFound.Equals(cacheVendorName, StringComparison.Ordinal)
                        ? null
                        : cacheVendorName;
                }
            }

            var vendorName = await CallMacVendorAsync(macAddress, cancellationToken);

            cacheFileLines.Add($"{macAddress}={vendorName}");
            await File.WriteAllLinesAsync(cacheFile, cacheFileLines, cancellationToken);

            return vendorName;
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static string? GetVendorFromCacheFileLines(
        string macAddress)
    {
        if (!cacheFileLines.Any())
        {
            return null;
        }

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

        return null;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
    private static async Task<string?> CallMacVendorAsync(
        string macAddress,
        CancellationToken cancellationToken)
    {
        var timeSpan = DateTimeOffset.Now - lastLookup;
        if (timeSpan.TotalMilliseconds < 1200)
        {
            await Task.Delay(1200 - (int)timeSpan.TotalMilliseconds, cancellationToken);
        }

        lastLookup = DateTimeOffset.Now;

        var uri = new Uri(MacVendorsApiUrl.AbsoluteUri + WebUtility.UrlEncode(macAddress));

        try
        {
            using var httpClient = new HttpClient();
            var httpResponseMessage = await httpClient.GetAsync(uri, cancellationToken);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            }

            return httpResponseMessage.StatusCode == HttpStatusCode.NotFound
                ? NotFound
                : null;
        }
        catch
        {
            return null;
        }
    }
}