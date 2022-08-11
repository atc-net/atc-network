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
            return (false, "Start-IP-address is not v4");
        }

        if (endIpAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            return (false, "End-IP-address is not v4");
        }

        var startIpAddressAsBytes = startIpAddress.GetAddressBytes();
        var endIpAddressAsBytes = endIpAddress.GetAddressBytes();

        Array.Reverse(startIpAddressAsBytes);
        Array.Reverse(endIpAddressAsBytes);

        var ipS = BitConverter.ToUInt32(startIpAddressAsBytes);
        var ipE = BitConverter.ToUInt32(endIpAddressAsBytes);

        return ipS > ipE
            ? (false, "Expect Start-IP-address is greater then End-IP-address")
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
        int cidrMaskLength)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var (startIpAddress, endIpAddress) = GetStartAndEndAddressesInRange(ipAddress, cidrMaskLength);
        return GetAddressesInRange(startIpAddress, endIpAddress);
    }

    public static (IPAddress StartIpAddress, IPAddress EndIpAddress) GetStartAndEndAddressesInRange(
        IPAddress ipAddress,
        int cidrMaskLength)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (cidrMaskLength is < 0 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(cidrMaskLength));
        }

        var ipAddressAsBytes = ipAddress.GetAddressBytes();
        if (ipAddressAsBytes.Length * 8 < cidrMaskLength)
        {
            throw new FormatException();
        }

        var maskBytes = GetBitMask(ipAddressAsBytes.Length, cidrMaskLength);
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
        string cidrMask)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
        ArgumentNullException.ThrowIfNull(cidrMask);

        var sa = cidrMask.Split('/');
        if (sa.Length != 2)
        {
            throw new ArgumentException("Invalid CIDR-mask", nameof(cidrMask));
        }

        var cidrMaskIp = IPAddress.Parse(sa[0]);
        var cidrMaskLength = int.Parse(sa[1], GlobalizationConstants.EnglishCultureInfo);
        if (cidrMaskLength is < 0 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(cidrMask), "Invalid CIDR-mask");
        }

        var ip = BitConverter.ToInt32(ipAddress.GetAddressBytes());
        var cidrM1 = BitConverter.ToInt32(cidrMaskIp.GetAddressBytes());
        var calcM2 = IPAddress.HostToNetworkOrder(-1 << (32 - cidrMaskLength));

        return (ip & calcM2) == (cidrM1 & calcM2);
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