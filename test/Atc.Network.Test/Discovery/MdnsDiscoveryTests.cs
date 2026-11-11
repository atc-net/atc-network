namespace Atc.Network.Test.Discovery;

public sealed class MdnsDiscoveryTests
{
    private const int Ptr = 12;
    private const int Txt = 16;
    private const int Srv = 33;
    private const int A = 1;
    private const string Http = "_http._tcp.local";
    private const string Printer = "Printer._http._tcp.local";

    private static readonly TimeSpan ShortDuration = TimeSpan.FromMilliseconds(500);

    [Fact]
    public async Task Browse_ReturnsTheInstanceWithItsHostPortAddressesAndProperties()
    {
        // Arrange - the responder answers the PTR query with every record at once, as most responders do.
        using var responder = new FakeResponder(_ =>
        [
            new DnsResponseBuilder()
                .PointerRecord(Http, Printer)
                .ServiceRecord(Printer, "printer.local", 8080)
                .TextRecord(Printer, "path=/x", "flag")
                .AddressRecord("printer.local", "192.168.1.10")
                .Build(),
        ]);

        // Act
        var actual = await MdnsDiscovery.Browse(Http, ShortDuration, responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        var instance = Assert.Single(actual);
        Assert.Equal("Printer", instance.InstanceName);
        Assert.Equal(Http, instance.ServiceType);
        Assert.Equal(Printer, instance.FullName);
        Assert.Equal("printer.local", instance.HostName);
        Assert.Equal(8080, instance.Port);
        Assert.Equal([IPAddress.Parse("192.168.1.10")], instance.IPAddresses);
        Assert.Equal("/x", instance.Properties["path"]);
        Assert.True(instance.Properties.ContainsKey("flag"));
        Assert.Null(instance.Properties["flag"]);

        var question = Assert.Single(DnsResponseBuilder.ReadQuestions(responder.Requests[0]));
        Assert.Equal((Http, Ptr), question);
    }

    [Fact]
    public async Task Browse_ServiceTypeWithoutLocal_AddsIt()
    {
        // Arrange
        using var responder = new FakeResponder(_ => []);

        // Act
        await MdnsDiscovery.Browse("_http._tcp", TimeSpan.FromMilliseconds(200), responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal(Http, DnsResponseBuilder.ReadQuestions(responder.Requests[0])[0].Name);
    }

    [Fact]
    public async Task Browse_MissingRecords_AreAskedFor()
    {
        // Arrange - the responder only answers what it is asked: PTR, then SRV and TXT, then A.
        using var responder = new FakeResponder(request =>
        {
            var questions = DnsResponseBuilder.ReadQuestions(request);
            var builder = new DnsResponseBuilder();
            foreach (var (name, type) in questions)
            {
                _ = (name, type) switch
                {
                    (Http, Ptr) => builder.PointerRecord(Http, Printer),
                    (Printer, Srv) => builder.ServiceRecord(Printer, "printer.local", 8080),
                    (Printer, Txt) => builder.TextRecord(Printer, "path=/x"),
                    ("printer.local", A) => builder.AddressRecord("printer.local", "192.168.1.10"),
                    _ => builder,
                };
            }

            return [builder.Build()];
        });

        // Act
        var actual = await MdnsDiscovery.Browse(Http, ShortDuration, responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        var instance = Assert.Single(actual);
        Assert.Equal(8080, instance.Port);
        Assert.Equal([IPAddress.Parse("192.168.1.10")], instance.IPAddresses);
        Assert.Equal("/x", instance.Properties["path"]);
    }

    [Fact]
    public async Task Browse_InstanceThatNeverCompletes_IsReturnedWhenTheDurationEnds()
    {
        // Arrange - only the PTR record ever arrives.
        using var responder = new FakeResponder(request =>
            DnsResponseBuilder.ReadQuestions(request)[0].Type == Ptr
                ? [new DnsResponseBuilder().PointerRecord(Http, Printer).Build()]
                : []);

        // Act
        var actual = await MdnsDiscovery.Browse(Http, TimeSpan.FromMilliseconds(300), responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        var instance = Assert.Single(actual);
        Assert.Equal("Printer", instance.InstanceName);
        Assert.Null(instance.HostName);
        Assert.Equal(0, instance.Port);
        Assert.Empty(instance.IPAddresses);
    }

    [Fact]
    public async Task Browse_SameInstanceTwice_IsReturnedOnce()
    {
        // Arrange
        var response = new DnsResponseBuilder()
            .PointerRecord(Http, Printer)
            .ServiceRecord(Printer, "printer.local", 8080)
            .AddressRecord("printer.local", "192.168.1.10")
            .Build();
        using var responder = new FakeResponder(_ => [response, response]);

        // Act
        var actual = await MdnsDiscovery.Browse(Http, ShortDuration, responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Single(actual);
    }

    [Fact]
    public async Task Browse_MalformedPackets_AreIgnored()
    {
        // Arrange
        using var responder = new FakeResponder(_ =>
        [
            [0x00, 0x01, 0x02],
            new DnsResponseBuilder()
                .PointerRecord(Http, Printer)
                .ServiceRecord(Printer, "printer.local", 8080)
                .AddressRecord("printer.local", "192.168.1.10")
                .Build(),
        ]);

        // Act
        var actual = await MdnsDiscovery.Browse(Http, ShortDuration, responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Single(actual);
    }

    [Fact]
    public async Task BrowseAll_BrowsesEveryServiceTypeFound()
    {
        // Arrange - the responder has an HTTP and an IPP service.
        // BrowseAll asks for both service types in one query, so answer every question.
        using var responder = new FakeResponder(request => DnsResponseBuilder
            .ReadQuestions(request)
            .SelectMany(question => question.Name switch
            {
                MdnsDiscovery.ServiceTypesName =>
                [
                    new DnsResponseBuilder()
                        .PointerRecord(MdnsDiscovery.ServiceTypesName, Http)
                        .PointerRecord(MdnsDiscovery.ServiceTypesName, "_ipp._tcp.local")
                        .Build(),
                ],
                Http => [Complete(Http, Printer, 80)],
                "_ipp._tcp.local" => [Complete("_ipp._tcp.local", "Printer._ipp._tcp.local", 631)],
                _ => Array.Empty<byte[]>(),
            })
            .ToList());

        // Act
        var actual = await MdnsDiscovery.BrowseAll(ShortDuration, responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Contains(actual, x => x.ServiceType == Http && x.Port == 80);
        Assert.Contains(actual, x => x.ServiceType == "_ipp._tcp.local" && x.Port == 631);
    }

    [Fact]
    public async Task GetServiceTypes_ReturnsEachTypeOnce()
    {
        // Arrange
        var response = new DnsResponseBuilder()
            .PointerRecord(MdnsDiscovery.ServiceTypesName, Http)
            .PointerRecord(MdnsDiscovery.ServiceTypesName, "_ipp._tcp.local")
            .Build();
        using var responder = new FakeResponder(_ => [response, response]);

        // Act
        var actual = await MdnsDiscovery.GetServiceTypes(ShortDuration, responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal([Http, "_ipp._tcp.local"], actual.Order(StringComparer.Ordinal), StringComparer.Ordinal);
        Assert.Equal((MdnsDiscovery.ServiceTypesName, Ptr), DnsResponseBuilder.ReadQuestions(responder.Requests[0])[0]);
    }

    [Fact]
    public async Task Browse_Cancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var responder = new FakeResponder(_ => []);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await MdnsDiscovery.Browse(Http, TimeSpan.FromSeconds(30), responder.EndPoint, cancellationTokenSource.Token).ToListAsync());
    }

    [Fact]
    public async Task Browse_RecordsMessagesAndAnActivity()
    {
        // Arrange
        using var telemetry = new TelemetryCollector();
        using var responder = new FakeResponder(_ => [Complete(Http, Printer, 80)]);

        // Act
        await MdnsDiscovery.Browse(Http, TimeSpan.FromMilliseconds(300), responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        var port = responder.EndPoint.Port;
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesSentInstrumentName, "mdns_discovery", port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesReceivedInstrumentName, "mdns_discovery", port));
        Assert.Single(telemetry.Activities("MdnsDiscovery.Browse", port));
    }

    [Fact]
    public async Task MdnsServiceInstance_ToString_ListsTheValues()
    {
        // Arrange
        using var responder = new FakeResponder(_ => [Complete(Http, Printer, 80)]);
        var instance = (await MdnsDiscovery.Browse(Http, ShortDuration, responder.EndPoint, CancellationToken.None).ToListAsync())[0];

        // Act
        var actual = instance.ToString();

        // Assert
        Assert.Contains("InstanceName: Printer", actual, StringComparison.Ordinal);
        Assert.Contains("ServiceType: _http._tcp.local", actual, StringComparison.Ordinal);
        Assert.Contains("HostName: printer.local", actual, StringComparison.Ordinal);
        Assert.Contains("Port: 80", actual, StringComparison.Ordinal);
        Assert.Contains("192.168.1.10", actual, StringComparison.Ordinal);
    }

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task BrowseAll_OnTheLocalNetwork_DoesNotThrow()
    {
        // Act - what answers depends on the network, so only check that browsing works.
        var exception = await Record.ExceptionAsync(async () => await MdnsDiscovery.BrowseAll(TimeSpan.FromSeconds(2)).ToListAsync());

        // Assert
        Assert.Null(exception);
    }

    private static byte[] Complete(
        string serviceType,
        string instance,
        ushort port)
        => new DnsResponseBuilder()
            .PointerRecord(serviceType, instance)
            .ServiceRecord(instance, "printer.local", port)
            .AddressRecord("printer.local", "192.168.1.10")
            .Build();
}