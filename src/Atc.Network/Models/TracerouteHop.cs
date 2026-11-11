namespace Atc.Network.Models;

/// <summary>
/// One hop of a traceroute: a router on the way, or the target.
/// </summary>
/// <param name="HopNumber">The hop's number, starting at 1: the time to live of the probe.</param>
/// <param name="IPAddress">The address that answered, or <see langword="null"/> when nothing answered in time.</param>
/// <param name="RoundtripTime">The time until the answer, or <see langword="null"/> when nothing answered in time.</param>
/// <param name="Status">
/// <see cref="IPStatus.TtlExpired"/> (or <see cref="IPStatus.TimeExceeded"/>) for a router, <see cref="IPStatus.Success"/> for the target,
/// <see cref="IPStatus.TimedOut"/> when nothing answered, or another status, such as <see cref="IPStatus.DestinationHostUnreachable"/>,
/// that ends the trace.
/// </param>
/// <param name="IsDestination">Whether the target itself answered.</param>
public sealed record TracerouteHop(
    int HopNumber,
    IPAddress? IPAddress,
    TimeSpan? RoundtripTime,
    IPStatus Status,
    bool IsDestination)
{
    /// <summary>
    /// Returns the hop as a traceroute line: "1  192.168.1.1  1 ms", or "2  *" when nothing answered.
    /// </summary>
    /// <returns>The line.</returns>
    public override string ToString()
        => IPAddress is null
            ? $"{HopNumber}  *"
            : $"{HopNumber}  {IPAddress}  {(long)(RoundtripTime ?? TimeSpan.Zero).TotalMilliseconds} ms";
}