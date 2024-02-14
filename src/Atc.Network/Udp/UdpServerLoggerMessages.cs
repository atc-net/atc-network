namespace Atc.Network.Udp;

/// <summary>
/// UdpServer LoggerMessages.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK - By Design")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK")]
public partial class UdpServer
{
    private readonly ILogger logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpServer.NotRunning,
        Level = LogLevel.Trace,
        Message = "Udp server is not running.")]
    private partial void LogUdpServerNotRunning();

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpServer.DataReceivedByteLength,
        Level = LogLevel.Trace,
        Message = "Received {byteLength} bytes.")]
    private partial void LogDataReceived(
        int byteLength);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpServer.DataSendingSocketError,
        Level = LogLevel.Error,
        Message = "Received error when sending data - {socketError}: {errorMessage}.")]
    private partial void LogDataSendingSocketError(
        string socketError,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpServer.DataSendingError,
        Level = LogLevel.Error,
        Message = "Received error when sending data: {errorMessage}.")]
    private partial void LogDataSendingError(
        string errorMessage);
}