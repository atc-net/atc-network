namespace Atc.Network.Tcp;

public static class TcpConstants
{
    /// <summary>
    /// The connect time-out value, in milliseconds (10 sec);
    /// </summary>
    public const int DefaultConnectTimeout = 10000;

    /// <summary>
    /// The send/receive time-out value, in milliseconds.
    /// </summary>
    public const int DefaultSendReceiveTimeout = 0;

    /// <summary>
    /// The send/receive buffer value, in bytes (8 Kb);
    /// </summary>
    public const int DefaultBufferSize = 8192;

    /// <summary>
    /// The reconnect delay value, in milliseconds.
    /// </summary>
    public const int DefaultReconnectDelay = 100;
}