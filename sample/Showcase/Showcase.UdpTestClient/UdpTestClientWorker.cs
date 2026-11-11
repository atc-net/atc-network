namespace Showcase.UdpTestClient;

/// <summary>
/// Sends "ping n" to the UDP test server every 2 seconds and logs the replies.
/// UDP has no connection, so a datagram sent while the server is down is simply lost.
/// </summary>
/// <param name="configuration">The configuration, with the server endpoint from Aspire.</param>
/// <param name="loggerFactory">Creates the logger for the UdpClient.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class UdpTestClientWorker(
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    ILogger<UdpTestClientWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan SendInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aspire passes the server's endpoint with WithReference; 127.0.0.1:9101 when the app runs on its own.
        var endpoint = configuration.GetServiceEndpoint("udp-test-server", "udp", new IPEndPoint(IPAddress.Loopback, 9101));
        LogUsingServer(endpoint);

        using var udpClient = new UdpClient(
            loggerFactory.CreateLogger<UdpClient>(),
            endpoint,
            new UdpClientConfig
            {
                DefaultEncoding = Encoding.UTF8,
            });

        udpClient.DataReceived += data =>
        {
            var reply = Encoding.UTF8.GetString(data);
            LogReplyReceived(reply);
        };

        // For example a receive error while the server is down: the datagram is answered with "port unreachable".
        udpClient.ErrorOccurred += (_, e) => LogNetworkError(e.Kind, e.EventName ?? "none", e.Exception);

        while (!stoppingToken.IsCancellationRequested &&
               !await udpClient.Connect(stoppingToken))
        {
            await Task.Delay(SendInterval, stoppingToken);
        }

        var counter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            counter++;
            var message = $"ping {counter}";
            await udpClient.Send(message, stoppingToken);
            LogPingSent(message);

            await Task.Delay(SendInterval, stoppingToken);
        }

        await udpClient.Disconnect();
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Using UDP server {endpoint}")]
    private partial void LogUsingServer(IPEndPoint endpoint);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Sent \"{message}\"")]
    private partial void LogPingSent(string message);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Received \"{message}\"")]
    private partial void LogReplyReceived(string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "UDP client reported a {kind} error (event: {eventName})")]
    private partial void LogNetworkError(NetworkErrorKind kind, string eventName, Exception exception);
}