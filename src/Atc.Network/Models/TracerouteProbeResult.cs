namespace Atc.Network.Models;

/// <summary>
/// The answer to one traceroute probe.
/// </summary>
/// <param name="Status">The answer's status.</param>
/// <param name="Address">The address that answered, or <see langword="null"/>.</param>
/// <param name="RoundtripTime">The time until the answer.</param>
internal sealed record TracerouteProbeResult(
    IPStatus Status,
    IPAddress? Address,
    TimeSpan RoundtripTime);