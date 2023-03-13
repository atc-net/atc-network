namespace Atc.Network.Tcp;

/// <summary>
/// KeepAlive configurations for <see cref="TcpClient"/>.
/// </summary>
public class TcpClientKeepAliveConfig
{
    /// <summary>
    /// Keep alive enable/disable on the socket option <see cref="SocketOptionName.KeepAlive"/>.
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Keep alive interval on the socket option <see cref="SocketOptionName.TcpKeepAliveInterval"/>.
    /// </summary>
    public int Interval { get; set; } = 2;

    /// <summary>
    /// Keep alive time on the socket option <see cref="SocketOptionName.TcpKeepAliveTime"/>.
    /// </summary>
    public int Time { get; set; } = 2;

    /// <summary>
    /// Keep alive retry count on the socket option <see cref="SocketOptionName.TcpKeepAliveRetryCount"/>.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Enable)}: {Enable}, {nameof(Interval)}: {Interval}, {nameof(Time)}: {Time}, {nameof(RetryCount)}: {RetryCount}";
}