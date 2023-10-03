namespace Atc.Network.Test.Data;

public class IPServicePortListsTests
{
    [Fact]
    public void GetWellKnown()
    {
        // Act
        var actual = IPServicePortLists.GetWellKnown();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(9, actual.Count);
    }

    [Theory]
    [InlineData(0, ServiceProtocolType.None)]
    [InlineData(0, ServiceProtocolType.Unknown)]
    [InlineData(1, ServiceProtocolType.Https)]
    [InlineData(1, ServiceProtocolType.Http)]
    [InlineData(2, ServiceProtocolType.Ftps)]
    [InlineData(2, ServiceProtocolType.Ftp)]
    [InlineData(1, ServiceProtocolType.Ssh)]
    [InlineData(1, ServiceProtocolType.Telnet)]
    public void GetWellKnown_ServiceProtocolType(int expected, ServiceProtocolType serviceProtocolType)
    {
        // Act
        var actual = IPServicePortLists.GetWellKnown(serviceProtocolType);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual.Count);
    }

    [Fact]
    public void GetWellKnownOrCommon()
    {
        // Act
        var actual = IPServicePortLists.GetWellKnownOrCommon();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(60, actual.Count);
    }

    [Theory]
    [InlineData(0, ServiceProtocolType.None)]
    [InlineData(0, ServiceProtocolType.Unknown)]
    [InlineData(20, ServiceProtocolType.Https)]
    [InlineData(31, ServiceProtocolType.Http)]
    [InlineData(3, ServiceProtocolType.Ftps)]
    [InlineData(2, ServiceProtocolType.Ftp)]
    [InlineData(1, ServiceProtocolType.Ssh)]
    [InlineData(1, ServiceProtocolType.Telnet)]
    public void GetWellKnownOrCommon_ServiceProtocolType(int expected, ServiceProtocolType serviceProtocolType)
    {
        // Act
        var actual = IPServicePortLists.GetWellKnownOrCommon(serviceProtocolType);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual.Count);
    }
}