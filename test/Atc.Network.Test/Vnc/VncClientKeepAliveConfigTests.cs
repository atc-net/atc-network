namespace Atc.Network.Test.Vnc;

public class VncClientKeepAliveConfigTests
{
    [Fact]
    public void Default_Values_Are_Correct()
    {
        // Arrange & Act
        var config = new VncClientKeepAliveConfig();

        // Assert
        Assert.True(config.Enable);
        Assert.Equal(2, config.Interval);
        Assert.Equal(2, config.Time);
        Assert.Equal(3, config.RetryCount);
    }

    [Fact]
    public void ToString_Contains_All_Properties()
    {
        // Arrange
        var config = new VncClientKeepAliveConfig();

        // Act
        var result = config.ToString();

        // Assert
        Assert.Contains("Enable", result, StringComparison.Ordinal);
        Assert.Contains("Interval", result, StringComparison.Ordinal);
        Assert.Contains("Time", result, StringComparison.Ordinal);
        Assert.Contains("RetryCount", result, StringComparison.Ordinal);
    }
}