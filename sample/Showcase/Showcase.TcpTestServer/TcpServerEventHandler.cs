namespace Showcase.TcpTestServer;

/// <summary>
/// Logs clients coming and going, and answers every message with "ack: message".
/// </summary>
/// <param name="tcpServer">The TCP server.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class TcpServerEventHandler(
    TcpServer tcpServer,
    ILogger<TcpServerEventHandler> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        tcpServer.ClientConnected += OnClientConnected;
        tcpServer.ClientDisconnected += OnClientDisconnected;
        tcpServer.ClientDataReceived += OnClientDataReceived;
        tcpServer.ErrorOccurred += OnErrorOccurred;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        tcpServer.ClientConnected -= OnClientConnected;
        tcpServer.ClientDisconnected -= OnClientDisconnected;
        tcpServer.ClientDataReceived -= OnClientDataReceived;
        tcpServer.ErrorOccurred -= OnErrorOccurred;
        return Task.CompletedTask;
    }

    private void OnClientConnected(
        object? sender,
        TcpServerClientEventArgs e)
        => LogClientConnected(e.ClientId, e.RemoteEndPoint);

    private void OnClientDisconnected(
        object? sender,
        TcpServerClientEventArgs e)
        => LogClientDisconnected(e.ClientId, e.RemoteEndPoint);

    private void OnErrorOccurred(
        object? sender,
        NetworkErrorEventArgs e)
        => LogNetworkError(e.Kind, e.EventName, e.Exception);

    private void OnClientDataReceived(
        object? sender,
        TcpServerDataReceivedEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Data.Span).TrimEnd('\n');
        LogMessageReceived(e.ClientId, message);

        // Reply on another task, so the server can read the next message right away.
        // ReplyAsync catches its own exceptions (see docs/events.md).
        _ = ReplyAsync(e.ClientId, message);
    }

    private async Task ReplyAsync(
        Guid clientId,
        string message)
    {
        try
        {
            if (!await tcpServer.Send(clientId, $"ack: {message}", CancellationToken.None))
            {
                LogReplyNotSent(clientId);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogReplyFailed(clientId, ex);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Client {clientId} connected from {remoteEndPoint}")]
    private partial void LogClientConnected(Guid clientId, IPEndPoint remoteEndPoint);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Client {clientId} from {remoteEndPoint} disconnected")]
    private partial void LogClientDisconnected(Guid clientId, IPEndPoint remoteEndPoint);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Client {clientId} sent \"{message}\"")]
    private partial void LogMessageReceived(Guid clientId, string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Reply to client {clientId} was not sent; the client is gone")]
    private partial void LogReplyNotSent(Guid clientId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Reply to client {clientId} failed")]
    private partial void LogReplyFailed(Guid clientId, Exception exception);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "TCP server reported a {kind} error (event: {eventName})")]
    private partial void LogNetworkError(NetworkErrorKind kind, string? eventName, Exception exception);
}