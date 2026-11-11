namespace Showcase.UdpTestServer;

/// <summary>
/// Answers every datagram with "ack: text", sent back to the sender.
/// </summary>
/// <param name="udpServer">The UDP server.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class UdpServerEventHandler(
    UdpServer udpServer,
    ILogger<UdpServerEventHandler> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        udpServer.DatagramReceived += OnDatagramReceived;
        udpServer.ErrorOccurred += OnErrorOccurred;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        udpServer.DatagramReceived -= OnDatagramReceived;
        udpServer.ErrorOccurred -= OnErrorOccurred;
        return Task.CompletedTask;
    }

    private void OnErrorOccurred(
        object? sender,
        NetworkErrorEventArgs e)
        => LogNetworkError(e.Kind, e.EventName, e.Exception);

    private void OnDatagramReceived(
        object? sender,
        UdpDatagramReceivedEventArgs e)
    {
        var message = Encoding.UTF8.GetString(e.Data.Span);
        LogDatagramReceived(e.RemoteEndPoint, message);

        // Reply on another task, so the server can receive the next datagram right away.
        // ReplyAsync catches its own exceptions (see docs/events.md).
        _ = ReplyAsync(e.RemoteEndPoint, message);
    }

    private async Task ReplyAsync(
        IPEndPoint sender,
        string message)
    {
        try
        {
            await udpServer.Send(sender, $"ack: {message}", CancellationToken.None);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogReplyFailed(sender, ex);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{sender} sent \"{message}\"")]
    private partial void LogDatagramReceived(IPEndPoint sender, string message);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Reply to {sender} failed")]
    private partial void LogReplyFailed(IPEndPoint sender, Exception exception);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "UDP server reported a {kind} error (event: {eventName})")]
    private partial void LogNetworkError(NetworkErrorKind kind, string? eventName, Exception exception);
}