namespace Atc.Network;

/// <summary>
/// The names of the metrics and traces that Atc.Network emits, for use with OpenTelemetry,
/// for example <c>AddMeter(NetworkTelemetry.MeterName)</c> and <c>AddSource(NetworkTelemetry.ActivitySourceName)</c>.
/// </summary>
/// <remarks>
/// Every measurement and activity is tagged with <see cref="ComponentTag"/> (for example "tcp_client"),
/// <see cref="ServerAddressTag"/> and <see cref="ServerPortTag"/>: the server a client connects to,
/// or the address and port a server listens on.
/// </remarks>
public static class NetworkTelemetry
{
    /// <summary>
    /// The name of the <see cref="System.Diagnostics.Metrics.Meter"/>.
    /// </summary>
    public const string MeterName = "Atc.Network";

    /// <summary>
    /// The name of the <see cref="System.Diagnostics.ActivitySource"/>.
    /// </summary>
    public const string ActivitySourceName = "Atc.Network";

    /// <summary>
    /// Connections that are open now. A server counts each connected client.
    /// </summary>
    public const string ActiveConnectionsInstrumentName = "atc.network.connections.active";

    /// <summary>
    /// Connect attempts that failed, and server-side TLS handshakes that failed.
    /// </summary>
    public const string ConnectionFailuresInstrumentName = "atc.network.connection.failures";

    /// <summary>
    /// Reconnect attempts, tagged with <see cref="OutcomeTag"/> "success" or "failure".
    /// </summary>
    public const string ReconnectsInstrumentName = "atc.network.reconnects";

    /// <summary>
    /// Bytes sent.
    /// </summary>
    public const string BytesSentInstrumentName = "atc.network.bytes.sent";

    /// <summary>
    /// Bytes received.
    /// </summary>
    public const string BytesReceivedInstrumentName = "atc.network.bytes.received";

    /// <summary>
    /// Messages sent: one per send call.
    /// </summary>
    public const string MessagesSentInstrumentName = "atc.network.messages.sent";

    /// <summary>
    /// Messages received: one per DataReceived event (a TCP client read, a TCP server message or a UDP datagram).
    /// </summary>
    public const string MessagesReceivedInstrumentName = "atc.network.messages.received";

    /// <summary>
    /// Event handlers that threw, tagged with <see cref="EventTag"/>.
    /// </summary>
    public const string HandlerErrorsInstrumentName = "atc.network.handler.errors";

    /// <summary>
    /// The tag with the component: "tcp_client", "tcp_server", "udp_client", "udp_server" or "vnc_client".
    /// </summary>
    public const string ComponentTag = "atc.network.component";

    /// <summary>
    /// The tag with the server address.
    /// </summary>
    public const string ServerAddressTag = "server.address";

    /// <summary>
    /// The tag with the server port.
    /// </summary>
    public const string ServerPortTag = "server.port";

    /// <summary>
    /// The tag with the event whose handler threw.
    /// </summary>
    public const string EventTag = "atc.network.event";

    /// <summary>
    /// The tag with the outcome of a reconnect attempt: "success" or "failure".
    /// </summary>
    public const string OutcomeTag = "atc.network.outcome";

    private static readonly string? Version = typeof(NetworkTelemetry).Assembly.GetName().Version?.ToString();

    internal static readonly Meter Meter = new(MeterName, Version);

    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version);

    internal static readonly UpDownCounter<long> ActiveConnections = Meter.CreateUpDownCounter<long>(
        ActiveConnectionsInstrumentName,
        unit: "{connection}",
        description: "Connections that are open now.");

    internal static readonly Counter<long> ConnectionFailures = Meter.CreateCounter<long>(
        ConnectionFailuresInstrumentName,
        unit: "{connection}",
        description: "Connect attempts and TLS handshakes that failed.");

    internal static readonly Counter<long> Reconnects = Meter.CreateCounter<long>(
        ReconnectsInstrumentName,
        unit: "{attempt}",
        description: "Reconnect attempts.");

    internal static readonly Counter<long> BytesSent = Meter.CreateCounter<long>(
        BytesSentInstrumentName,
        unit: "By",
        description: "Bytes sent.");

    internal static readonly Counter<long> BytesReceived = Meter.CreateCounter<long>(
        BytesReceivedInstrumentName,
        unit: "By",
        description: "Bytes received.");

    internal static readonly Counter<long> MessagesSent = Meter.CreateCounter<long>(
        MessagesSentInstrumentName,
        unit: "{message}",
        description: "Messages sent.");

    internal static readonly Counter<long> MessagesReceived = Meter.CreateCounter<long>(
        MessagesReceivedInstrumentName,
        unit: "{message}",
        description: "Messages received.");

    internal static readonly Counter<long> HandlerErrors = Meter.CreateCounter<long>(
        HandlerErrorsInstrumentName,
        unit: "{error}",
        description: "Event handlers that threw.");
}