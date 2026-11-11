namespace Showcase.TcpTestClient;

/// <summary>
/// Connects to the TCP test server and sends "ping n" every 2 seconds.
/// After the first connect, TcpClient reconnects by itself when the connection drops.
/// </summary>
/// <param name="configuration">The configuration, with the server endpoint from Aspire.</param>
/// <param name="loggerFactory">Creates the logger for the TcpClient.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class TcpTestClientWorker(
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    ILogger<TcpTestClientWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan SendInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aspire passes the server's endpoint with WithReference; 127.0.0.1:9100 when the app runs on its own.
        var endpoint = configuration.GetServiceEndpoint("tcp-test-server", "tcp", new IPEndPoint(IPAddress.Loopback, 9100));
        LogUsingServer(endpoint);

        using var tcpClient = new TcpClient(
            loggerFactory.CreateLogger<TcpClient>(),
            endpoint,
            new TcpClientConfig
            {
                ConnectTimeout = 5_000,
                DefaultEncoding = Encoding.UTF8,
                TerminationType = TerminationType.LineFeed,
            },
            new TcpClientReconnectConfig
            {
                Enable = true,
                RetryInterval = 2_000,
            });

        tcpClient.DataReceived += data =>
        {
            var reply = Encoding.UTF8.GetString(data).TrimEnd('\n');
            LogReplyReceived(reply);
        };

        // Errors the client handled and logged itself, such as a failing handler or a failed read.
        tcpClient.ErrorOccurred += (_, e) => LogNetworkError(e.Kind, e.EventName, e.Exception);

        await ConnectUntilConnected(tcpClient, stoppingToken);

        var counter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            if (tcpClient.IsConnected)
            {
                counter++;
                await SendPing(tcpClient, counter, stoppingToken);
            }

            await Task.Delay(SendInterval, stoppingToken);
        }

        await tcpClient.Disconnect();
    }

    /// <summary>
    /// TcpClient reconnects after a dropped connection, but not when the very first connect fails,
    /// for example because the server is still starting.
    /// </summary>
    private static async Task ConnectUntilConnected(
        TcpClient tcpClient,
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested &&
               !await tcpClient.Connect(stoppingToken))
        {
            await Task.Delay(SendInterval, stoppingToken);
        }
    }

    private async Task SendPing(
        TcpClient tcpClient,
        int counter,
        CancellationToken stoppingToken)
    {
        var message = $"ping {counter}";
        try
        {
            await tcpClient.Send(message, stoppingToken);
            LogPingSent(message);
        }
        catch (TcpException ex)
        {
            // The connection dropped between the IsConnected check and the send.
            LogPingFailed(message, ex.Message);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Using TCP server {endpoint}")]
    private partial void LogUsingServer(IPEndPoint endpoint);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Sent \"{message}\"")]
    private partial void LogPingSent(string message);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Received \"{message}\"")]
    private partial void LogReplyReceived(string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Could not send \"{message}\": {reason}")]
    private partial void LogPingFailed(string message, string reason);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "TCP client reported a {kind} error (event: {eventName})")]
    private partial void LogNetworkError(NetworkErrorKind kind, string? eventName, Exception exception);
}