namespace Atc.Network.Helpers;

public static class OpcUaAddressHelper
{
    public static bool IsValid(
        string url,
        bool restrictToIp4Address = false)
    {
        ArgumentNullException.ThrowIfNull(url);

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               IsValid(uriResult, restrictToIp4Address);
    }

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

        return uri.Port is >= 1 and <= ushort.MaxValue;
    }
}