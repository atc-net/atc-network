namespace Atc.Network.Tcp;

/// <content>
/// TcpServer LoggerMessages.
/// </content>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK - By Design")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK")]
public partial class TcpServer
{
    private readonly ILogger logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.Starting,
        Level = LogLevel.Trace,
        Message = "Starting tpc-listener on {ipAddress}:{port}.")]
    private partial void LogStarting(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.Started,
        Level = LogLevel.Information,
        Message = "Started tpc-listener on {ipAddress}:{port}.")]
    private partial void LogStarted(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.Stopping,
        Level = LogLevel.Trace,
        Message = "Stopping tpc-listener on {ipAddress}:{port}.")]
    private partial void LogStopping(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.Stopped,
        Level = LogLevel.Information,
        Message = "Stopped tpc-listener on {ipAddress}:{port}.")]
    private partial void LogStopped(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.NotRunning,
        Level = LogLevel.Trace,
        Message = "Tcp server is not running on {ipAddress}:{port}.")]
    private partial void LogTcpServerNotRunning(
        string ipAddress,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.DataReceivedByteLength,
        Level = LogLevel.Trace,
        Message = "Received message {byteLength} bytes on {ipAddress}:{port}.")]
    private partial void LogDataReceived(
        string ipAddress,
        int port,
        int byteLength);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.DataReceivedChunkByteLength,
        Level = LogLevel.Trace,
        Message = "Received chunk {byteLength} bytes on {ipAddress}:{port}.")]
    private partial void LogDataReceivedChunk(
        string ipAddress,
        int port,
        int byteLength);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.DataReceivedHandlerError,
        Level = LogLevel.Error,
        Message = "A DataReceived handler failed for data received on {ipAddress}:{port}.")]
    private partial void LogDataReceivedHandlerError(
        string ipAddress,
        int port,
        Exception exception);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.ClientConnectionError,
        Level = LogLevel.Warning,
        Message = "Client connection on {ipAddress}:{port} failed: {errorMessage}.")]
    private partial void LogClientConnectionError(
        string ipAddress,
        int port,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.ListenerError,
        Level = LogLevel.Error,
        Message = "Tcp listener on {ipAddress}:{port} stopped unexpectedly.")]
    private partial void LogListenerError(
        string ipAddress,
        int port,
        Exception exception);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpServer.EventHandlerError,
        Level = LogLevel.Error,
        Message = "A {eventName} handler failed on {ipAddress}:{port}.")]
    private partial void LogEventHandlerError(
        string ipAddress,
        int port,
        string eventName,
        Exception exception);
}