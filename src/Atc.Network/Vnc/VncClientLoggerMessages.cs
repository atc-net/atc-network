namespace Atc.Network.Vnc;

/// <summary>
/// VncClient LoggerMessages.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK - By Design")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK")]
public partial class VncClient
{
    private readonly ILogger logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Connecting,
        Level = LogLevel.Trace,
        Message = "Trying to connect to {ipAddressOrHostName}:{port}.")]
    private partial void LogConnecting(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Connected,
        Level = LogLevel.Information,
        Message = "Connected to {ipAddressOrHostName}:{port}.")]
    private partial void LogConnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.ConnectionError,
        Level = LogLevel.Error,
        Message = "Could not connect to {ipAddressOrHostName}:{port}: {errorMessage}.")]
    private partial void LogConnectionError(
        string ipAddressOrHostName,
        int port,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.ClientNotConnected,
        Level = LogLevel.Error,
        Message = "Client is not connected to {ipAddressOrHostName}:{port}.")]
    private partial void LogClientNotConnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Disconnecting,
        Level = LogLevel.Trace,
        Message = "Trying to disconnect from {ipAddressOrHostName}:{port}.")]
    private partial void LogDisconnecting(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Disconnected,
        Level = LogLevel.Information,
        Message = "Disconnected from {ipAddressOrHostName}:{port}.")]
    private partial void LogDisconnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Authenticating,
        Level = LogLevel.Trace,
        Message = "Authenticating with {ipAddressOrHostName}:{port}.")]
    private partial void LogAuthenticating(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Authenticated,
        Level = LogLevel.Information,
        Message = "Authenticated with {ipAddressOrHostName}:{port}.")]
    private partial void LogAuthenticated(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.AuthenticationFailed,
        Level = LogLevel.Error,
        Message = "Authentication failed with {ipAddressOrHostName}:{port}: {errorMessage}.")]
    private partial void LogAuthenticationFailed(
        string ipAddressOrHostName,
        int port,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Initializing,
        Level = LogLevel.Trace,
        Message = "Initializing session with {ipAddressOrHostName}:{port}.")]
    private partial void LogInitializing(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Initialized,
        Level = LogLevel.Information,
        Message = "Initialized session with {ipAddressOrHostName}:{port} - Desktop: {desktopName}, Size: {width}x{height}.")]
    private partial void LogInitialized(
        string ipAddressOrHostName,
        int port,
        string desktopName,
        int width,
        int height);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.FramebufferUpdateReceived,
        Level = LogLevel.Trace,
        Message = "Framebuffer update received from {ipAddressOrHostName}:{port} - {rectangleCount} rectangle(s).")]
    private partial void LogFramebufferUpdateReceived(
        string ipAddressOrHostName,
        int port,
        int rectangleCount);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.FramebufferUpdateError,
        Level = LogLevel.Error,
        Message = "Error processing framebuffer update from {ipAddressOrHostName}:{port}: {errorMessage}.")]
    private partial void LogFramebufferUpdateError(
        string ipAddressOrHostName,
        int port,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.ServerMessageError,
        Level = LogLevel.Error,
        Message = "Error processing server message from {ipAddressOrHostName}:{port}: {errorMessage}.")]
    private partial void LogServerMessageError(
        string ipAddressOrHostName,
        int port,
        string errorMessage);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.BellReceived,
        Level = LogLevel.Trace,
        Message = "Bell received from {ipAddressOrHostName}:{port}.")]
    private partial void LogBellReceived(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.ServerCutTextReceived,
        Level = LogLevel.Trace,
        Message = "Server cut text received from {ipAddressOrHostName}:{port}.")]
    private partial void LogServerCutTextReceived(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.ProtocolVersionError,
        Level = LogLevel.Error,
        Message = "Unsupported protocol version from {ipAddressOrHostName}:{port}: {version}.")]
    private partial void LogProtocolVersionError(
        string ipAddressOrHostName,
        int port,
        string version);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Reconnecting,
        Level = LogLevel.Trace,
        Message = "Trying to reconnect to {ipAddressOrHostName}:{port}.")]
    private partial void LogReconnecting(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.Reconnected,
        Level = LogLevel.Information,
        Message = "Reconnected to {ipAddressOrHostName}:{port}.")]
    private partial void LogReconnected(
        string ipAddressOrHostName,
        int port);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.ReconnectionWarning,
        Level = LogLevel.Warning,
        Message = "Could not reconnect to {ipAddressOrHostName}:{port}: Retry attempt {retryAttempt} of {retryMaxAttempts}.")]
    private partial void LogReconnectionWarning(
        string ipAddressOrHostName,
        int port,
        int retryAttempt,
        int retryMaxAttempts);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.VncClient.ReconnectionMaxRetryExceededError,
        Level = LogLevel.Error,
        Message = "Could not reconnect to {ipAddressOrHostName}:{port}: max retry attempts exceeded.")]
    private partial void LogReconnectionMaxRetryExceededError(
        string ipAddressOrHostName,
        int port);
}