namespace Atc.Network.Test.Internet;

/// <summary>
/// Port scanning speed. These tests depend on the network dropping packets to 192.0.2.1
/// (TEST-NET-1, reserved for documentation and never routed), so they are integration tests.
/// </summary>
[Trait(Traits.Category, Traits.Categories.Integration)]
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class IPScannerPortScanTests
{
    private const int PortCount = 10;
    private const int TimeoutTcpInMs = 300;

    [Fact]
    public async Task Scan_PortsThatTimeOut_AreScannedConcurrently()
    {
        // Arrange
        var unroutableAddress = IPAddress.Parse("192.0.2.1");
        List<ushort> portNumbers = [.. Enumerable.Range(10_000, PortCount).Select(x => (ushort)x)];
        using var ipScanner = new IPScanner(
            NullLogger.Instance,
            new IPScannerConfig
            {
                IcmpPing = false,
                ResolveHostName = false,
                ResolveMacAddress = false,
                ResolveVendorFromMacAddress = false,
                TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
                TimeoutTcp = TimeSpan.FromMilliseconds(TimeoutTcpInMs),
                PortNumbers = portNumbers,
            });

        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = await ipScanner.Scan(unroutableAddress, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        var scanResult = Assert.Single(results.CollectedResults);
        Assert.Equal(PortCount, scanResult.Ports.Count);
        Assert.Empty(scanResult.OpenPortNumbers);

        // One after another, the ports take at least PortCount x timeout (3 s).
        Assert.True(
            stopwatch.ElapsedMilliseconds < PortCount * TimeoutTcpInMs / 2,
            $"Scanning {PortCount} ports took {stopwatch.ElapsedMilliseconds} ms; they should be scanned concurrently.");
    }
}