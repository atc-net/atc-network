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
    [LocalizedDescription(nameof(Connecting), typeof(Resource.Communication))]
    Connecting,

    /// <summary>
    /// Connected.
    /// </summary>
    [LocalizedDescription(nameof(Connected), typeof(Resource.Communication))]
    Connected,

    /// <summary>
    /// Disconnecting.
    /// </summary>
    [LocalizedDescription(nameof(Disconnecting), typeof(Resource.Communication))]
    Disconnecting,

    /// <summary>
    /// Disconnected.
    /// </summary>
    [LocalizedDescription(nameof(Disconnected), typeof(Resource.Communication))]
    Disconnected,

    /// <summary>
    /// The connection failed.
    /// </summary>
    [LocalizedDescription(nameof(ConnectionFailed), typeof(Resource.Communication))]
    ConnectionFailed,

    /// <summary>
    /// Reconnecting.
    /// </summary>
    [LocalizedDescription(nameof(Reconnecting), typeof(Resource.Communication))]
    Reconnecting,

    /// <summary>
    /// Reconnected.
    /// </summary>
    [LocalizedDescription(nameof(Reconnected), typeof(Resource.Communication))]
    Reconnected,

    /// <summary>
    /// Pulse.
    /// </summary>
    [LocalizedDescription(nameof(Pulse), typeof(Resource.Communication))]
    Pulse,
}