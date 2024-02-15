namespace Atc.Network.Tcp;

/// <summary>
/// This class contains default constant for
/// <see cref="TcpClient"/> and <see cref="TcpClientReconnectConfig"/>.
/// </summary>
public static class TcpConstants
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
    /// The send/receive buffer value, in bytes (8 Kb).
    /// </summary>
    public const int DefaultBufferSize = 8192;

    /// <summary>
    /// The reconnect retry interval value, in milliseconds (1 sec).
    /// </summary>
    public const int DefaultReconnectRetryInterval = 1_000;

    /// <summary>
    /// The reconnect retry max attempts value.
    /// </summary>
    public const int DefaultReconnectRetryMaxAttempts = 3600;

    /// <summary>
    /// The grace period timeout, in milliseconds (1 sec).
    /// </summary>
    public const int GracePeriodTimeout = 1_000;
}