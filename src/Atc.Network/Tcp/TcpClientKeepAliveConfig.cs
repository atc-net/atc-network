namespace Atc.Network.Tcp;

/// <summary>
/// TcpClient KeepAlive Config
/// </summary>
public class TcpClientKeepAliveConfig
{
    /// <summary>
    /// KeepAliveInterval
    /// </summary>
    public int KeepAliveInterval { get; set; } = 2;

    /// <summary>
    /// KeepAliveTime
    /// </summary>
    public int KeepAliveTime { get; set; } = 2;

    /// <summary>
    /// KeepAliveRetryCount
    /// </summary>
    public int KeepAliveRetryCount { get; set; } = 3;
}