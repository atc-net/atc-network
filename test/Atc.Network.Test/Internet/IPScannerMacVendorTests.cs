namespace Atc.Network.Test.Internet;

public sealed class IPScannerMacVendorTests
{
    [Fact]
    public async Task ResolveMacVendor_RemoteAddress_UsesTheGivenLookup()
    {
        // Arrange - 192.0.2.10 is a documentation address, so it is never the local machine.
        var lookup = new RecordingLookup("Offline Vendor");
        using var scanner = new IPScanner(NullLogger.Instance, new IPScannerConfig(), lookup);

        // Act
        var vendor = await scanner.ResolveMacVendor(IPAddress.Parse("192.0.2.10"), "00:11:22:33:44:55", CancellationToken.None);

        // Assert
        Assert.Equal("Offline Vendor", vendor);
        Assert.Equal(["00:11:22:33:44:55"], lookup.MacAddresses, StringComparer.Ordinal);
    }

    [Fact]
    public async Task ResolveMacVendor_LoopbackMacAddress_DoesNotCallTheLookup()
    {
        // Arrange
        var lookup = new RecordingLookup("Offline Vendor");
        using var scanner = new IPScanner(NullLogger.Instance, new IPScannerConfig(), lookup);

        // Act
        var vendor = await scanner.ResolveMacVendor(IPAddress.Loopback, ArpHelper.LoopbackMacAddress, CancellationToken.None);

        // Assert
        Assert.Equal("Loopback Interface", vendor);
        Assert.Empty(lookup.MacAddresses);
    }

    private sealed class RecordingLookup(string vendorName) : IMacAddressVendorLookup
    {
        public ConcurrentQueue<string> MacAddresses { get; } = new();

        public Task<string?> LookupVendorName(
            string macAddress,
            CancellationToken cancellationToken = default)
        {
            MacAddresses.Enqueue(macAddress);
            return Task.FromResult<string?>(vendorName);
        }
    }
}