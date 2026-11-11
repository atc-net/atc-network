namespace Atc.Network.Test.Helpers;

/// <summary>
/// How PingHelper turns a ping reply into a result. No network is used.
/// </summary>
public sealed class PingHelperResultTests
{
    [Fact]
    public void CreateResult_Success_UsesTheRoundTripTimeOfTheReply()
    {
        // Act - the call took 80 ms (the first Ping in a process is slow), the reply took 0 ms.
        var actual = PingHelper.CreateResult(IPAddress.Loopback, IPStatus.Success, roundtripTimeInMs: 0, elapsedInMs: 80);

        // Assert
        Assert.Equal(0, actual.PingInMs);
        Assert.Equal(NetworkQualityCategoryType.Perfect, actual.QualityCategory);
    }

    [Fact]
    public void CreateResult_NoReply_UsesTheElapsedTime()
    {
        // Act - without a reply the round-trip time is 0, so it says nothing about the time.
        var actual = PingHelper.CreateResult(IPAddress.Loopback, IPStatus.DestinationUnreachable, roundtripTimeInMs: 0, elapsedInMs: 80);

        // Assert
        Assert.Equal(80, actual.PingInMs);
    }
}