// ReSharper disable RedundantCast
namespace Atc.Network.Test.Internet;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "OK.")]
[Trait(Traits.Category, Traits.Categories.Integration)]
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public class IPScannerTests
{
    private static readonly List<ushort> TestPortNumbers = new()
    {
        80,
    };

    [Fact]
    public async Task Scan()
    {
        // Arrange
        var testIpAddress = GetTestIpAddress();
        var ipScannerConfig = new IPScannerConfig
        {
            PortNumbers = TestPortNumbers,
        };

        var ipScanner = new IPScanner(ipScannerConfig);
        ipScanner.ProgressReporting += IpScannerOnProgressReporting;

        // Act
        var actual = await ipScanner.Scan(
            testIpAddress,
            CancellationToken.None);

        // Assert
        Assert.NotNull(actual);
        Trace.TraceInformation(actual.ToString());
    }

    [Fact]
    public async Task ScanCidrRange()
    {
        // Arrange
        var testIpAddress = GetTestIpAddress();
        const int cidrMaskLength = 31;
        var ipScannerConfig = new IPScannerConfig
        {
            PortNumbers = TestPortNumbers,
        };

        var ipScanner = new IPScanner(ipScannerConfig);
        ipScanner.ProgressReporting += IpScannerOnProgressReporting;

        // Act
        var actual = await ipScanner.ScanCidrRange(
            testIpAddress,
            cidrMaskLength,
            CancellationToken.None);

        // Assert
        Assert.NotNull(actual);
        Trace.TraceInformation(actual.ToString());
    }

    [Fact]
    public async Task ScanRange()
    {
        // Arrange
        var testIpAddressStart = GetTestIpAddress(-1);
        var testIpAddressEnd = GetTestIpAddress(1);
        var ipScannerConfig = new IPScannerConfig
        {
            PortNumbers = TestPortNumbers,
        };

        var ipScanner = new IPScanner(ipScannerConfig);
        ipScanner.ProgressReporting += IpScannerOnProgressReporting;

        // Act
        var actual = await ipScanner.ScanRange(
            testIpAddressStart,
            testIpAddressEnd,
            CancellationToken.None);

        // Assert
        Assert.NotNull(actual);
        Trace.TraceInformation(actual.ToString());
    }

    private static IPAddress GetTestIpAddress()
    {
        var localAddress = IPv4AddressHelper.GetLocalAddress();
        return localAddress ?? IPAddress.Parse("8.8.8.8");
    }

    private static IPAddress GetTestIpAddress(int i)
    {
        var bytes = GetTestIpAddress().GetAddressBytes();
        return IPAddress.Parse($"{(int)bytes[0]}.{(int)bytes[1]}.{(int)bytes[2]}.{(int)bytes[3] + i}");
    }

    private static void IpScannerOnProgressReporting(object? sender, IPScannerProgressReport args)
    {
        var t = $"{DateTime.Now.Minute}.{DateTime.Now.Second}.{DateTime.Now.Millisecond}";
        Trace.TraceInformation($"{t} # {args}");
    }
}