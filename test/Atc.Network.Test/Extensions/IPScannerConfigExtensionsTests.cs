namespace Atc.Network.Test.Extensions;

public class IPScannerConfigExtensionsTests
{
    [Theory]
    [InlineData(4, true, true, true, true, 0, true)]
    [InlineData(3, false, true, true, true, 0, true)]
    [InlineData(2, false, false, true, true, 0, true)]
    [InlineData(0, false, false, false, true, 0, true)]
    [InlineData(0, false, false, false, false, 0, true)]
    [InlineData(1, false, false, true, false, 0, true)]
    [InlineData(10, true, true, true, true, 3, true)]
    [InlineData(9, false, true, true, true, 3, true)]
    [InlineData(8, false, false, true, true, 3, true)]
    [InlineData(6, false, false, false, true, 3, true)]
    [InlineData(6, false, false, false, false, 3, true)]
    [InlineData(7, false, false, true, false, 3, true)]
    public void GetTasksToProcessCount(
        int expected,
        bool resolvePing,
        bool resolveHostName,
        bool resolveMacAddress,
        bool resolveVendorFromMacAddress,
        int numberOfPorts,
        bool resolveIPProtocolHttp)
    {
        // Arrange
        var ipScannerConfig = new IPScannerConfig
        {
            ResolvePing = resolvePing,
            ResolveHostName = resolveHostName,
            ResolveMacAddress = resolveMacAddress,
            ResolveVendorFromMacAddress = resolveVendorFromMacAddress,
            ResolveServiceProtocolHttp = resolveIPProtocolHttp,
        };

        for (var i = 0; i < numberOfPorts; i++)
        {
            ipScannerConfig.PortNumbers.Add(i + 10);
        }

        // Atc
        var actual = ipScannerConfig.GetTasksToProcessCount();

        // Assert
        Assert.Equal(expected, actual);
    }
}