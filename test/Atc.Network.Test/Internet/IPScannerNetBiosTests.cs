namespace Atc.Network.Test.Internet;

public sealed class IPScannerNetBiosTests
{
    [Fact]
    public async Task Scan_ResolveNetBiosName_SetsTheNodeStatus()
    {
        // Arrange
        using var responder = new FakeResponder(request =>
        [
            NetBiosResponseBuilder.Create(
                NetBiosResponseBuilder.ReadTransactionId(request),
                [("DESKTOP-ABC", 0x00, false), ("WORKGROUP", 0x00, true)],
                [0x00, 0x15, 0x5D, 0x01, 0x02, 0x03]),
        ]);
        using var ipScanner = CreateScanner(responder, resolveNetBiosName: true);

        // Act
        var results = await ipScanner.Scan(IPAddress.Loopback, CancellationToken.None);

        // Assert
        var result = Assert.Single(results.CollectedResults);
        Assert.NotNull(result.NetBiosStatus);
        Assert.Equal("DESKTOP-ABC", result.NetBiosStatus.ComputerName);
        Assert.Equal("WORKGROUP", result.NetBiosStatus.Workgroup);
        Assert.True(result.HasConnection);
        Assert.Contains("DESKTOP-ABC", result.ToString(), StringComparison.Ordinal);
        Assert.Equal(100, results.PercentageCompleted);
    }

    [Fact]
    public async Task Scan_ResolveNetBiosNameOff_SendsNoQuery()
    {
        // Arrange
        using var responder = new FakeResponder(_ => []);
        using var ipScanner = CreateScanner(responder, resolveNetBiosName: false);

        // Act
        var results = await ipScanner.Scan(IPAddress.Loopback, CancellationToken.None);

        // Assert
        var result = Assert.Single(results.CollectedResults);
        Assert.Null(result.NetBiosStatus);
        Assert.Empty(responder.Requests);
    }

    [Fact]
    public async Task Scan_NoNetBiosAnswer_LeavesTheNodeStatusEmpty()
    {
        // Arrange
        using var responder = new FakeResponder(_ => []);
        using var ipScanner = CreateScanner(responder, resolveNetBiosName: true);

        // Act
        var results = await ipScanner.Scan(IPAddress.Loopback, CancellationToken.None);

        // Assert
        var result = Assert.Single(results.CollectedResults);
        Assert.Null(result.NetBiosStatus);
        Assert.Single(responder.Requests);
        Assert.Equal(100, results.PercentageCompleted);
    }

    [Fact]
    public void GetTasksToProcessCount_CountsTheNetBiosQuery()
    {
        // Arrange
        var config = new IPScannerConfig
        {
            IcmpPing = false,
            ResolveHostName = false,
            ResolveMacAddress = false,
            ResolveVendorFromMacAddress = false,
            TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
            ResolveNetBiosName = true,
        };

        // Act & Assert
        Assert.Equal(1, config.GetTasksToProcessCount());
    }

    private static IPScanner CreateScanner(
        FakeResponder responder,
        bool resolveNetBiosName)
        => new(
            NullLogger.Instance,
            new IPScannerConfig
            {
                IcmpPing = false,
                ResolveHostName = false,
                ResolveMacAddress = false,
                ResolveVendorFromMacAddress = false,
                TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
                ResolveNetBiosName = resolveNetBiosName,
                TimeoutNetBios = TimeSpan.FromMilliseconds(300),
            })
        {
            NetBiosPort = responder.EndPoint.Port,
        };
}