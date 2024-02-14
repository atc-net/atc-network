namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for validating OPC UA (Open Platform Communications Unified Architecture) addresses.
/// </summary>
public static class OpcUaAddressHelper
{
    private const int MinPortNumber = 1;
    private const int MaxPortNumber = ushort.MaxValue;

    /// <summary>
    /// Validates the format of a given OPC UA address specified as a URL string.
    /// </summary>
    /// <param name="url">The OPC UA address to validate.</param>
    /// <param name="restrictToIp4Address">Indicates whether to restrict validation to IPv4 addresses only.</param>
    /// <returns>
    /// True if the address is a valid OPC UA address; otherwise, false.
    /// </returns>
    /// <remarks>
    /// This method checks if the URL is an absolute URI with the scheme "opc.tcp".
    /// If <paramref name="restrictToIp4Address"/> is true, it further validates that the host part
    /// of the URI is a valid IPv4 address.
    /// </remarks>
    public static bool IsValid(
        string url,
        bool restrictToIp4Address = false)
    {
        ArgumentNullException.ThrowIfNull(url);

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               IsValid(uriResult, restrictToIp4Address);
    }

    /// <summary>
    /// Validates the format of a given OPC UA address specified as a Uri object.
    /// </summary>
    /// <param name="uri">The Uri object representing the OPC UA address to validate.</param>
    /// <param name="restrictToIp4Address">Indicates whether to restrict validation to IPv4 addresses only.</param>
    /// <returns>
    /// True if the address is a valid OPC UA address; otherwise, false.
    /// </returns>
    /// <remarks>
    /// Validates that the Uri uses the "opc.tcp" scheme and, optionally, that its host is a valid IPv4 address
    /// if <paramref name="restrictToIp4Address"/> is true. Also checks that the port number is within the valid range.
    /// </remarks>
    public static bool IsValid(
        Uri uri,
        bool restrictToIp4Address = false)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (!string.Equals(uri.Scheme, "opc.tcp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (restrictToIp4Address)
        {
            if (!IPv4AddressHelper.IsValid(uri.Host))
            {
                return false;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(uri.Host))
            {
                return false;
            }
        }

        return uri.Port is >= MinPortNumber and <= MaxPortNumber;
    }
}