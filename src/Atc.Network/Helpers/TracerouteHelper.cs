namespace Atc.Network.Helpers;

/// <summary>
/// Traces the route to a host: the routers a packet passes on its way, as <c>tracert</c> and <c>traceroute</c> show them.
/// </summary>
/// <remarks>
/// Pings are sent with a time to live of 1, 2, 3 and so on. Each router that drops a ping because its time to live ran out
/// answers with "time exceeded", which names the router; the target answers the ping itself. One probe is sent per hop.
/// </remarks>
public static class TracerouteHelper
{
    /// <summary>
    /// The default largest number of hops.
    /// </summary>
    public const int DefaultMaxHops = 30;

    /// <summary>
    /// The default time to wait for each hop's answer, in milliseconds.
    /// </summary>
    public const int DefaultTimeoutPerHopInMs = 1_000;

    private const int MaxTimeToLive = 255;
    private const int PayloadLength = 32;

    private static readonly byte[] Payload = new byte[PayloadLength];

    /// <summary>
    /// Traces the route to a host, and returns each hop as soon as it has answered or timed out.
    /// </summary>
    /// <param name="target">The host to trace the route to, IPv4 or IPv6.</param>
    /// <param name="maxHops">The largest number of hops, from 1 to 255. The default is 30.</param>
    /// <param name="timeoutPerHop">How long to wait for each hop's answer. The default is 1 second.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The hops, in order. The sequence ends at the target, at a hop that reports the target unreachable, or after
    /// <paramref name="maxHops"/> hops. A hop that does not answer has no address (shown as "*").
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxHops"/> is not between 1 and 255.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token was cancelled.</exception>
    /// <exception cref="PingException">A ping could not be sent, for example without a network.</exception>
    [SuppressMessage("Reliability", "CA2025:Ensure tasks using 'IDisposable' instances complete before the instances are disposed", Justification = "OK - each probe is awaited before the next, and all before the Ping is disposed.")]
    public static async IAsyncEnumerable<TracerouteHop> Trace(
        IPAddress target,
        int maxHops = DefaultMaxHops,
        TimeSpan? timeoutPerHop = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // One Ping for the whole trace, so the cost of creating it does not count in the hops' times.
        using var ping = new Ping();
        await foreach (var hop in Trace(
                           target,
                           maxHops,
                           timeoutPerHop ?? TimeSpan.FromMilliseconds(DefaultTimeoutPerHopInMs),
                           (probeTarget, timeToLive, timeout, token) => SendProbe(ping, probeTarget, timeToLive, timeout, token),
                           cancellationToken))
        {
            yield return hop;
        }
    }

    /// <summary>
    /// Traces the route, sending each probe with <paramref name="sendProbe"/> (a real ping, or a fake in tests).
    /// </summary>
    /// <param name="target">The host to trace the route to.</param>
    /// <param name="maxHops">The largest number of hops.</param>
    /// <param name="timeoutPerHop">How long to wait for each hop's answer.</param>
    /// <param name="sendProbe">Sends one probe with a time to live, and returns the answer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The hops.</returns>
    internal static IAsyncEnumerable<TracerouteHop> Trace(
        IPAddress target,
        int maxHops,
        TimeSpan timeoutPerHop,
        Func<IPAddress, int, TimeSpan, CancellationToken, Task<TracerouteProbeResult>> sendProbe,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxHops, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxHops, MaxTimeToLive);

        return TraceHops(target, maxHops, timeoutPerHop, sendProbe, cancellationToken);
    }

    private static async IAsyncEnumerable<TracerouteHop> TraceHops(
        IPAddress target,
        int maxHops,
        TimeSpan timeoutPerHop,
        Func<IPAddress, int, TimeSpan, CancellationToken, Task<TracerouteProbeResult>> sendProbe,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // ICMP has no port, so the port tag is 0.
        var instrumentation = new NetworkInstrumentation("traceroute", target.ToString(), 0);
        using var activity = instrumentation.StartActivity("Traceroute.Trace", ActivityKind.Client);

        for (var timeToLive = 1; timeToLive <= maxHops; timeToLive++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var probe = await sendProbe(target, timeToLive, timeoutPerHop, cancellationToken);
            instrumentation.Sent(PayloadLength);

            var answered = probe.Status != IPStatus.TimedOut && probe.Address is not null;
            if (answered)
            {
                instrumentation.Received(PayloadLength);
            }

            yield return new TracerouteHop(
                timeToLive,
                answered ? probe.Address : null,
                answered ? probe.RoundtripTime : null,
                probe.Status,
                IsDestination: probe.Status == IPStatus.Success);

            if (!IsRouterOrSilentHop(probe.Status))
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Whether the trace goes on after a hop with this status: a router answered, or nothing answered.
    /// The target (Success) and errors such as "unreachable" end it.
    /// </summary>
    private static bool IsRouterOrSilentHop(IPStatus status)
        => status is IPStatus.TtlExpired or IPStatus.TimeExceeded or IPStatus.TimedOut;

    private static async Task<TracerouteProbeResult> SendProbe(
        Ping ping,
        IPAddress target,
        int timeToLive,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var reply = await ping.SendPingAsync(target, timeout, Payload, new PingOptions(timeToLive, dontFragment: true), cancellationToken);
        stopwatch.Stop();

        // The reply's round-trip time is only set for the target's echo reply; for a router's "time exceeded"
        // it is 0 on Windows, so measure it.
        var roundtripTime = reply.Status == IPStatus.Success
            ? TimeSpan.FromMilliseconds(reply.RoundtripTime)
            : stopwatch.Elapsed;

        var address = reply.Address is null || reply.Address.Equals(IPAddress.Any) || reply.Address.Equals(IPAddress.IPv6Any)
            ? null
            : reply.Address;

        return new TracerouteProbeResult(reply.Status, address, roundtripTime);
    }
}