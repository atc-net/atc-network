// ReSharper disable NotAccessedField.Local
namespace Atc.Network.Internet;

/// <summary>
/// IPScanner LoggerMessages.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK - By Design")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "OK")]
[SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "OK")]
public partial class IPScanner
{
    private readonly ILogger logger;
}