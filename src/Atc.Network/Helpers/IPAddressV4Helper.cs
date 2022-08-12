// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
namespace Atc.Network.Helpers;

public static class IPAddressV4Helper
{
    public static (bool IsValid, string? ErrorMessage) ValidateAddresses(
        IPAddress startIpAddress,
        IPAddress endIpAddress)
    {
        ArgumentNullException.ThrowIfNull(startIpAddress);
        ArgumentNullException.ThrowIfNull(endIpAddress);

        if (startIpAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            return (false, $"{nameof(startIpAddress)} is not IPv4");
        }

        if (endIpAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            return (false, $"{nameof(endIpAddress)} is not IPv4");
        }

        var a = BitConverter.ToUInt32(startIpAddress.GetAddressBytes());
        var b = BitConverter.ToUInt32(endIpAddress.GetAddressBytes());

        return a > b
            ? (false, $"{nameof(startIpAddress)} is higher than {nameof(endIpAddress)}")
            : (true, null);
    }

    public static IPAddress? GetLocalAddress()
    {
        var hostName = Dns.GetHostName();
        var host = Dns.GetHostEntry(hostName);
        return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }

    public static IPAddress[] GetAddressesInRange(
        IPAddress startIpAddress,
        IPAddress endIpAddress)
    {
        var validationResult = ValidateAddresses(startIpAddress, endIpAddress);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ErrorMessage);
        }

        var startIpAddressAsBytes = startIpAddress.GetAddressBytes();
        var endIpAddressAsBytes = endIpAddress.GetAddressBytes();

        Array.Reverse(startIpAddressAsBytes);
        Array.Reverse(endIpAddressAsBytes);

        var startIpAddressAsUnsignedInt = BitConverter.ToUInt32(startIpAddressAsBytes);
        var endIpAddressAsUnsignedInt = BitConverter.ToUInt32(endIpAddressAsBytes);

        var list = new List<IPAddress>();
        for (var i = startIpAddressAsUnsignedInt; i <= endIpAddressAsUnsignedInt; i++)
        {
            var bytes = BitConverter.GetBytes(i);
            Array.Reverse(bytes);
            list.Add(new IPAddress(bytes));
        }

        return list.ToArray();
    }

    public static IPAddress[] GetAddressesInRange(
        IPAddress ipAddress,
        int cidrLength)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var (startIpAddress, endIpAddress) = GetStartAndEndAddressesInRange(ipAddress, cidrLength);
        return GetAddressesInRange(startIpAddress, endIpAddress);
    }

    public static (IPAddress StartIpAddress, IPAddress EndIpAddress) GetStartAndEndAddressesInRange(
        IPAddress ipAddress,
        int cidrLength)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (cidrLength is < 0 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(cidrLength));
        }

        var ipAddressAsBytes = ipAddress.GetAddressBytes();
        if (ipAddressAsBytes.Length * 8 < cidrLength)
        {
            throw new FormatException();
        }

        var maskBytes = GetBitMask(ipAddressAsBytes.Length, cidrLength);
        ipAddressAsBytes = DoBitOperation(BooleanOperatorType.AND, ipAddressAsBytes, maskBytes);

        var startIpAddress = new IPAddress(ipAddressAsBytes);
        var endIpAddress = new IPAddress(
            DoBitOperation(
                BooleanOperatorType.OR,
                ipAddressAsBytes,
                DoBitOperation(
                    BooleanOperatorType.NOT,
                    maskBytes,
                    b: null)));

        return (startIpAddress, endIpAddress);
    }

    public static bool IsInRange(
        IPAddress ipAddress,
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

    private static byte[] GetBitMask(
        int sizeOfBuffer,
        int bitLength)
    {
        var maskBytes = new byte[sizeOfBuffer];
        var bytesLen = bitLength / 8;
        var bitsLen = bitLength % 8;
        for (var i = 0; i < bytesLen; i++)
        {
            maskBytes[i] = 0xff;
        }

        if (bitsLen > 0)
        {
            maskBytes[bytesLen] = (byte)~Enumerable
                .Range(1, 8 - bitsLen)
                .Select(n => 1 << (n - 1))
                .Aggregate((a, b) => a | b);
        }

        return maskBytes;
    }

    private static byte[] DoBitOperation(
        BooleanOperatorType operatorType,
        byte[] a,
        byte[]? b)
    {
        if (b is null &&
            operatorType is BooleanOperatorType.AND or BooleanOperatorType.OR)
        {
            throw new ArgumentNullException(nameof(b));
        }

        var result = (byte[])a.Clone();
        for (var i = 0; i < result.Length; i++)
        {
            switch (operatorType)
            {
                case BooleanOperatorType.AND:
                    result[i] &= b![i];
                    break;
                case BooleanOperatorType.OR:
                    result[i] |= b![i];
                    break;
                case BooleanOperatorType.NOT:
                    result[i] = (byte)~result[i];
                    break;
                default:
                    throw new SwitchCaseDefaultException(operatorType);
            }
        }

        return result;
    }
}