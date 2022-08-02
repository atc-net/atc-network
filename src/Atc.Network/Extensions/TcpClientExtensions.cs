namespace Atc.Network.Extensions;

public static class TcpClientExtensions
{
    /// <summary>
    /// SetKeepAlive
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

        tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: true);
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, tcpKeepAliveTime);
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, tcpKeepAliveInterval);
        tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, tcpKeepAliveRetryCount);
    }
}