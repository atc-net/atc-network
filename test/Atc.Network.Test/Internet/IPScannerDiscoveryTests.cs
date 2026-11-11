namespace Atc.Network.Test.Internet;

public sealed class IPScannerDiscoveryTests
{
    private const string Http = "_http._tcp.local";
    private const string Printer = "Printer._http._tcp.local";

    [Fact]
    public async Task Scan_DiscoverServices_AttachesTheServicesOfTheAddress()
    {
        // Arrange - one service on 127.0.0.1 and one on another address.
        using var mdnsResponder = new FakeResponder(request => DnsResponseBuilder
            .ReadQuestions(request)
            .Select(question => question.Name == MdnsDiscovery.ServiceTypesName
                ? new DnsResponseBuilder().PointerRecord(MdnsDiscovery.ServiceTypesName, Http).Build()
                : new DnsResponseBuilder()
                    .PointerRecord(Http, Printer)
                    .ServiceRecord(Printer, "printer.local", 8080)
                    .AddressRecord("printer.local", "127.0.0.1")
                    .PointerRecord(Http, "Other._http._tcp.local")
                    .ServiceRecord("Other._http._tcp.local", "other.local", 80)
                    .AddressRecord("other.local", "192.168.1.10")
                    .Build())
            .ToList());
        using var ssdpResponder = new FakeResponder(_ => [Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nUSN: uuid:printer\r\nSERVER: Printer/1.0\r\n\r\n")]);
        using var ipScanner = CreateScanner(mdnsResponder, ssdpResponder, discoverServices: true);

        // Act
        var results = await ipScanner.Scan(IPAddress.Loopback, CancellationToken.None);

        // Assert
        var result = Assert.Single(results.CollectedResults);
        var service = Assert.Single(result.MdnsServices);
        Assert.Equal("Printer", service.InstanceName);
        Assert.Equal(8080, service.Port);
        Assert.Equal("printer.local", result.MdnsHostName);

        var device = Assert.Single(result.SsdpDevices);
        Assert.Equal("Printer/1.0", device.Server);
        Assert.True(result.HasConnection);
        Assert.Contains("printer.local", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Scan_DiscoverServicesOff_SendsNoQueries()
    {
        // Arrange
        using var mdnsResponder = new FakeResponder(_ => []);
        using var ssdpResponder = new FakeResponder(_ => []);
        using var ipScanner = CreateScanner(mdnsResponder, ssdpResponder, discoverServices: false);

        // Act
        var results = await ipScanner.Scan(IPAddress.Loopback, CancellationToken.None);

        // Assert
        var result = Assert.Single(results.CollectedResults);
        Assert.Empty(result.MdnsServices);
        Assert.Empty(result.SsdpDevices);
        Assert.Null(result.MdnsHostName);
        Assert.Empty(mdnsResponder.Requests);
        Assert.Empty(ssdpResponder.Requests);
    }

    [Fact]
    public async Task Scan_DiscoveryFails_TheScanStillCompletes()
    {
        // Arrange - an IPv6 endpoint, which discovery's IPv4 socket can't send to on any OS.
        var logger = new CapturingLogger();
        using var ipScanner = new IPScanner(
            logger,
            new IPScannerConfig
            {
                IcmpPing = false,
                ResolveHostName = false,
                ResolveMacAddress = false,
                ResolveVendorFromMacAddress = false,
                TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
                DiscoverServices = true,
                TimeoutDiscovery = TimeSpan.FromMilliseconds(300),
            })
        {
            MdnsQueryEndPoint = new IPEndPoint(IPAddress.IPv6Loopback, 5353),
            SsdpQueryEndPoint = new IPEndPoint(IPAddress.IPv6Loopback, 1900),
        };

        // Act
        var results = await ipScanner.Scan(IPAddress.Loopback, CancellationToken.None);

        // Assert - both discoveries failed, were logged, and the scan completed without services.
        Assert.Null(results.ErrorMessage);
        var result = Assert.Single(results.CollectedResults);
        Assert.Empty(result.MdnsServices);
        Assert.Empty(result.SsdpDevices);
        Assert.Equal(2, logger.Entries.Count(x => x.EventId.Id == 10500));
    }

    [Fact]
    public async Task Scan_DiscoverOnvif_AttachesTheOnvifDeviceOfTheAddress()
    {
        // Arrange - a camera on 127.0.0.1 answers the probes.
        using var onvifResponder = new FakeResponder(request =>
        [
            OnvifFixtures.RelatedTo(
                OnvifFixtures.TapoC210ProbeMatch.Replace("192.168.1.132", "127.0.0.1", StringComparison.Ordinal),
                OnvifFixtures.ReadProbe(request).MessageId),
        ]);
        using var ipScanner = CreateOnvifScanner(onvifResponder, discoverOnvif: true);

        // Act
        var results = await ipScanner.Scan(IPAddress.Loopback, CancellationToken.None);

        // Assert
        var result = Assert.Single(results.CollectedResults);
        var device = Assert.Single(result.OnvifDevices);
        Assert.Equal("C210", device.Name);
        Assert.True(result.HasConnection);
        Assert.Contains("C210", result.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Scan_DiscoverOnvifOff_SendsNoProbe()
    {
        // Arrange
        using var onvifResponder = new FakeResponder(_ => []);
        using var ipScanner = CreateOnvifScanner(onvifResponder, discoverOnvif: false);

        // Act
        var results = await ipScanner.Scan(IPAddress.Loopback, CancellationToken.None);

        // Assert
        Assert.Empty(Assert.Single(results.CollectedResults).OnvifDevices);
        Assert.Empty(onvifResponder.Requests);
    }

    private static IPScanner CreateOnvifScanner(
        FakeResponder onvifResponder,
        bool discoverOnvif)
        => new(
            NullLogger.Instance,
            new IPScannerConfig
            {
                IcmpPing = false,
                ResolveHostName = false,
                ResolveMacAddress = false,
                ResolveVendorFromMacAddress = false,
                TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
                DiscoverOnvif = discoverOnvif,
                TimeoutDiscovery = TimeSpan.FromMilliseconds(500),
            })
        {
            OnvifQueryEndPoint = onvifResponder.EndPoint,
        };

    private static IPScanner CreateScanner(
        FakeResponder mdnsResponder,
        FakeResponder ssdpResponder,
        bool discoverServices)
        => new(
            NullLogger.Instance,
            new IPScannerConfig
            {
                IcmpPing = false,
                ResolveHostName = false,
                ResolveMacAddress = false,
                ResolveVendorFromMacAddress = false,
                TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
                DiscoverServices = discoverServices,
                TimeoutDiscovery = TimeSpan.FromMilliseconds(500),
            })
        {
            MdnsQueryEndPoint = mdnsResponder.EndPoint,
            SsdpQueryEndPoint = ssdpResponder.EndPoint,
        };
}