namespace Atc.Network.Udp;

/// <summary>
/// This class contains default constant for
/// <see cref="UdpClient"/> and <see cref="UdpServer"/>.
/// </summary>
public static class UdpConstants
{
    /// <summary>
    /// The connect time-out value, in milliseconds (10 sec).
    /// </summary>
    public const int DefaultConnectTimeout = 10_000;

    /// <summary>
    /// The send/receive time-out value, in milliseconds (5 min.).
    /// </summary>
    public const int DefaultSendReceiveTimeout = 600_000;

    /// <summary>
    /// The send/receive buffer value, in bytes. The default is 8192 (8 Kb);
    /// </summary>
    public const int DefaultBufferSize = 8192;

    /// <summary>
    /// The grace period timeout, in milliseconds (1 sec).
    /// </summary>
    public const int GracePeriodTimeout = 1_000;
}