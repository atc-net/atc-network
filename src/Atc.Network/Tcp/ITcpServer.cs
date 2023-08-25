namespace Atc.Network.Tcp;

public interface ITcpServer : IHostedService, IDisposable
{
    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    event Action<byte[]>? DataReceived;

    /// <summary>
    /// IPAddress for server connection.
    /// </summary>
    IPAddress IpAddress { get; }

    /// <summary>
    /// Port number for server connection.
    /// </summary>
    int Port { get; }

    /// <summary>
    /// Is running.
    /// </summary>
    bool IsRunning { get; }
}