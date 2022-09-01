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
        EventId = LoggingEventIdConstants.ServiceNotRunning,
        Level = LogLevel.Trace,
        Message = "Service is not running.")]
    private partial void LogServiceNotRunning();
}