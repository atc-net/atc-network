namespace Atc.Network.Test.Vnc;

public class VncClientConfigTests
{
    [Fact]
    public void Default_Values_Are_Correct()
    {
        // Arrange & Act
        var config = new VncClientConfig();

        // Assert
        Assert.Equal(10_000, config.ConnectTimeout);
        Assert.Equal(30_000, config.SendTimeout);
        Assert.Equal(30_000, config.ReceiveTimeout);
        Assert.Equal(32, config.BitsPerPixel);
        Assert.Equal(24, config.Depth);
        Assert.False(config.ViewOnly);
        Assert.True(config.SharedDesktop);
        Assert.Equal(5900, config.Port);
    }

    [Fact]
    public void ToString_Contains_All_Properties()
    {
        // Arrange
        var config = new VncClientConfig();

        // Act
        var result = config.ToString();

        // Assert
        Assert.Contains("ConnectTimeout", result, StringComparison.Ordinal);
        Assert.Contains("SendTimeout", result, StringComparison.Ordinal);
        Assert.Contains("BitsPerPixel", result, StringComparison.Ordinal);
        Assert.Contains("Depth", result, StringComparison.Ordinal);
        Assert.Contains("ViewOnly", result, StringComparison.Ordinal);
        Assert.Contains("SharedDesktop", result, StringComparison.Ordinal);
        Assert.Contains("Port", result, StringComparison.Ordinal);
    }
}