namespace Atc.Network.Test.Extensions;

public class IPAddressExtensionsTests
{
    [Theory]
    [InlineData(false, "10.0.0.0")]
    [InlineData(false, "10.255.255.255")]
    [InlineData(false, "172.16.0.0")]
    [InlineData(false, "172.31.255.255")]
    [InlineData(false, "192.168.0.0")]
    [InlineData(false, "192.168.255.255")]
    [InlineData(true, "172.32.0.0")]
    [InlineData(true, "192.169.0.0")]
    public void IsPublic(bool expected, string ipAddress)
        => Assert.Equal(
            expected,
            IPAddress.Parse(ipAddress).IsPublic());

    [Theory]
    [InlineData(true, "10.0.0.0")]
    [InlineData(true, "10.255.255.255")]
    [InlineData(true, "172.16.0.0")]
    [InlineData(true, "172.31.255.255")]
    [InlineData(true, "192.168.0.0")]
    [InlineData(true, "192.168.255.255")]
    [InlineData(false, "172.32.0.0")]
    [InlineData(false, "192.169.0.0")]
    public void IsPrivate(bool expected, string ipAddress)
        => Assert.Equal(
            expected,
            IPAddress.Parse(ipAddress).IsPrivate());

    [Theory]
    [InlineData(167772160, "10.0.0.0")]
    [InlineData(184549375, "10.255.255.255")]
    [InlineData(2886729728, "172.16.0.0")]
    [InlineData(2887778303, "172.31.255.255")]
    [InlineData(3232235520, "192.168.0.0")]
    [InlineData(3232301055, "192.168.255.255")]
    [InlineData(2887778304, "172.32.0.0")]
    [InlineData(3232301056, "192.169.0.0")]
    public void ToUnsignedInt(uint expected, string ipAddress)
        => Assert.Equal(
            expected,
            IPAddress.Parse(ipAddress).ToUnsignedInt());

    [Theory]
    [InlineData(true, "10.50.30.7", "10.0.0.0/8")]
    public void IsInRange(bool expected, string ipAddress, string cidrNotation)
        => Assert.Equal(
            expected,
            IPAddress.Parse(ipAddress).IsInRange(cidrNotation));
}