namespace Atc.Network.Test.Discovery;

public sealed class SsdpDiscoveryTests
{
    private const string RouterResponse =
        "HTTP/1.1 200 OK\r\n" +
        "CACHE-CONTROL: max-age=1800\r\n" +
        "LOCATION: http://192.168.1.1:5000/rootDesc.xml\r\n" +
        "SERVER: Linux/5.4 UPnP/1.1 MiniUPnPd/2.2\r\n" +
        "ST: urn:schemas-upnp-org:device:InternetGatewayDevice:1\r\n" +
        "USN: uuid:1234::urn:schemas-upnp-org:device:InternetGatewayDevice:1\r\n" +
        "EXT:\r\n" +
        "\r\n";

    [Fact]
    public void CreateSearchRequest_IsAnMSearch()
    {
        // Act
        var actual = Encoding.ASCII.GetString(SsdpDiscovery.CreateSearchRequest("ssdp:all", maxWaitInSeconds: 2));

        // Assert
        Assert.Equal(
            "M-SEARCH * HTTP/1.1\r\n" +
            "HOST: 239.255.255.250:1900\r\n" +
            "MAN: \"ssdp:discover\"\r\n" +
            "MX: 2\r\n" +
            "ST: ssdp:all\r\n" +
            "\r\n",
            actual);
    }

    [Fact]
    public void ParseResponse_ReadsTheHeaders()
    {
        // Act
        var actual = SsdpDiscovery.ParseResponse(RouterResponse, IPAddress.Parse("192.168.1.1"));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(IPAddress.Parse("192.168.1.1"), actual.IPAddress);
        Assert.Equal(new Uri("http://192.168.1.1:5000/rootDesc.xml"), actual.Location);
        Assert.Equal("Linux/5.4 UPnP/1.1 MiniUPnPd/2.2", actual.Server);
        Assert.Equal("urn:schemas-upnp-org:device:InternetGatewayDevice:1", actual.SearchTarget);
        Assert.Equal("uuid:1234::urn:schemas-upnp-org:device:InternetGatewayDevice:1", actual.UniqueServiceName);
        Assert.Equal("max-age=1800", actual.Headers["cache-control"]);
        Assert.Equal(string.Empty, actual.Headers["EXT"]);
    }

    [Fact]
    public void ParseResponse_LowerCaseHeadersAndLineFeeds_AreAccepted()
    {
        // Arrange
        const string response = "HTTP/1.1 200 OK\nusn: uuid:abc\nlocation: http://10.0.0.2/desc.xml\nserver: Test/1.0\n\n";

        // Act
        var actual = SsdpDiscovery.ParseResponse(response, IPAddress.Parse("10.0.0.2"));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("uuid:abc", actual.UniqueServiceName);
        Assert.Equal("Test/1.0", actual.Server);
        Assert.Equal(new Uri("http://10.0.0.2/desc.xml"), actual.Location);
    }

    [Theory]
    [InlineData("NOTIFY * HTTP/1.1\r\nUSN: uuid:abc\r\n\r\n")]
    [InlineData("HTTP/1.1 404 Not Found\r\nUSN: uuid:abc\r\n\r\n")]
    [InlineData("HTTP/1.1 200 OK\r\nSERVER: no usn\r\n\r\n")]
    [InlineData("")]
    public void ParseResponse_NotASearchResponse_ReturnsNull(string response)
        => Assert.Null(SsdpDiscovery.ParseResponse(response, IPAddress.Loopback));

    [Fact]
    public void ParseResponse_InvalidLocation_IsNull()
    {
        // Act
        var actual = SsdpDiscovery.ParseResponse("HTTP/1.1 200 OK\r\nUSN: uuid:abc\r\nLOCATION: not a url\r\n\r\n", IPAddress.Loopback);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Location);
    }

    [Fact]
    public async Task Search_ReturnsEachDeviceOnce()
    {
        // Arrange - the responder answers with the router twice and a second device.
        using var responder = new FakeResponder(request =>
        [
            Encoding.ASCII.GetBytes(RouterResponse),
            Encoding.ASCII.GetBytes(RouterResponse),
            Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nUSN: uuid:tv\r\nSERVER: TV/1.0\r\n\r\n"),
        ]);

        // Act
        var actual = await SsdpDiscovery
            .Search("ssdp:all", TimeSpan.FromMilliseconds(500), responder.EndPoint, CancellationToken.None)
            .ToListAsync();

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Contains(actual, x => x.UniqueServiceName == "uuid:tv" && x.IPAddress.Equals(IPAddress.Loopback));
        Assert.Contains("ST: ssdp:all", responder.ReceivedRequest, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Search_EndsWhenTheDurationIsOver()
    {
        // Arrange - a responder that never answers.
        using var responder = new FakeResponder(_ => []);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var actual = await SsdpDiscovery
            .Search("ssdp:all", TimeSpan.FromMilliseconds(300), responder.EndPoint, CancellationToken.None)
            .ToListAsync();

        // Assert
        Assert.Empty(actual);
        Assert.InRange(stopwatch.Elapsed, TimeSpan.FromMilliseconds(250), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Search_Cancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var responder = new FakeResponder(_ => []);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await SsdpDiscovery
                .Search("ssdp:all", TimeSpan.FromSeconds(30), responder.EndPoint, cancellationTokenSource.Token)
                .ToListAsync());
    }

    [Fact]
    public async Task Search_RecordsBytesAndAnActivity()
    {
        // Arrange
        using var telemetry = new TelemetryCollector();
        using var responder = new FakeResponder(_ => [Encoding.ASCII.GetBytes(RouterResponse)]);

        // Act
        await SsdpDiscovery
            .Search("ssdp:all", TimeSpan.FromMilliseconds(300), responder.EndPoint, CancellationToken.None)
            .ToListAsync();

        // Assert
        var port = responder.EndPoint.Port;
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesSentInstrumentName, "ssdp_discovery", port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesReceivedInstrumentName, "ssdp_discovery", port));
        Assert.Single(telemetry.Activities("SsdpDiscovery.Search", port));
    }

    [Fact]
    public void SsdpDevice_ToString_ListsTheValues()
    {
        // Arrange
        var device = SsdpDiscovery.ParseResponse(RouterResponse, IPAddress.Parse("192.168.1.1"));

        // Act
        var actual = device?.ToString();

        // Assert
        Assert.Contains("IPAddress: 192.168.1.1", actual, StringComparison.Ordinal);
        Assert.Contains("Server: Linux/5.4", actual, StringComparison.Ordinal);
        Assert.Contains("UniqueServiceName: uuid:1234", actual, StringComparison.Ordinal);
    }

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task Search_OnTheLocalNetwork_DoesNotThrow()
    {
        // Act - what answers depends on the network, so only check that searching works.
        var exception = await Record.ExceptionAsync(async () => await SsdpDiscovery.Search(duration: TimeSpan.FromSeconds(2)).ToListAsync());

        // Assert
        Assert.Null(exception);
    }
}