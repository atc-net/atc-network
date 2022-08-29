namespace Atc.Network.Test.Internet;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "OK.")]
[Trait(Traits.Category, Traits.Categories.Integration)]
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public class IPPortScanTests
{
    [Fact]
    public async Task Try_CanConnectWithTcp()
    {
        // Arrange
        var testIpAddress = GetTestIpAddress();

        var ipPortScan = new IPPortScan(testIpAddress);

        // Act
        var actual = await ipPortScan.CanConnectWithTcp(
            80,
            CancellationToken.None);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task Try_CanConnectWithHttp()
    {
        // Arrange
        var testIpAddress = GetTestIpAddress();

        var ipPortScan = new IPPortScan(testIpAddress);

        // Act
        var actual = await ipPortScan.CanConnectWithHttp(
                80,
                CancellationToken.None);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task Try_CanConnectWithHttps()
    {
        // Arrange
        var testIpAddress = GetTestIpAddress();

        var ipPortScan = new IPPortScan(testIpAddress);

        // Act
        var actual = await ipPortScan.CanConnectWithHttps(
            80,
            CancellationToken.None);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task Try_CanConnectWithHttpOrHttps()
    {
        // Arrange
        var testIpAddress = GetTestIpAddress();

        var ipPortScan = new IPPortScan(testIpAddress);

        // Act
        var actual = await ipPortScan.CanConnectWithHttpOrHttps(
            80,
            true,
            CancellationToken.None);

        // Assert
        Assert.False(actual);
    }

    private static IPAddress GetTestIpAddress()
    {
        var localAddress = IPv4AddressHelper.GetLocalAddress();
        return localAddress ?? IPAddress.Parse("8.8.8.8");
    }
}