// ReSharper disable InconsistentNaming
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for validating and working with IPv4 addresses.
/// </summary>
public static class IPv4AddressHelper
{
    /// <summary>
    /// Validates if a string is a valid IPv4 address.
    /// </summary>
    /// <param name="ipAddress">The IP address in string format to validate.</param>
    /// <returns>
    /// True if the IP address is valid; otherwise, false.
    /// </returns>
    /// <remarks>
    /// This method checks if the string can be parsed into an IPAddress object and belongs to the IPv4 address family.
    /// It also ensures that the IP address string has exactly four octets.
    /// </remarks>
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

    /// <summary>
    /// Validates that two IP addresses are valid IPv4 addresses and that the start IP is less than or equal to the end IP.
    /// </summary>
    /// <param name="startIpAddress">The starting IP address of the range.</param>
    /// <param name="endIpAddress">The ending IP address of the range.</param>
    /// <returns>
    /// A tuple containing a boolean indicating if the addresses are valid and an error message if they are not.
    /// </returns>
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

    /// <summary>
    /// Retrieves the local machine's IPv4 address.
    /// </summary>
    /// <returns>
    /// The local IPv4 address, or null if not found.
    /// </returns>
    public static IPAddress? GetLocalAddress()
    {
        var hostName = Dns.GetHostName();
        var host = Dns.GetHostEntry(hostName);
        return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }

    /// <summary>
    /// Generates a collection of IPv4 addresses within a specified range.
    /// </summary>
    /// <param name="startIpAddress">The starting IP address of the range.</param>
    /// <param name="endIpAddress">The ending IP address of the range.</param>
    /// <returns>
    /// A read-only collection of IP addresses within the specified range.
    /// </returns>
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

    /// <summary>
    /// Generates a collection of IPv4 addresses within a subnet defined by an IP address and CIDR notation length.
    /// </summary>
    /// <param name="ipAddress">The IP address within the subnet.</param>
    /// <param name="cidrLength">The CIDR notation length of the subnet mask.</param>
    /// <returns>
    /// A read-only collection of IP addresses within the specified subnet.
    /// </returns>
    /// <remarks>
    /// This method calculates the first and last IP addresses in the subnet range based on the provided IP address and CIDR length.
    /// It then generates all IP addresses within this range. This is particularly useful for subnet exploration and network analysis tasks.
    /// </remarks>
    public static IReadOnlyCollection<IPAddress> GetAddressesInRange(
        IPAddress ipAddress,
        int cidrLength)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var (startIpAddress, endIpAddress) = GetFirstAndLastAddressInRange(ipAddress, cidrLength);
        return GetAddressesInRange(startIpAddress, endIpAddress);
    }

    /// <summary>
    /// Calculates the first and last IP addresses in a subnet given an IP address and CIDR length.
    /// </summary>
    /// <param name="ipAddress">The IP address within the subnet.</param>
    /// <param name="cidrLength">The CIDR notation length of the subnet mask.</param>
    /// <returns>
    /// A tuple containing the first and last IP addresses in the subnet range.
    /// </returns>
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