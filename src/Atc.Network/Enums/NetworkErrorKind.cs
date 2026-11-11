// ReSharper disable once CheckNamespace
namespace Atc.Network;

/// <summary>
/// Where an error reported by an <c>ErrorOccurred</c> event came from.
/// </summary>
public enum NetworkErrorKind
{
    /// <summary>
    /// An event handler, or an <c>On…</c> override, threw. <see cref="NetworkErrorEventArgs.EventName"/> names the event.
    /// The client or server keeps running.
    /// </summary>
    EventHandler,

    /// <summary>
    /// Receiving failed, for example because the connection was reset.
    /// </summary>
    Receive,

    /// <summary>
    /// A server could not serve one client, for example because its TLS handshake failed.
    /// The server keeps serving the other clients.
    /// </summary>
    ClientConnection,

    /// <summary>
    /// A server's listener stopped unexpectedly. The server no longer accepts clients.
    /// </summary>
    Listener,
}