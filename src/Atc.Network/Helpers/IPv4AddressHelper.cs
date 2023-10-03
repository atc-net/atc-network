// ReSharper disable InconsistentNaming
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
namespace Atc.Network.Helpers;

public static class IPv4AddressHelper
{
    public static bool IsValid(
        string ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var isValid = IPAddress.TryParse(ipAddress, out var address) &&
                       address.AddressFamily == AddressFamily.InterNetwork;

        if (!isValid)
        {
            return false;
        }

        var octetCount = ipAddress.Split('.').Length;
        return octetCount == 4;
    }

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

        return startIpAddress.ToUnsignedInt() > endIpAddress.ToUnsignedInt()
            ? (false, $"{nameof(startIpAddress)} is higher than {nameof(endIpAddress)}")
            : (true, null);
    }

    public static IPAddress? GetLocalAddress()
    {
        var hostName = Dns.GetHostName();
        var host = Dns.GetHostEntry(hostName);
        return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }

    public static IReadOnlyCollection<IPAddress> GetAddressesInRange(
        IPAddress startIpAddress,
        IPAddress endIpAddress)
    {
        var validationResult = ValidateAddresses(startIpAddress, endIpAddress);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ErrorMessage);
        }

        var list = new List<IPAddress>();
        for (var i = startIpAddress.ToUnsignedInt(); i <= endIpAddress.ToUnsignedInt(); i++)
        {
            var bytes = BitConverter.GetBytes(i);
            Array.Reverse(bytes);
            list.Add(new IPAddress(bytes));
        }

        return list;
    }

    public static IReadOnlyCollection<IPAddress> GetAddressesInRange(
        IPAddress ipAddress,
        int cidrLength)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var (startIpAddress, endIpAddress) = GetFirstAndLastAddressInRange(ipAddress, cidrLength);
        return GetAddressesInRange(startIpAddress, endIpAddress);
    }

    public static (IPAddress StartIpAddress, IPAddress EndIpAddress) GetFirstAndLastAddressInRange(
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

    private static byte[] GetBitMask(
        int sizeOfBuffer,
        int bitLength)
    {
        var maskBytes = new byte[sizeOfBuffer];
        var byteLength = bitLength / 8;
        var bitsLength = bitLength % 8;
        for (var i = 0; i < byteLength; i++)
        {
            maskBytes[i] = 0xff;
        }

        if (bitsLength > 0)
        {
            maskBytes[byteLength] = (byte)~Enumerable
                .Range(1, 8 - bitsLength)
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