namespace Atc.Network.Udp;

/// <summary>
/// UdpClient LoggerMessages.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK - By Design")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK")]
public partial class UdpClient
{
    private readonly ILogger logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpClient.Connecting,
        Level = LogLevel.Trace,
        Message = "Trying to connect to '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogConnecting(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpClient.Connected,
        Level = LogLevel.Information,
        Message = "Connected to '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogConnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpClient.ConnectionError,
        Level = LogLevel.Error,
        Message = "Could not connect to '{ipAddressOrHostName}' on port '{port}': '{errorMessage}'.")]
    private partial void LogConnectionError(
        string ipAddressOrHostName,
        int port,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpClient.ClientNotConnected,
        Level = LogLevel.Error,
        Message = "Client is not connected to '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogClientNotConnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpClient.Disconnecting,
        Level = LogLevel.Trace,
        Message = "Trying to disconnect from '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogDisconnecting(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpClient.Disconnected,
        Level = LogLevel.Information,
        Message = "Disconnected from '{ipAddressOrHostName}' on port '{port}'.")]
    private partial void LogDisconnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpClient.DataSendingSocketError,
        Level = LogLevel.Error,
        Message = "Received error when sending data - {socketError}: {errorMessage}.")]
    private partial void LogDataSendingSocketError(
        string socketError,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.UdpClient.DataSendingError,
        Level = LogLevel.Error,
        Message = "Received error when sending data: {errorMessage}.")]
    private partial void LogDataSendingError(
        string errorMessage);
}