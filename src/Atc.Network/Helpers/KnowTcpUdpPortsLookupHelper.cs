// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable UnusedMember.Local
namespace Atc.Network.Helpers;

/// <summary>
/// KnowIPPortsLookupHelper.
/// </summary>
/// <remarks>
/// https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers
/// </remarks>
public static class KnowTcpUdpPortsLookupHelper
{
    public static bool IsKnow(
        ServiceProtocolType serviceProtocolType,
        int portNumber)
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

    private static readonly List<int> Https = new()
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

    private static readonly List<int> Http = new()
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

    private static readonly List<int> Ftps = new()
    {
        989,
        990,
        6619,
    };

    private static readonly List<int> Ftp = new()
    {
        20,
        21,
    };

    private static readonly List<int> Ssh = new()
    {
        22,
    };

    private static readonly List<int> Telnet = new()
    {
        23,
    };
}