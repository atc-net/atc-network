namespace Atc.Network.Helpers;

public static class DnsLookupHelper
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static string? hostname;
    private static IPAddress[]? hostAddresses;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
    public static async Task<string?> GetHostname(
        IPAddress ipAddress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            if (hostname is null &&
                hostAddresses is null)
            {
                hostname = Dns.GetHostName();
                hostAddresses = await Dns.GetHostAddressesAsync(hostname, cancellationToken);
            }

            if (hostAddresses is null)
            {
                return null;
            }

            var hostAddress = hostAddresses.FirstOrDefault(x => x.Equals(ipAddress));
            if (hostAddress is not null)
            {
                return hostname;
            }

            var result = await Dns.GetHostEntryAsync(ipAddress.ToString(), cancellationToken);
            return result.HostName;
        }
        catch
        {
            return null;
        }
        finally
        {
            SyncLock.Release();
        }
    }
}