namespace Atc.Network.Test.Helpers;

public class IPAddressV4HelperTests
{
    [Theory]
    [InlineData(true, "10.50.30.7", "10.50.30.7")]
    [InlineData(true, "10.50.30.7", "10.50.30.70")]
    [InlineData(false, "10.50.30.7", "10.50.30.6")]
    [InlineData(true, "10.50.30.7", "10.50.31.7")]
    [InlineData(false, "10.50.31.7", "10.50.30.7")]
    public void ValidateAddresses(bool expected, string ipAddressStart, string ipAddressEnd)
    {
        // Atc
        var (isValid, _) = IPAddressV4Helper.ValidateAddresses(
            IPAddress.Parse(ipAddressStart),
            IPAddress.Parse(ipAddressEnd));

        // Asset
        Assert.Equal(expected, isValid);
    }

    [Fact]
    public void GetLocalAddress()
        => Assert.NotNull(IPAddressV4Helper.GetLocalAddress());

    [Theory]
    [InlineData(1, "10.50.30.7", "10.50.30.7")]
    [InlineData(64, "10.50.30.7", "10.50.30.70")]
    [InlineData(255, "10.50.30.1", "10.50.30.255")]
    [InlineData(320, "10.50.30.7", "10.50.31.70")]
    public void GetAddressesInRange(int expected, string ipAddressStart, string ipAddressEnd)
    {
        // Atc
        var actual = IPAddressV4Helper.GetAddressesInRange(
            IPAddress.Parse(ipAddressStart),
            IPAddress.Parse(ipAddressEnd));

        // Asset
        Assert.Equal(expected, actual.Length);
    }

    [Theory]
    [InlineData(256, "10.0.0.0", 24)]
    [InlineData(128, "10.0.0.0", 25)]
    [InlineData(64, "10.0.0.0", 26)]
    [InlineData(32, "10.0.0.0", 27)]
    [InlineData(16, "10.0.0.0", 28)]
    [InlineData(8, "10.0.0.0", 29)]
    [InlineData(4, "10.0.0.0", 30)]
    [InlineData(2, "10.0.0.0", 31)]
    [InlineData(1, "10.0.0.0", 32)]
    public void GetAddressesInRange_Cidr(int expected, string ipAddress, int cidrMaskLength)
    {
        // Atc
        var actual = IPAddressV4Helper.GetAddressesInRange(
            IPAddress.Parse(ipAddress),
            cidrMaskLength);

        // Asset
        Assert.Equal(expected, actual.Length);
    }

    [Theory]
    [InlineData("10.0.0.0", "10.0.0.255", "10.0.0.0", 24)]
    [InlineData("10.0.0.0", "10.0.0.127", "10.0.0.0", 25)]
    [InlineData("10.0.0.0", "10.0.0.63", "10.0.0.0", 26)]
    [InlineData("10.0.0.0", "10.0.0.31", "10.0.0.0", 27)]
    [InlineData("10.0.0.0", "10.0.0.15", "10.0.0.0", 28)]
    [InlineData("10.0.0.0", "10.0.0.7", "10.0.0.0", 29)]
    [InlineData("10.0.0.0", "10.0.0.3", "10.0.0.0", 30)]
    [InlineData("10.0.0.0", "10.0.0.1", "10.0.0.0", 31)]
    [InlineData("10.0.0.0", "10.0.0.0", "10.0.0.0", 32)]
    [InlineData("192.168.0.0", "192.168.0.255", "192.168.0.7", 24)]
    public void GetStartAndEndAddressesInRange(string expected1, string expected2, string ipAddress, int cidrMaskLength)
    {
        // Atc
        var actual = IPAddressV4Helper.GetStartAndEndAddressesInRange(
            IPAddress.Parse(ipAddress),
            cidrMaskLength);

        // Asset
        Assert.Equal(IPAddress.Parse(expected1), actual.StartIpAddress);
        Assert.Equal(IPAddress.Parse(expected2), actual.EndIpAddress);
    }

    [Theory]
    [InlineData(true, "10.50.30.7", "10.0.0.0/8")]
    public void IsAddressInRange(bool expected, string ipAddress, string cidrMask)
        => Assert.Equal(
            expected,
            IPAddressV4Helper.IsAddressInRange(
                IPAddress.Parse(ipAddress),
                cidrMask));
}