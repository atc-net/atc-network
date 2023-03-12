// ReSharper disable once CheckNamespace
namespace Atc.Network;

/// <summary>
/// Enumeration: ConnectionType.
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Default None.
    /// </summary>
    None,

    /// <summary>
    /// Connecting.
    /// </summary>
    [LocalizedDescription(nameof(Connecting), typeof(Communication))]
    Connecting,

    /// <summary>
    /// Connected.
    /// </summary>
    [LocalizedDescription(nameof(Connected), typeof(Communication))]
    Connected,

    /// <summary>
    /// Disconnecting.
    /// </summary>
    [LocalizedDescription(nameof(Disconnecting), typeof(Communication))]
    Disconnecting,

    /// <summary>
    /// Disconnected.
    /// </summary>
    [LocalizedDescription(nameof(Disconnected), typeof(Communication))]
    Disconnected,

    /// <summary>
    /// The connection failed.
    /// </summary>
    [LocalizedDescription(nameof(ConnectionFailed), typeof(Communication))]
    ConnectionFailed,

    /// <summary>
    /// The reconnection failed.
    /// </summary>
    [LocalizedDescription(nameof(ReconnectionFailed), typeof(Communication))]
    ReconnectionFailed,

    /// <summary>
    /// Reconnecting.
    /// </summary>
    [LocalizedDescription(nameof(Reconnecting), typeof(Communication))]
    Reconnecting,

    /// <summary>
    /// Reconnected.
    /// </summary>
    [LocalizedDescription(nameof(Reconnected), typeof(Communication))]
    Reconnected,

    /// <summary>
    /// Pulse.
    /// </summary>
    [LocalizedDescription(nameof(Pulse), typeof(Communication))]
    Pulse,
}