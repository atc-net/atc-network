namespace Atc.Network.Tcp;

public static class TcpConstants
{
    /// <summary>
    /// The connect time-out value, in milliseconds. The default is 10000 (10 sec);
    /// </summary>
    public const int DefaultConnectTimeout = 10000;

    /// <summary>
    /// The send/receive time-out value, in milliseconds. The default is 0;
    /// </summary>
    public const int DefaultSendReceiveTimeout = 0;

    /// <summary>
    /// The send/receive buffer value, in bytes. The default is 8192 (8 Kb);
    /// </summary>
    public const int DefaultBufferSize = 8192;
}