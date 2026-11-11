namespace Atc.Network.Test.Helpers;

public sealed class ArpHelperTests
{
    [Fact]
    public void GetArpResult()
        => Assert.NotNull(ArpHelper.GetArpResult());

    [Theory]
    [InlineData(0, false)]
    [InlineData(30, false)]
    [InlineData(89, false)]
    [InlineData(90, true)]
    [InlineData(100, true)]
    [InlineData(3600, true)]
    public void IsCacheExpired(
        int cacheAgeInSeconds,
        bool expected)
    {
        // Act
        var result = ArpHelper.IsCacheExpired(TimeSpan.FromSeconds(cacheAgeInSeconds));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsLoopbackAddress_WithLoopbackAddress_ReturnsTrue()
    {
        // Arrange
        var loopbackAddress = IPAddress.Parse("127.0.0.1");

        // Act
        var result = ArpHelper.IsLoopbackAddress(loopbackAddress);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLoopbackAddress_WithNonLoopbackAddress_ReturnsFalse()
    {
        // Arrange
        var nonLoopbackAddress = IPAddress.Parse("192.168.1.1");

        // Act
        var result = ArpHelper.IsLoopbackAddress(nonLoopbackAddress);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetLoopbackArpEntity_ReturnsCorrectEntity()
    {
        // Arrange
        var loopbackAddress = IPAddress.Parse("127.0.0.1");

        // Act
        var result = ArpHelper.GetLoopbackArpEntity(loopbackAddress);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loopbackAddress, result.IPAddress);
        Assert.Equal(ArpHelper.LoopbackMacAddress, result.MacAddress);
        Assert.Equal(ArpHelper.LoopbackType, result.Type);
    }

    [Fact]
    public void IsLocalMachineAddress_WithLocalIp_ReturnsTrue()
    {
        // Arrange
        var localAddress = IPv4AddressHelper.GetLocalAddress();
        if (localAddress == null)
        {
            // Skip test if no local address is found
            return;
        }

        // Act
        var result = ArpHelper.IsLocalMachineAddress(localAddress);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsLocalMachineAddress_WithNonLocalIp_ReturnsFalse()
    {
        // Arrange
        var nonLocalAddress = IPAddress.Parse("8.8.8.8");

        // Act
        var result = ArpHelper.IsLocalMachineAddress(nonLocalAddress);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetLocalMachineArpEntity_ReturnsValidEntity()
    {
        // Arrange
        var localAddress = IPv4AddressHelper.GetLocalAddress();
        if (localAddress == null)
        {
            // Skip test if no local address is found
            return;
        }

        // Act
        var result = ArpHelper.GetLocalMachineArpEntity(localAddress);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(localAddress, result.IPAddress);
        Assert.NotNull(result.MacAddress);
        Assert.Equal(ArpHelper.LoopbackType, result.Type);
    }
}