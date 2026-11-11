namespace Atc.Network.Test.Discovery;

public sealed class OnvifDiscoveryTests
{
    private const string MessageId = "uuid:11111111-2222-4333-8444-555555555555";
    private static readonly IPAddress TapoAddress = IPAddress.Parse("192.168.1.132");
    private static readonly TimeSpan ShortDuration = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void CreateProbe_IsAWsDiscoveryProbeForTheType()
    {
        // Act
        var probe = OnvifDiscovery.CreateProbe(MessageId, OnvifDiscovery.NetworkVideoTransmitterType);

        // Assert
        var document = System.Xml.Linq.XDocument.Parse(Encoding.UTF8.GetString(probe));
        string Value(string localName)
            => document.Descendants().First(x => x.Name.LocalName == localName).Value;

        Assert.Equal("http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe", Value("Action"));
        Assert.Equal(MessageId, Value("MessageID"));
        Assert.Equal("urn:schemas-xmlsoap-org:ws:2005:04:discovery", Value("To"));

        var types = document.Descendants().First(x => x.Name.LocalName == "Types");
        Assert.Equal("dp0:NetworkVideoTransmitter", types.Value);
        Assert.Equal("http://www.onvif.org/ver10/network/wsdl", types.GetNamespaceOfPrefix("dp0")?.NamespaceName);
    }

    [Fact]
    public void ParseProbeMatches_TapoCamera_ReadsTheDevice()
    {
        // Act
        var actual = OnvifDiscovery.ParseProbeMatches(OnvifFixtures.RelatedTo(OnvifFixtures.TapoC210ProbeMatch, MessageId), TapoAddress, MessageId);

        // Assert
        var device = Assert.Single(actual);
        Assert.Equal(TapoAddress, device.IPAddress);
        Assert.Equal("uuid:00000000-0000-4000-8000-000000000132", device.EndpointReference);
        Assert.Equal("C210", device.Name);
        Assert.Equal("C210", device.Hardware);
        Assert.Equal("Hong Kong", device.Location);
        Assert.True(device.IsNetworkVideoTransmitter);
        Assert.Equal(["tdn:NetworkVideoTransmitter"], device.Types);
        Assert.Contains("onvif://www.onvif.org/Profile/Streaming", device.Scopes, StringComparer.Ordinal);
        Assert.Equal(new Uri("http://192.168.1.132:2020/onvif/device_service"), device.DeviceServiceUrl);
        Assert.Equal([new Uri("http://192.168.1.132:2020/onvif/device_service")], device.XAddrs);
    }

    [Fact]
    public void ParseProbeMatches_EmptyProbeMatches_HasNoDevices()
        => Assert.Empty(OnvifDiscovery.ParseProbeMatches(OnvifFixtures.RelatedTo(OnvifFixtures.NasEmptyProbeMatches, MessageId), IPAddress.Parse("192.168.1.212"), MessageId));

    [Fact]
    public void ParseProbeMatches_AnswerToAnotherProbe_IsIgnored()
        => Assert.Empty(OnvifDiscovery.ParseProbeMatches(OnvifFixtures.RelatedTo(OnvifFixtures.TapoC210ProbeMatch, "uuid:another"), TapoAddress, MessageId));

    [Fact]
    public void ParseProbeMatches_WithoutRelatesTo_IsAccepted()
    {
        // Arrange - some devices leave out RelatesTo.
        var answer = OnvifFixtures.TapoC210ProbeMatch.Replace("<wsa:RelatesTo>{RelatesTo}</wsa:RelatesTo>", string.Empty, StringComparison.Ordinal);

        // Act & Assert
        Assert.Single(OnvifDiscovery.ParseProbeMatches(OnvifFixtures.RelatedTo(answer, MessageId), TapoAddress, MessageId));
    }

    [Fact]
    public void ParseProbeMatches_SeveralXAddrs_PrefersTheAddressThatAnswered()
    {
        // Arrange - an IPv6 and another IPv4 address before the one the answer came from.
        var answer = OnvifFixtures.TapoC210ProbeMatch.Replace(
            "<wsdd:XAddrs>http://192.168.1.132:2020/onvif/device_service</wsdd:XAddrs>",
            "<wsdd:XAddrs>http://[fe80::1]/onvif/device_service http://10.0.0.5/onvif/device_service http://192.168.1.132:2020/onvif/device_service</wsdd:XAddrs>",
            StringComparison.Ordinal);

        // Act
        var device = Assert.Single(OnvifDiscovery.ParseProbeMatches(OnvifFixtures.RelatedTo(answer, MessageId), TapoAddress, MessageId));

        // Assert
        Assert.Equal(3, device.XAddrs.Count);
        Assert.Equal(new Uri("http://192.168.1.132:2020/onvif/device_service"), device.DeviceServiceUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not xml")]
    [InlineData("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://www.w3.org/2003/05/soap-envelope\"><SOAP-ENV:Body>")]
    [InlineData("<?xml version=\"1.0\"?><!DOCTYPE lolz [<!ENTITY lol \"lol\"><!ENTITY lol2 \"&lol;&lol;&lol;&lol;\">]><lolz>&lol2;</lolz>")]
    public void ParseProbeMatches_MalformedOrUnsafeXml_IsIgnored(string answer)
        => Assert.Empty(OnvifDiscovery.ParseProbeMatches(Encoding.UTF8.GetBytes(answer), TapoAddress, MessageId));

    [Fact]
    public void ParseProbeMatches_TruncatedAnywhere_IsIgnoredWithoutThrowing()
    {
        // Arrange
        var answer = OnvifFixtures.RelatedTo(OnvifFixtures.TapoC210ProbeMatch, MessageId);

        // Act & Assert
        for (var length = 0; length < answer.Length - 1; length += 7)
        {
            Assert.Empty(OnvifDiscovery.ParseProbeMatches(answer.AsSpan(0, length).ToArray(), TapoAddress, MessageId));
        }
    }

    [Fact]
    public async Task Probe_ProbesForCamerasAndDevices_AndReturnsEachDeviceOnce()
    {
        // Arrange - the camera answers both probes, and the NAS answers with an empty ProbeMatches.
        using var responder = new FakeResponder(request =>
        {
            var (messageId, _) = OnvifFixtures.ReadProbe(request);
            return
            [
                OnvifFixtures.RelatedTo(OnvifFixtures.TapoC210ProbeMatch, messageId),
                OnvifFixtures.RelatedTo(OnvifFixtures.NasEmptyProbeMatches, messageId),
            ];
        });

        // Act
        var actual = await OnvifDiscovery.Probe(ShortDuration, responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        var device = Assert.Single(actual);
        Assert.Equal("C210", device.Name);

        var probedTypes = ReadProbedTypes(responder.Requests);
        Assert.Contains("dp0:NetworkVideoTransmitter", probedTypes, StringComparer.Ordinal);
        Assert.Contains("dp0:Device", probedTypes, StringComparer.Ordinal);
    }

    [Fact]
    public async Task Probe_EndsWhenTheDurationIsOver()
    {
        // Arrange
        using var responder = new FakeResponder(_ => []);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var actual = await OnvifDiscovery.Probe(TimeSpan.FromMilliseconds(300), responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Empty(actual);
        Assert.InRange(stopwatch.Elapsed, TimeSpan.FromMilliseconds(250), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Probe_Cancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var responder = new FakeResponder(_ => []);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await OnvifDiscovery.Probe(TimeSpan.FromSeconds(30), responder.EndPoint, cancellationTokenSource.Token).ToListAsync());
    }

    [Fact]
    public async Task Probe_RecordsMessagesAndAnActivity()
    {
        // Arrange
        using var telemetry = new TelemetryCollector();
        using var responder = new FakeResponder(_ => []);

        // Act
        await OnvifDiscovery.Probe(TimeSpan.FromMilliseconds(300), responder.EndPoint, CancellationToken.None).ToListAsync();

        // Assert - one probe per type.
        var port = responder.EndPoint.Port;
        Assert.Equal(2, telemetry.Sum(NetworkTelemetry.MessagesSentInstrumentName, "onvif_discovery", port));
        Assert.Single(telemetry.Activities("OnvifDiscovery.Probe", port));
    }

    [Fact]
    public void OnvifDevice_ToString_ListsTheValues()
    {
        // Arrange
        var device = OnvifDiscovery.ParseProbeMatches(OnvifFixtures.RelatedTo(OnvifFixtures.TapoC210ProbeMatch, MessageId), TapoAddress, MessageId)[0];

        // Act
        var actual = device.ToString();

        // Assert
        Assert.Contains("IPAddress: 192.168.1.132", actual, StringComparison.Ordinal);
        Assert.Contains("Name: C210", actual, StringComparison.Ordinal);
        Assert.Contains("DeviceServiceUrl: http://192.168.1.132:2020/onvif/device_service", actual, StringComparison.Ordinal);
    }

    private static List<string> ReadProbedTypes(IReadOnlyList<byte[]> requests)
        => requests.Select(x => OnvifFixtures.ReadProbe(x).Types).ToList();

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task Probe_OnTheLocalNetwork_DoesNotThrow()
    {
        // Act - what answers depends on the network, so only check that probing works.
        var exception = await Record.ExceptionAsync(async () => await OnvifDiscovery.Probe(TimeSpan.FromSeconds(2)).ToListAsync());

        // Assert
        Assert.Null(exception);
    }
}