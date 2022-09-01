namespace Atc.Network.Test.Helpers;

[Trait(Traits.Category, Traits.Categories.Integration)]
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public class PingHelperTests
{
    [Fact]
    public async Task GetStatus_TimeSpan()
    {
        // Act
        var actual = await PingHelper.GetStatus(IPAddress.Loopback, new TimeSpan(0, 0, 0, 1));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(IPAddress.Loopback, actual.IPAddress);
        Assert.Null(actual.Exception);
        Assert.Equal(NetworkQualityCategoryType.Perfect, actual.QualityCategory);
    }

    [Fact]
    public async Task GetStatus_Default_TimeoutInMs()
    {
        // Act
        var actual = await PingHelper.GetStatus(IPAddress.Loopback);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(IPAddress.Loopback, actual.IPAddress);
        Assert.Null(actual.Exception);
        Assert.Equal(NetworkQualityCategoryType.Perfect, actual.QualityCategory);
    }

    [Fact]
    public async Task GetStatus_TimeoutInMs()
    {
        // Act
        var actual = await PingHelper.GetStatus(IPAddress.Loopback, 500);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(IPAddress.Loopback, actual.IPAddress);
        Assert.Null(actual.Exception);
        Assert.Equal(NetworkQualityCategoryType.Perfect, actual.QualityCategory);
    }
}