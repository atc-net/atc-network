// ReSharper disable CheckNamespace
namespace Atc.Network;

/// <summary>
/// ConnectionStateEventArgs.
/// </summary>
/// <seealso cref="EventArgs" />
public class ConnectionStateEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStateEventArgs"/> class.
    /// </summary>
    /// <param name="state">The state of connection type.</param>
    public ConnectionStateEventArgs(
        ConnectionType state)
    {
        this.State = state;
        this.ErrorMessage = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStateEventArgs"/> class.
    /// </summary>
    /// <param name="state">The state of connection type.</param>
    /// <param name="errorMessage">The error message.</param>
    public ConnectionStateEventArgs(
        ConnectionType state,
        string errorMessage)
    {
        this.State = state;
        this.ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets the state.
    /// </summary>
    /// <value>
    /// The state.
    /// </value>
    public ConnectionType State { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    /// <value>
    /// The error message.
    /// </value>
    public string? ErrorMessage { get; }

    /// <inheritdoc />
    public override string ToString()
        => string.IsNullOrEmpty(ErrorMessage)
            ? $"{nameof(State)}: {State}"
            : $"{nameof(State)}: {State}, {nameof(ErrorMessage)}: {ErrorMessage}";
}