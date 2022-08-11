namespace Atc.Network.Test.Helpers;

public class DnsLookupHelperTests
{
    [Fact]
    public async Task GetHostname()
    {
        // Arrange
        var localAddress = IPAddressV4Helper.GetLocalAddress();
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