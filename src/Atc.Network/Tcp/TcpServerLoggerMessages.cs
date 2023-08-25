namespace Atc.Network.Tcp;

/// <summary>
/// TcpServer LoggerMessages.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK - By Design")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK")]
public partial class TcpServer
{
    private readonly ILogger logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.Starting,
        Level = LogLevel.Trace,
        Message = "Starting tpc-listener on '{ipAddress}:{port}'.")]
    private partial void LogStarting(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.Started,
        Level = LogLevel.Information,
        Message = "Started tpc-listener on '{ipAddress}:{port}'.")]
    private partial void LogStarted(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.Stopping,
        Level = LogLevel.Trace,
        Message = "Stopping tpc-listener on '{ipAddress}:{port}'.")]
    private partial void LogStopping(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.Stopped,
        Level = LogLevel.Information,
        Message = "Stopped tpc-listener on '{ipAddress}:{port}'.")]
    private partial void LogStopped(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.NotRunning,
        Level = LogLevel.Trace,
        Message = "Tcp server is not running.")]
    private partial void LogTcpServerNotRunning();

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.DataReceivedByteLength,
        Level = LogLevel.Trace,
        Message = "Received '{byteLength}' bytes.")]
    private partial void LogDataReceived(
        int byteLength);
}