namespace Atc.Network.Test.Vnc;

public class VncClientReconnectConfigTests
{
    [Fact]
    public void Default_Values_Are_Correct()
    {
        // Arrange & Act
        var config = new VncClientReconnectConfig();

        // Assert
        Assert.True(config.Enable);
        Assert.Equal(2_000, config.RetryInterval);
        Assert.Equal(1800, config.RetryMaxAttempts);
    }

    [Fact]
    public void Custom_Values_Are_Applied()
    {
        // Arrange & Act
        var config = new VncClientReconnectConfig
        {
            Enable = false,
            RetryInterval = 5_000,
            RetryMaxAttempts = 100,
        };

        // Assert
        Assert.False(config.Enable);
        Assert.Equal(5_000, config.RetryInterval);
        Assert.Equal(100, config.RetryMaxAttempts);
    }

    [Fact]
    public void ToString_Contains_All_Properties()
    {
        // Arrange
        var config = new VncClientReconnectConfig();

        // Act
        var result = config.ToString();

        // Assert
        Assert.Contains("Enable", result, StringComparison.Ordinal);
        Assert.Contains("RetryInterval", result, StringComparison.Ordinal);
    }
}