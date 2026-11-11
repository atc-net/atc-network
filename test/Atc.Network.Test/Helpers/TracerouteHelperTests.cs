namespace Atc.Network.Test.Helpers;

public sealed class TracerouteHelperTests
{
    private static readonly IPAddress Target = IPAddress.Parse("203.0.113.10");
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    [Fact]
    public async Task Trace_ReturnsEachHopUntilTheTarget()
    {
        // Arrange - two routers, then the target.
        var probes = FakeProbes(
            (IPStatus.TtlExpired, "192.168.1.1", 1),
            (IPStatus.TtlExpired, "10.0.0.1", 8),
            (IPStatus.Success, "203.0.113.10", 20));

        // Act
        var actual = await TracerouteHelper.Trace(Target, maxHops: 30, Timeout, probes.Send, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal(3, actual.Count);
        Assert.Equal([1, 2, 3], actual.Select(x => x.HopNumber));
        Assert.Equal(IPAddress.Parse("192.168.1.1"), actual[0].IPAddress);
        Assert.Equal(TimeSpan.FromMilliseconds(8), actual[1].RoundtripTime);
        Assert.False(actual[1].IsDestination);
        Assert.True(actual[2].IsDestination);
        Assert.Equal([1, 2, 3], probes.TimeToLives);
    }

    [Fact]
    public async Task Trace_HopWithoutAnswer_HasNoAddressAndTheTraceContinues()
    {
        // Arrange - the second router does not answer.
        var probes = FakeProbes(
            (IPStatus.TtlExpired, "192.168.1.1", 1),
            (IPStatus.TimedOut, null, 0),
            (IPStatus.Success, "203.0.113.10", 20));

        // Act
        var actual = await TracerouteHelper.Trace(Target, maxHops: 30, Timeout, probes.Send, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal(3, actual.Count);
        Assert.Null(actual[1].IPAddress);
        Assert.Null(actual[1].RoundtripTime);
        Assert.Equal(IPStatus.TimedOut, actual[1].Status);
        Assert.True(actual[2].IsDestination);
    }

    [Fact]
    public async Task Trace_TargetNeverReached_StopsAtMaxHops()
    {
        // Arrange - every hop times out.
        var probes = new FakeProbeSender(_ => (IPStatus.TimedOut, null, 0));

        // Act
        var actual = await TracerouteHelper.Trace(Target, maxHops: 4, Timeout, probes.Send, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal(4, actual.Count);
        Assert.All(actual, x => Assert.False(x.IsDestination));
    }

    [Fact]
    public async Task Trace_DestinationUnreachable_IsTheLastHop()
    {
        // Arrange - a router reports that the target can't be reached.
        var probes = FakeProbes(
            (IPStatus.TtlExpired, "192.168.1.1", 1),
            (IPStatus.DestinationHostUnreachable, "10.0.0.1", 5),
            (IPStatus.Success, "203.0.113.10", 20));

        // Act
        var actual = await TracerouteHelper.Trace(Target, maxHops: 30, Timeout, probes.Send, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Equal(IPStatus.DestinationHostUnreachable, actual[1].Status);
        Assert.False(actual[1].IsDestination);
    }

    [Fact]
    public async Task Trace_TimeExceeded_IsARouterHop()
    {
        // Arrange - for IPv6, a router's "time exceeded" can be reported as TimeExceeded instead of TtlExpired.
        var probes = FakeProbes(
            (IPStatus.TimeExceeded, "2001:db8::1", 3),
            (IPStatus.Success, "2001:db8::10", 9));

        // Act
        var actual = await TracerouteHelper.Trace(IPAddress.Parse("2001:db8::10"), maxHops: 30, Timeout, probes.Send, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.True(actual[1].IsDestination);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(256)]
    public Task Trace_MaxHopsOutOfRange_Throws(int maxHops)
        => Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await TracerouteHelper.Trace(Target, maxHops, Timeout, FakeProbes().Send, CancellationToken.None).ToListAsync());

    [Fact]
    public async Task Trace_Cancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var probes = new FakeProbeSender(ttl =>
        {
            if (ttl == 2)
            {
                cancellationTokenSource.Cancel();
            }

            return (IPStatus.TtlExpired, "192.168.1.1", 1);
        });

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await TracerouteHelper.Trace(Target, maxHops: 30, Timeout, probes.Send, cancellationTokenSource.Token).ToListAsync());
    }

    [Fact]
    public async Task Trace_RecordsProbesAndAnActivity()
    {
        // Arrange
        using var telemetry = new TelemetryCollector();
        var probes = FakeProbes(
            (IPStatus.TimedOut, null, 0),
            (IPStatus.Success, "203.0.113.10", 20));

        // Act
        await TracerouteHelper.Trace(Target, maxHops: 30, Timeout, probes.Send, CancellationToken.None).ToListAsync();

        // Assert - two probes sent, one answered. ICMP has no port, so the port tag is 0.
        Assert.Equal(2, telemetry.Sum(NetworkTelemetry.MessagesSentInstrumentName, "traceroute", 0));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesReceivedInstrumentName, "traceroute", 0));
        Assert.Single(telemetry.Activities("Traceroute.Trace", 0));
    }

    [Fact]
    public void TracerouteHop_ToString_LooksLikeATracerouteLine()
    {
        // Assert
        Assert.Equal("1  192.168.1.1  1 ms", new TracerouteHop(1, IPAddress.Parse("192.168.1.1"), TimeSpan.FromMilliseconds(1), IPStatus.TtlExpired, IsDestination: false).ToString());
        Assert.Equal("2  *", new TracerouteHop(2, null, null, IPStatus.TimedOut, IsDestination: false).ToString());
    }

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task Trace_Loopback_IsOneHopToTheTarget()
    {
        // Act
        var actual = await TracerouteHelper.Trace(IPAddress.Loopback).ToListAsync();

        // Assert
        var hop = Assert.Single(actual);
        Assert.True(hop.IsDestination);
        Assert.Equal(IPAddress.Loopback, hop.IPAddress);
    }

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task Trace_PublicAddress_PassesThroughRouters()
    {
        // Act - 1.1.1.1 is a few hops away from any network with internet access.
        var actual = await TracerouteHelper.Trace(IPAddress.Parse("1.1.1.1"), maxHops: 30, TimeSpan.FromSeconds(1)).ToListAsync();

        // Assert
        Assert.True(actual.Count > 1, $"Expected routers before the target, got {actual.Count} hop(s).");
        Assert.Contains(actual, x => x.Status == IPStatus.TtlExpired && x.IPAddress is not null);
    }

    private static FakeProbeSender FakeProbes(
        params (IPStatus Status, string? Address, long RoundtripInMs)[] hops)
        => new(ttl => ttl <= hops.Length ? hops[ttl - 1] : (IPStatus.TimedOut, null, 0));

    /// <summary>
    /// Answers each probe as a router or the target would, by its time to live.
    /// </summary>
    private sealed class FakeProbeSender
    {
        private readonly Func<int, (IPStatus Status, string? Address, long RoundtripInMs)> answer;

        public FakeProbeSender(
            Func<int, (IPStatus Status, string? Address, long RoundtripInMs)> answer)
        {
            this.answer = answer;
        }

        public List<int> TimeToLives { get; } = [];

        public Task<TracerouteProbeResult> Send(
            IPAddress target,
            int timeToLive,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            TimeToLives.Add(timeToLive);
            var (status, address, roundtripInMs) = answer(timeToLive);
            return Task.FromResult(new TracerouteProbeResult(
                status,
                address is null ? null : IPAddress.Parse(address),
                TimeSpan.FromMilliseconds(roundtripInMs)));
        }
    }
}