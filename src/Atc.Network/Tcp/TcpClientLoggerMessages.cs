namespace Atc.Network.Tcp;

/// <summary>
/// TcpClient LoggerMessages.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK - By Design")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK")]
public partial class TcpClient
{
    private readonly ILogger logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.Connecting,
        Level = LogLevel.Trace,
        Message = "Trying to connect to '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogConnecting(string ipAddressOrHostName, int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.Connected,
        Level = LogLevel.Information,
        Message = "Connected to '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogConnected(string ipAddressOrHostName, int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.ConnectionError,
        Level = LogLevel.Error,
        Message = "Could not connected to '{ipAddressOrHostName}' on port '{port}': '{errorMessage}'.")]
    private partial void LogConnectionError(string ipAddressOrHostName, int port, string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.ClientNotConnected,
        Level = LogLevel.Error,
        Message = "Client is not connected to '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogClientNotConnected(string ipAddressOrHostName, int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.Disconnecting,
        Level = LogLevel.Trace,
        Message = "Trying to disconnect from '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogDisconnecting(string ipAddressOrHostName, int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.Disconnected,
        Level = LogLevel.Information,
        Message = "Disconnected from '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogDisconnected(string ipAddressOrHostName, int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.DataSendingByteLength,
        Level = LogLevel.Trace,
        Message = "Sending '{byteLength}' bytes.")]
    private partial void LogDataSendingByteLength(int byteLength);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.DataReceivedByteLength,
        Level = LogLevel.Trace,
        Message = "Received '{byteLength}' bytes.")]
    private partial void LogDataReceivedByteLength(int byteLength);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.DataReceiveTimeout,
        Level = LogLevel.Warning,
        Message = "Timeout occurred when receiving data.")]
    private partial void LogDataReceiveTimeout();

    [LoggerMessage(
        EventId = LoggingEventIdConstants.DataReceiveNoData,
        Level = LogLevel.Warning,
        Message = "Received no data.")]
    private partial void LogDataReceiveNoData();

    [LoggerMessage(
        EventId = LoggingEventIdConstants.DataReceiveError,
        Level = LogLevel.Error,
        Message = "Received error when retrieving data: '{errorMessage}'.")]
    private partial void LogDataReceiveError(string errorMessage);
}