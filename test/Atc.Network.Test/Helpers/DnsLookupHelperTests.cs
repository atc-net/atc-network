namespace Atc.Network.Test.Helpers;

public class DnsLookupHelperTests
{
    [Fact]
    public async Task GetHostname()
    {
        // Arrange
        var localAddress = IPv4AddressHelper.GetLocalAddress();
        if (localAddress is null)
        {
            return;
        }

        // Act
        var actual = await DnsLookupHelper.GetHostname(localAddress, CancellationToken.None);

        // Assert
        Assert.NotNull(actual);
    }
}