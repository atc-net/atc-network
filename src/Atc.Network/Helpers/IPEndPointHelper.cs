namespace Atc.Network.Helpers;

/// <summary>
/// Helpers for IPv6 and dual-mode sockets.
/// </summary>
internal static class IPEndPointHelper
{
    /// <summary>
    /// Returns the endpoint with an IPv4 address when a dual-mode socket reports an IPv4 peer
    /// as an IPv4-mapped IPv6 address (::ffff:a.b.c.d); otherwise the endpoint itself.
    /// </summary>
    /// <param name="endPoint">The endpoint.</param>
    /// <returns>The endpoint, with a plain IPv4 address for IPv4 peers.</returns>
    public static IPEndPoint WithoutIPv4Mapping(IPEndPoint endPoint)
        => endPoint.Address.IsIPv4MappedToIPv6
            ? new IPEndPoint(endPoint.Address.MapToIPv4(), endPoint.Port)
            : endPoint;

    /// <summary>
    /// Throws when dual mode is asked for on an address that is not IPv6.
    /// </summary>
    /// <param name="dualMode">Whether dual mode is on.</param>
    /// <param name="ipAddress">The address the server binds to.</param>
    /// <param name="paramName">The parameter name for the exception.</param>
    public static void ThrowIfDualModeWithoutIPv6(
        bool dualMode,
        IPAddress ipAddress,
        string paramName)
    {
        if (dualMode && ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
        {
            throw new ArgumentException(
                $"DualMode needs an IPv6 address, such as IPAddress.IPv6Any, but got {ipAddress}.",
                paramName);
        }
    }

    /// <summary>
    /// Returns an address and port to use in a URL, with brackets around an IPv6 address: [::1]:80.
    /// </summary>
    /// <param name="ipAddress">The address.</param>
    /// <param name="port">The port.</param>
    /// <returns>The host and port for a URL.</returns>
    public static string ToUrlAuthority(
        IPAddress ipAddress,
        int port)
    {
        if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return $"{ipAddress}:{port}";
        }

        // A scope id (fe80::1%12) is written as %25 in a URL.
        var address = ipAddress.ToString().Replace("%", "%25", StringComparison.Ordinal);
        return $"[{address}]:{port}";
    }
}