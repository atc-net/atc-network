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
        EventId = LoggingEventIdConstants.TcpClient.Connecting,
        Level = LogLevel.Trace,
        Message = "Trying to connect to {ipAddressOrHostName}:{port}.")]
    private partial void LogConnecting(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.Connected,
        Level = LogLevel.Information,
        Message = "Connected to {ipAddressOrHostName}:{port}.")]
    private partial void LogConnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.Reconnecting,
        Level = LogLevel.Trace,
        Message = "Trying to reconnect to {ipAddressOrHostName}:{port}.")]
    private partial void LogReconnecting(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.Reconnected,
        Level = LogLevel.Information,
        Message = "Reconnected to {ipAddressOrHostName}:{port}.")]
    private partial void LogReconnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.ConnectionError,
        Level = LogLevel.Error,
        Message = "Could not connect to {ipAddressOrHostName}:{port}: {errorMessage}.")]
    private partial void LogConnectionError(
        string ipAddressOrHostName,
        int port,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.ReconnectionWarning,
        Level = LogLevel.Warning,
        Message = "Could not reconnect to {ipAddressOrHostName}:{port}: Retry attempt {reconnectRetryCounter} of {retryMaxAttempts}.")]
    private partial void LogReconnectionWarning(
        string ipAddressOrHostName,
        int port,
        int reconnectRetryCounter,
        int retryMaxAttempts);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.ReconnectionMaxRetryExceededError,
        Level = LogLevel.Error,
        Message = "Could not reconnect to {ipAddressOrHostName}:{port}: max retry attempts exceeded.")]
    private partial void LogReconnectionMaxRetryExceededError(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.ClientNotConnected,
        Level = LogLevel.Error,
        Message = "Client is not connected to {ipAddressOrHostName}:{port}.")]
    private partial void LogClientNotConnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.Disconnecting,
        Level = LogLevel.Trace,
        Message = "Trying to disconnect from {ipAddressOrHostName}:{port}.")]
    private partial void LogDisconnecting(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.Disconnected,
        Level = LogLevel.Information,
        Message = "Disconnected from {ipAddressOrHostName}:{port}.")]
    private partial void LogDisconnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.DataSendingByteLength,
        Level = LogLevel.Trace,
        Message = "Sending {byteLength} bytes to {ipAddressOrHostName}:{port}.")]
    private partial void LogDataSendingByteLength(
        string ipAddressOrHostName,
        int port,
        int byteLength);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.DataSendingSocketError,
        Level = LogLevel.Error,
        Message = "Received error when sending data from {ipAddressOrHostName}:{port} - {socketError}: {errorMessage}.")]
    private partial void LogDataSendingSocketError(
        string ipAddressOrHostName,
        int port,
        string socketError,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.DataSendingError,
        Level = LogLevel.Error,
        Message = "Received error when sending data from {ipAddressOrHostName}:{port}: {errorMessage}.")]
    private partial void LogDataSendingError(
        string ipAddressOrHostName,
        int port,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.DataReceivedByteLength,
        Level = LogLevel.Trace,
        Message = "Received {byteLength} bytes from {ipAddressOrHostName}:{port}.")]
    private partial void LogDataReceivedByteLength(
        string ipAddressOrHostName,
        int port,
        int byteLength);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.DataReceiveTimeout,
        Level = LogLevel.Warning,
        Message = "Timeout occurred when receiving data from {ipAddressOrHostName}:{port}.")]
    private partial void LogDataReceiveTimeout(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.DataReceiveNoData,
        Level = LogLevel.Warning,
        Message = "Received no data from {ipAddressOrHostName}:{port}.")]
    private partial void LogDataReceiveNoData(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.TcpClient.DataReceiveError,
        Level = LogLevel.Error,
        Message = "Received error when retrieving data from {ipAddressOrHostName}:{port}: {errorMessage}.")]
    private partial void LogDataReceiveError(
        string ipAddressOrHostName,
        int port,
        string errorMessage);
}