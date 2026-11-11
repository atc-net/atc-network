// ReSharper disable NotAccessedField.Local
namespace Atc.Network.Internet;

/// <content>
/// IPScanner LoggerMessages.
/// </content>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK - By Design")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK")]
[SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "OK")]
public partial class IPScanner
{
    private readonly ILogger logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.IPScanner.ServiceDiscoveryFailed,
        Level = LogLevel.Warning,
        Message = "Service discovery with {Protocol} failed, so the scan has no {Protocol} services: {ErrorMessage}")]
    private partial void LogServiceDiscoveryFailed(
        string protocol,
        string errorMessage);
}