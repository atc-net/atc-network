// ReSharper disable CommentTypo
namespace Atc.Network.Internet;

public static class IPScannerConstants
{
    /// <summary>
    /// The connect time-out value, in milliseconds. The default is 180000 (3 min);
    /// </summary>
    public const int TimeoutInMs = 180000;

    /// <summary>
    /// The connect time-out for ping (ICMP) value, in milliseconds. The default is 60000 (1 min);
    /// </summary>
    public const int TimeoutPingInMs = 60000;

    /// <summary>
    /// The connect time-out for tpc value, in milliseconds. The default is 100 (100 msec);
    /// </summary>
    public const int TimeoutTcpInMs = 100;

    /// <summary>
    /// The connect time-out for http/https value, in milliseconds. The default is 500 (500 msec);
    /// </summary>
    public const int TimeoutHttpInMs = 500;
}