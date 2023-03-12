// ReSharper disable once CheckNamespace
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable CommentTypo
namespace Atc.Network;

public static class TcpClientExtensions
{
    /// <summary>
    /// Sets the buffer size and timeouts.
    /// </summary>
    /// <param name="tcpClient">The TCP client.</param>
    /// <param name="sendTimeout">The send timeout value of the connection in milliseconds.</param>
    /// <param name="sendBufferSize">Size of the send buffer in bytes.</param>
    /// <param name="receiveTimeout">The receive timeout value of the connection in milliseconds.</param>
    /// <param name="receiveBufferSize">Size of the receive buffer in bytes.</param>
    public static void SetBufferSizeAndTimeouts(
        this System.Net.Sockets.TcpClient tcpClient,
        int sendTimeout = TcpConstants.DefaultSendReceiveTimeout,
        int sendBufferSize = TcpConstants.DefaultBufferSize,
        int receiveTimeout = TcpConstants.DefaultSendReceiveTimeout,
        int receiveBufferSize = TcpConstants.DefaultBufferSize)
    {
        ArgumentNullException.ThrowIfNull(tcpClient);

        tcpClient.SendTimeout = sendTimeout;
        tcpClient.SendBufferSize = sendBufferSize;
        tcpClient.ReceiveTimeout = receiveTimeout;
        tcpClient.ReceiveBufferSize = receiveBufferSize;
    }

    /// <summary>
    /// Sets the KeepAlive options.
    /// </summary>
    /// <param name="tcpClient">The TcpClient.</param>
    /// <param name="tcpKeepAliveTime">Specifies how often TCP sends keep-alive transmissions (milliseconds).</param>
    /// <param name="tcpKeepAliveInterval">Specifies how often TCP repeats keep-alive transmissions when no response is received.</param>
    /// <param name="tcpKeepAliveRetryCount">The number of TCP keep alive probes that will be sent before the connection is terminated.</param>
    public static void SetKeepAlive(
        this System.Net.Sockets.TcpClient tcpClient,
        int tcpKeepAliveTime = 2,
        int tcpKeepAliveInterval = 2,
        int tcpKeepAliveRetryCount = 5)
    {
        ArgumentNullException.ThrowIfNull(tcpClient);

        if (tcpClient.Client is null)
        {
            throw new SocketException();
        }

        tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: true);
        if (tcpKeepAliveTime >= 1)
        {
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, tcpKeepAliveTime);
        }

        if (tcpKeepAliveInterval >= 1)
        {
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, tcpKeepAliveInterval);
        }

        if (tcpKeepAliveRetryCount >= 0)
        {
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, tcpKeepAliveRetryCount);
        }
    }

    /// <summary>
    /// Disables the keep alive.
    /// </summary>
    /// <param name="tcpClient">The TCP client.</param>
    public static void DisableKeepAlive(
        this System.Net.Sockets.TcpClient tcpClient)
    {
        ArgumentNullException.ThrowIfNull(tcpClient);

        if (tcpClient.Client is null ||
            !tcpClient.Client.Connected)
        {
            return;
        }

        tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: false);
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 1);
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 0);

        // This will disable the Nagle algorithm, which is a mechanism used to reduce network traffic
        // by buffering small packets and sending them together in larger segments.
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, optionValue: true);
    }
}