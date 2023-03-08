namespace Atc.Network.Tcp;

/// <summary>
/// TcpClient KeepAlive Config
/// </summary>
public class TcpClientKeepAliveConfig
{
    /// <summary>
    /// Keep alive interval on the socket option <see cref="SocketOptionName.TcpKeepAliveInterval"/>.
    /// </summary>
    public int KeepAliveInterval { get; set; } = 2;

    /// <summary>
    /// Keep alive time on the socket option <see cref="SocketOptionName.TcpKeepAliveTime"/>.
    /// </summary>
    public int KeepAliveTime { get; set; } = 2;

    /// <summary>
    /// Keep alive retry count on the socket option <see cref="SocketOptionName.TcpKeepAliveRetryCount"/>.
    /// </summary>
    public int KeepAliveRetryCount { get; set; } = 3;

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(KeepAliveInterval)}: {KeepAliveInterval}, {nameof(KeepAliveTime)}: {KeepAliveTime}, {nameof(KeepAliveRetryCount)}: {KeepAliveRetryCount}";
}