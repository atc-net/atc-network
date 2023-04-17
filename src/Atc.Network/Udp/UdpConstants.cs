namespace Atc.Network.Udp;

/// <summary>
/// This class contains default constant for
/// <see cref="UdpClient"/> and <see cref="UdpServer"/>.
/// </summary>
public static class UdpConstants
{
    /// <summary>
    /// The send/receive time-out value, in milliseconds.
    /// </summary>
    public const int DefaultSendReceiveTimeout = 0;

    /// <summary>
    /// The send/receive buffer value, in bytes. The default is 8192 (8 Kb);
    /// </summary>
    public const int DefaultBufferSize = 8192;
}