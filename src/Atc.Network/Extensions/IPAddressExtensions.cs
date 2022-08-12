// ReSharper disable once CheckNamespace
namespace Atc.Network;

public static class IPAddressExtensions
{
    public static bool IsPublic(
        this IPAddress ipAddress)
        => !IsPrivate(ipAddress);

    public static bool IsPrivate(
        this IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var ipAddressAsBytes = ipAddress.GetAddressBytes();
        if (ipAddressAsBytes[0] is not (10 or 172 or 192))
        {
            return false;
        }

        var ipAddressAsUnsignedInt = ToUnsignedInt(ipAddress);
        switch (ipAddressAsBytes[0])
        {
            case 10:
            {
                var a = ToUnsignedInt(IPAddress.Parse("10.0.0.0"));
                var b = ToUnsignedInt(IPAddress.Parse("10.255.255.255"));
                if (ipAddressAsUnsignedInt >= a && ipAddressAsUnsignedInt <= b)
                {
                    return true;
                }

                break;
            }

            case 172:
            {
                var a = ToUnsignedInt(IPAddress.Parse("172.16.0.0"));
                var b = ToUnsignedInt(IPAddress.Parse("172.31.255.255"));
                if (ipAddressAsUnsignedInt >= a && ipAddressAsUnsignedInt <= b)
                {
                    return true;
                }

                break;
            }

            case 192:
            {
                var a = ToUnsignedInt(IPAddress.Parse("192.168.0.0"));
                var b = ToUnsignedInt(IPAddress.Parse("192.168.255.255"));
                if (ipAddressAsUnsignedInt >= a && ipAddressAsUnsignedInt <= b)
                {
                    return true;
                }

                break;
            }
        }

        return false;
    }

    public static uint ToUnsignedInt(
        this IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var b = ipAddress.GetAddressBytes();
        Array.Reverse(b);
        return BitConverter.ToUInt32(b);
    }

    public static bool IsInRange(
        this IPAddress ipAddress,
        string cidrNotation)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
        ArgumentNullException.ThrowIfNull(cidrNotation);

        var sa = cidrNotation.Split('/');
        if (sa.Length != 2)
        {
            throw new ArgumentException("Invalid CIDR notation", nameof(cidrNotation));
        }

        var network = IPAddress.Parse(sa[0]);
        var cidr = byte.Parse(sa[1], GlobalizationConstants.EnglishCultureInfo);
        var ipAddressAsBytes = BitConverter.ToInt32(ipAddress.GetAddressBytes());
        var networkAsBytes = BitConverter.ToInt32(network.GetAddressBytes());
        var calc = IPAddress.HostToNetworkOrder(-1 << (32 - cidr));

        return (ipAddressAsBytes & calc) == (networkAsBytes & calc);
    }
}