// ReSharper disable once CheckNamespace
namespace Atc.Network;

/// <summary>
/// Extension methods for <see langword="ushort" />
/// </summary>
public static class UshortExtensions
{
    public static bool IsPortForIPService(
        this ushort portNumber,
        ServiceProtocolType serviceProtocolType,
        IPServicePortExaminationLevel matchLevel)
        => matchLevel switch
        {
            IPServicePortExaminationLevel.All => true,
            IPServicePortExaminationLevel.WellKnownAndCommon => portNumber.IsWellKnownOrCommonIPServicePort(serviceProtocolType),
            IPServicePortExaminationLevel.WellKnown => portNumber.IsWellKnownIPServicePort(serviceProtocolType),
            _ => false,
        };

    /// <summary>
    /// Returns <see langword="true"/> if the value is a well known port or
    /// a common substitute for a IP service.
    /// </summary>
    /// <param name="portNumber">The port number.</param>
    /// <param name="serviceProtocolType">Type of the service protocol.</param>
    /// <remarks>
    /// See <see href="https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers" /> for
    /// a complete list of well known port numbers.
    /// </remarks>
    public static bool IsWellKnownOrCommonIPServicePort(
        this ushort portNumber,
        ServiceProtocolType serviceProtocolType)
    {
        return serviceProtocolType switch
        {
            ServiceProtocolType.Https => Https.Contains(portNumber),
            ServiceProtocolType.Http => Http.Contains(portNumber),
            ServiceProtocolType.Ftps => Ftps.Contains(portNumber),
            ServiceProtocolType.Ftp => Ftp.Contains(portNumber),
            ServiceProtocolType.Telnet => Telnet.Contains(portNumber),
            ServiceProtocolType.Ssh => Ssh.Contains(portNumber),
            _ => false,
        };
    }

    /// <summary>
    /// Returns <see langword="true"/> if the value is a well known port for a IP service.
    /// </summary>
    /// <param name="portNumber">The port number.</param>
    /// <param name="serviceProtocolType">Type of the service protocol.</param>
    /// <remarks>
    /// See <see href="https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers" /> for
    /// a complete list of well known port numbers.
    /// </remarks>
    public static bool IsWellKnownIPServicePort(
        this ushort portNumber,
        ServiceProtocolType serviceProtocolType)
    {
        return serviceProtocolType switch
        {
            ServiceProtocolType.Https => portNumber is 443,
            ServiceProtocolType.Http => portNumber is 80,
            ServiceProtocolType.Ftps => portNumber is 989 or 990,
            ServiceProtocolType.Ftp => portNumber is 20 or 21,
            ServiceProtocolType.Telnet => portNumber is 23,
            ServiceProtocolType.Ssh => portNumber == 22,
            _ => false,
        };
    }

    private static readonly List<ushort> Https = new()
    {
        443,
        832,
        981,
        1311,
        4444,
        4445,
        5001,
        5986,
        7000,
        7002,
        8243,
        8333,
        8403,
        8448,
        8531,
        8888,
        9443,
        12043,
        18091,
        18092,
    };

    private static readonly List<ushort> Http = new()
    {
        80,
        280,
        591,
        1965,
        2480,
        4444,
        4445,
        4567,
        5000,
        5104,
        5800,
        5985,
        7001,
        8000,
        8005,
        8008,
        8042,
        8080,
        8088,
        8096,
        8222,
        8280,
        8281,
        8403,
        8530,
        8887,
        9080,
        9981,
        11371,
        12046,
        16080,
    };

    private static readonly List<ushort> Ftps = new()
    {
        989,
        990,
        6619,
    };

    private static readonly List<ushort> Ftp = new()
    {
        20,
        21,
    };

    private static readonly List<ushort> Ssh = new()
    {
        22,
    };

    private static readonly List<ushort> Telnet = new()
    {
        23,
    };
}