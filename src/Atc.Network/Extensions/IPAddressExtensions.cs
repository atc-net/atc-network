// ReSharper disable once CheckNamespace
namespace Atc.Network;

public static class IPAddressExtensions
{
    public static bool IsPublic(
        this IPAddress ipAddress)
        => !IsPrivate(ipAddress);

    /// <summary>
    /// Is IP address in private network scope.
    /// </summary>
    /// <param name="ipAddress">The ip address.</param>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/Reserved_IP_addresses
    /// </remarks>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public static bool IsPrivate(
        this IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var ipAddressAsBytes = ipAddress.GetAddressBytes();
        if (ipAddressAsBytes[0] is not (10 or 100 or 172 or 192 or 198))
        {
            return false;
        }

        var ipAddressAsUnsignedInt = ToUnsignedInt(ipAddress);
        switch (ipAddressAsBytes[0])
        {
            case 10:
            {
                if (ipAddressAsUnsignedInt >= ToUnsignedInt(IPAddress.Parse("10.0.0.0")) &&
                    ipAddressAsUnsignedInt <= ToUnsignedInt(IPAddress.Parse("10.255.255.255")))
                {
                    return true;
                }

                break;
            }

            case 100:
            {
                if (ipAddressAsUnsignedInt >= ToUnsignedInt(IPAddress.Parse("100.64.0.0")) &&
                    ipAddressAsUnsignedInt <= ToUnsignedInt(IPAddress.Parse("100.127.255.255")))
                {
                    return true;
                }

                break;
            }

            case 172:
            {
                if (ipAddressAsUnsignedInt >= ToUnsignedInt(IPAddress.Parse("172.16.0.0")) &&
                    ipAddressAsUnsignedInt <= ToUnsignedInt(IPAddress.Parse("172.31.255.255")))
                {
                    return true;
                }

                break;
            }

            case 192:
            {
                switch (ipAddressAsBytes[1])
                {
                    case 0:
                    {
                        if (ipAddressAsUnsignedInt >= ToUnsignedInt(IPAddress.Parse("192.0.0.0")) &&
                            ipAddressAsUnsignedInt <= ToUnsignedInt(IPAddress.Parse("192.0.0.255")))
                        {
                            return true;
                        }

                        break;
                    }

                    case 168:
                    {
                        if (ipAddressAsUnsignedInt >= ToUnsignedInt(IPAddress.Parse("192.168.0.0")) &&
                            ipAddressAsUnsignedInt <= ToUnsignedInt(IPAddress.Parse("192.168.255.255")))
                        {
                            return true;
                        }

                        break;
                    }
                }

                break;
            }

            case 198:
            {
                if (ipAddressAsUnsignedInt >= ToUnsignedInt(IPAddress.Parse("198.18.0.0")) &&
                    ipAddressAsUnsignedInt <= ToUnsignedInt(IPAddress.Parse("198.19.255.255")))
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