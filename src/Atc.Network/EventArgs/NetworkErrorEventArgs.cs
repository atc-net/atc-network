// ReSharper disable CheckNamespace
namespace Atc.Network;

/// <summary>
/// Event data for an error that a client or server handled and logged, so it did not stop.
/// </summary>
/// <seealso cref="EventArgs" />
public class NetworkErrorEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkErrorEventArgs"/> class.
    /// </summary>
    /// <param name="kind">Where the error came from.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="eventName">The event whose handler threw, for <see cref="NetworkErrorKind.EventHandler"/>.</param>
    public NetworkErrorEventArgs(
        NetworkErrorKind kind,
        Exception exception,
        string? eventName = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Kind = kind;
        Exception = exception;
        EventName = eventName;
    }

    /// <summary>
    /// Gets where the error came from.
    /// </summary>
    public NetworkErrorKind Kind { get; }

    /// <summary>
    /// Gets the exception.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the event whose handler threw, such as "DataReceived", for <see cref="NetworkErrorKind.EventHandler"/>; otherwise <see langword="null"/>.
    /// </summary>
    public string? EventName { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Kind)}: {Kind}, {nameof(EventName)}: {EventName}, {nameof(Exception)}: {Exception.Message}";
}