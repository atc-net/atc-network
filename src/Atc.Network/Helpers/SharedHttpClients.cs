namespace Atc.Network.Helpers;

/// <summary>
/// Shared <see cref="HttpClient"/> instances. Creating a client per request leaves sockets in
/// TIME_WAIT and can exhaust them during wide scans, so the library reuses these two.
/// </summary>
internal static class SharedHttpClients
{
    /// <summary>
    /// Gets the client for calls to web APIs (MAC vendor lookup, world time).
    /// Pooled connections are recycled every few minutes, so DNS changes are picked up.
    /// </summary>
    public static HttpClient Api { get; } = new(
        new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        });

    /// <summary>
    /// Gets the client for HTTP/HTTPS reachability probes.
    /// It accepts any server certificate, because only reachability is checked and no data is sent,
    /// and it has no client-wide timeout: each probe passes its own timeout as a cancellation token.
    /// Probe requests should send "Connection: close", so no idle connections stay open to scanned devices.
    /// </summary>
    [SuppressMessage("Security", "S4830:Server certificates should be verified during SSL/TLS connections", Justification = "OK - only reachability is checked and no data is sent.")]
    [SuppressMessage("Security", "MA0039:Do not write your own certificate validation method", Justification = "OK - only reachability is checked and no data is sent.")]
    [SuppressMessage("Security", "CA5359:Do not disable certificate validation", Justification = "OK - only reachability is checked and no data is sent.")]
    public static HttpClient Probe { get; } = new(
        new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (_, _, _, _) => true,
            },
        })
    {
        Timeout = Timeout.InfiniteTimeSpan,
    };
}