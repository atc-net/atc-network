namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for performing DNS lookups.
/// </summary>
public static class DnsLookupHelper
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static string? hostname;
    private static IPAddress[]? hostAddresses;

    /// <summary>
    /// Resolves the hostname for a given IP address asynchronously.
    /// </summary>
    /// <param name="ipAddress">The IP address to resolve the hostname for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// The hostname associated with the specified IP address or null if the lookup fails or the
    /// address is a private IP address for which the local hostname cannot be resolved.
    /// </returns>
    /// <remarks>
    /// This method uses a SemaphoreSlim to ensure thread-safe access to the hostname and hostAddresses static fields.
    /// It first checks if the IP address is a private address. If so, and if the hostname and hostAddresses have not
    /// been previously set, it attempts to set them by resolving the local machine's hostname and IP addresses.
    /// For public IP addresses, it performs a DNS lookup to resolve the hostname.
    /// This method suppresses all exceptions, returning null in case of any errors or if the operation is canceled.
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
    public static async Task<string?> GetHostname(
        IPAddress ipAddress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            if (ipAddress.IsPrivate())
            {
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
                return hostAddress is null
                    ? null
                    : hostname;
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