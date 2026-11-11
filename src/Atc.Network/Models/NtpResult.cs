namespace Atc.Network.Models;

/// <summary>
/// The time from an NTP server, and how far the local clock is from it.
/// </summary>
public sealed class NtpResult
{
    public NtpResult(
        IPEndPoint server,
        int stratum,
        TimeSpan offset,
        TimeSpan roundTripDelay,
        DateTime utcTime)
    {
        Server = server;
        Stratum = stratum;
        Offset = offset;
        RoundTripDelay = roundTripDelay;
        UtcTime = utcTime;
    }

    /// <summary>
    /// Gets the server that answered.
    /// </summary>
    public IPEndPoint Server { get; }

    /// <summary>
    /// Gets the server's distance from a reference clock: 1 for a server with its own clock (GPS, atomic),
    /// 2 for a server that synchronizes with a stratum 1 server, and so on.
    /// </summary>
    public int Stratum { get; }

    /// <summary>
    /// Gets how far the server's clock is ahead of the local clock. A negative value means the local clock is ahead.
    /// Add it to <see cref="DateTime.UtcNow"/> to get the server's time.
    /// </summary>
    public TimeSpan Offset { get; }

    /// <summary>
    /// Gets the time the request and the reply spent on the network, without the time the server needed to answer.
    /// </summary>
    public TimeSpan RoundTripDelay { get; }

    /// <summary>
    /// Gets the server's time (UTC) at the moment the reply arrived: the local time then, plus <see cref="Offset"/>.
    /// </summary>
    public DateTime UtcTime { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Server)}: {Server}, {nameof(Stratum)}: {Stratum}, {nameof(Offset)}: {Offset}, {nameof(RoundTripDelay)}: {RoundTripDelay}, {nameof(UtcTime)}: {UtcTime:O}";
}