namespace Atc.Network.Extensions;

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
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, tcpKeepAliveTime);
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, tcpKeepAliveInterval);
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, tcpKeepAliveRetryCount);
    }
}