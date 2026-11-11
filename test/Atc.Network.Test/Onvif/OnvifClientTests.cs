namespace Atc.Network.Test.Onvif;

[SuppressMessage("Security", "SCS0015:Hardcoded value", Justification = "OK - a made-up password for the tests.")]
public sealed class OnvifClientTests
{
    private static readonly Uri DeviceServiceUrl = new("http://192.168.1.132:2020/onvif/device_service");
    private static readonly NetworkCredential Credential = new("admin", "secret");

    [Fact]
    public void ComputePasswordDigest_IsBase64OfSha1OfNonceCreatedAndPassword()
    {
        // Act - the expected value is computed outside .NET: base64(sha1(bytes 0..15 + created + "secret")).
        var actual = OnvifClient.ComputePasswordDigest(
            Enumerable.Range(0, 16).Select(x => (byte)x).ToArray(),
            "2026-11-11T23:04:46Z",
            "secret");

        // Assert
        Assert.Equal("iZ05Yx0A7EJOICyD6vj37uWvwHQ=", actual);
    }

    [Fact]
    public void CreateEnvelope_WithCredential_HasAUsernameTokenWithAPasswordDigest()
    {
        // Arrange
        var nonce = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();
        var created = new DateTime(2026, 11, 11, 23, 4, 46, DateTimeKind.Utc);

        // Act
        var envelope = OnvifClient.CreateEnvelope(new XElement(XName.Get("GetDeviceInformation", "http://www.onvif.org/ver10/device/wsdl")), Credential, created, nonce);

        // Assert
        var document = XDocument.Parse(envelope);
        var password = Descendant(document, "Password");
        Assert.Equal("admin", Descendant(document, "Username").Value);
        Assert.EndsWith("#PasswordDigest", password.Attribute("Type")?.Value, StringComparison.Ordinal);
        Assert.Equal("iZ05Yx0A7EJOICyD6vj37uWvwHQ=", password.Value);
        Assert.Equal("AAECAwQFBgcICQoLDA0ODw==", Descendant(document, "Nonce").Value);
        Assert.Equal("2026-11-11T23:04:46Z", Descendant(document, "Created").Value);
        Assert.DoesNotContain("secret", envelope, StringComparison.Ordinal);
        Assert.Equal("GetDeviceInformation", Descendant(document, "Body").Elements().Single().Name.LocalName);
    }

    [Fact]
    public void CreateEnvelope_WithoutCredential_HasNoSecurityHeader()
    {
        // Act
        var envelope = OnvifClient.CreateEnvelope(new XElement(XName.Get("GetSystemDateAndTime", "http://www.onvif.org/ver10/device/wsdl")), credential: null, DateTime.UtcNow, new byte[16]);

        // Assert
        Assert.DoesNotContain("Security", envelope, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetSystemDateAndTime_ReadsTheDeviceClockAndTheOffset()
    {
        // Arrange - the captured Tapo answer.
        using var device = new FakeOnvifDevice((_, _) => (HttpStatusCode.OK, OnvifFixtures.TapoGetSystemDateAndTimeResponse));
        var client = device.Client;

        // Act
        var actual = await client.GetSystemDateAndTime(CancellationToken.None);

        // Assert
        Assert.Equal(new DateTime(2026, 11, 11, 23, 4, 46, DateTimeKind.Utc), actual);
        Assert.Equal(DateTimeKind.Utc, actual.Kind);
        Assert.InRange(client.ClockOffset - (actual - DateTime.UtcNow), TimeSpan.FromSeconds(-5), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AuthenticatedRequest_UsesTheDeviceClock()
    {
        // Arrange - the device's clock is an hour ahead; a token with the local time would be rejected.
        var deviceNow = DateTime.UtcNow.AddHours(1);
        using var device = new FakeOnvifDevice(
            (operation, _) => operation == "GetSystemDateAndTime"
                ? (HttpStatusCode.OK, OnvifClientFixtures.GetSystemDateAndTimeResponse(deviceNow))
                : (HttpStatusCode.OK, OnvifClientFixtures.GetDeviceInformationResponse));
        var client = device.Client;
        var handler = device.Handler;

        // Act
        await client.GetDeviceInformation(CancellationToken.None);

        // Assert - the clock was read first, without credentials, then the token's Created follows the device's clock.
        Assert.Equal(["GetSystemDateAndTime", "GetDeviceInformation"], handler.Requests.Select(x => x.Operation), StringComparer.Ordinal);
        Assert.DoesNotContain("UsernameToken", handler.Requests[0].Body, StringComparison.Ordinal);
        var created = DateTime.Parse(Descendant(XDocument.Parse(handler.Requests[1].Body), "Created").Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        Assert.InRange(created - deviceNow, TimeSpan.FromSeconds(-5), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetDeviceInformation_ReadsTheDevice()
    {
        // Arrange
        using var device = new FakeOnvifDevice(Answer);
        var client = device.Client;

        // Act
        var actual = await client.GetDeviceInformation(CancellationToken.None);

        // Assert
        Assert.Equal("TP-Link", actual.Manufacturer);
        Assert.Equal("C210", actual.Model);
        Assert.Equal("1.3.11 Build 231115", actual.FirmwareVersion);
        Assert.Equal("00000000", actual.SerialNumber);
        Assert.Equal("2.0", actual.HardwareId);
    }

    [Fact]
    public async Task NotAuthorizedFault_ThrowsOnvifExceptionWithTheSubcode()
    {
        // Arrange - the captured Tapo fault, HTTP 400.
        using var device = new FakeOnvifDevice(
            (operation, _) => operation == "GetSystemDateAndTime"
                ? (HttpStatusCode.OK, OnvifFixtures.TapoGetSystemDateAndTimeResponse)
                : (HttpStatusCode.BadRequest, OnvifFixtures.TapoNotAuthorizedFault));
        var client = device.Client;

        // Act
        var exception = await Assert.ThrowsAsync<OnvifException>(() => client.GetDeviceInformation(CancellationToken.None));

        // Assert
        Assert.Equal("ter:NotAuthorized", exception.Subcode);
        Assert.True(exception.IsNotAuthorized);
        Assert.Equal("Authority failure", exception.Reason);
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task Http401WithoutSoap_ThrowsOnvifExceptionAboutHttpDigest()
    {
        // Arrange - some devices want HTTP digest authentication instead of a WS-Security token.
        using var device = new FakeOnvifDevice((operation, _) => operation == "GetSystemDateAndTime" ? (HttpStatusCode.OK, OnvifFixtures.TapoGetSystemDateAndTimeResponse) : (HttpStatusCode.Unauthorized, string.Empty));
        var client = device.Client;

        // Act
        var exception = await Assert.ThrowsAsync<OnvifException>(() => client.GetDeviceInformation(CancellationToken.None));

        // Assert
        Assert.True(exception.IsNotAuthorized);
        Assert.Contains("HTTP digest", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MalformedResponse_ThrowsOnvifException()
    {
        // Arrange
        using var device = new FakeOnvifDevice((_, _) => (HttpStatusCode.OK, "<not xml"));

        // Act
        var exception = await Record.ExceptionAsync(async () => await device.Client.GetSystemDateAndTime(CancellationToken.None));

        // Assert
        Assert.IsType<OnvifException>(exception);
    }

    [Fact]
    public async Task GetCapabilities_ReadsTheServiceUrls()
    {
        // Arrange
        using var device = new FakeOnvifDevice(Answer);
        var client = device.Client;

        // Act
        var actual = await client.GetCapabilities(CancellationToken.None);

        // Assert
        Assert.Equal(new Uri("http://192.168.1.132:2020/onvif/media_service"), actual.MediaServiceUrl);
        Assert.Equal(new Uri("http://192.168.1.132:2020/onvif/ptz_service"), actual.PtzServiceUrl);
        Assert.Equal(new Uri("http://192.168.1.132:2020/onvif/service"), actual.EventsServiceUrl);
    }

    [Fact]
    public async Task GetProfiles_IsSentToTheMediaServiceAndReadsTheProfiles()
    {
        // Arrange
        using var device = new FakeOnvifDevice(Answer);
        var client = device.Client;
        var handler = device.Handler;

        // Act
        var actual = await client.GetProfiles(CancellationToken.None);

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Equal(new OnvifMediaProfile("profile_1", "mainStream", "H264", 2304, 1296, 15), actual[0]);
        Assert.Equal("minorStream", actual[1].Name);
        var getProfiles = handler.Requests.Single(x => x.Operation == "GetProfiles");
        Assert.Equal(new Uri("http://192.168.1.132:2020/onvif/media_service"), getProfiles.Uri);
    }

    [Fact]
    public async Task GetStreamUri_AsksForRtspOverUnicast()
    {
        // Arrange
        using var device = new FakeOnvifDevice(Answer);
        var client = device.Client;
        var handler = device.Handler;

        // Act
        var actual = await client.GetStreamUri("profile_1", CancellationToken.None);

        // Assert
        Assert.Equal(new Uri("rtsp://192.168.1.132:554/stream1"), actual);
        var request = XDocument.Parse(handler.Requests.Single(x => x.Operation == "GetStreamUri").Body);
        Assert.Equal("profile_1", Descendant(request, "ProfileToken").Value);
        Assert.Equal("RTP-Unicast", Descendant(request, "Stream").Value);
        Assert.Equal("RTSP", Descendant(request, "Protocol").Value);
    }

    [Fact]
    public async Task GetStreams_ReturnsEachProfileWithItsStreamUri()
    {
        // Arrange
        using var device = new FakeOnvifDevice(Answer);
        var client = device.Client;

        // Act
        var actual = await client.GetStreams(CancellationToken.None);

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Equal("mainStream", actual[0].Profile.Name);
        Assert.Equal(new Uri("rtsp://192.168.1.132:554/stream1"), actual[0].StreamUri);
        Assert.Equal(new Uri("rtsp://192.168.1.132:554/stream2"), actual[1].StreamUri);
    }

    [Fact]
    public async Task GetStreams_StaleHostInTheStreamUri_IsReplacedByTheDeviceHost()
    {
        // Arrange - the camera reports an old address in its stream URIs.
        using var device = new FakeOnvifDevice(
            (operation, body) => operation == "GetStreamUri"
                ? (HttpStatusCode.OK, OnvifClientFixtures.GetStreamUriResponse("rtsp://192.168.0.99:554/stream1"))
                : Answer(operation, body));
        var client = device.Client;

        // Act
        var actual = await client.GetStreams(CancellationToken.None);

        // Assert
        Assert.All(actual, x => Assert.Equal("192.168.1.132", x.StreamUri.Host));
        Assert.All(actual, x => Assert.Equal(554, x.StreamUri.Port));
    }

    [Fact]
    public async Task GetStreams_WithoutHostReplacement_KeepsTheReportedHost()
    {
        // Arrange
        using var device = new FakeOnvifDevice(
            (operation, body) => operation == "GetStreamUri"
                ? (HttpStatusCode.OK, OnvifClientFixtures.GetStreamUriResponse("rtsp://192.168.0.99:554/stream1"))
                : Answer(operation, body));

        // Act
        var actual = await device.Client.GetStreams(replaceHostWithDeviceHost: false, CancellationToken.None);

        // Assert
        Assert.All(actual, x => Assert.Equal("192.168.0.99", x.StreamUri.Host));
    }

    [Fact]
    public async Task GetNetworkInterfaces_ReadsWiredAndWirelessInterfaces()
    {
        // Arrange
        using var device = new FakeOnvifDevice(Answer);
        var client = device.Client;

        // Act
        var actual = await client.GetNetworkInterfaces(CancellationToken.None);

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Equal("eth0", actual[0].Name);
        Assert.False(actual[0].IsWireless);
        Assert.Equal("00:11:22:33:44:55", actual[0].MacAddress);
        Assert.Equal([IPAddress.Parse("192.168.1.140")], actual[0].IPv4Addresses);
        Assert.True(actual[1].IsWireless);
        Assert.Equal([IPAddress.Parse("192.168.1.132")], actual[1].IPv4Addresses);
    }

    [Fact]
    public async Task Request_SendsTheActionInTheContentType()
    {
        // Arrange
        using var device = new FakeOnvifDevice(Answer);
        var client = device.Client;
        var handler = device.Handler;

        // Act
        await client.GetSystemDateAndTime(CancellationToken.None);

        // Assert
        Assert.Equal("GetSystemDateAndTime", Assert.Single(handler.Requests).Operation);
    }

    [Fact]
    public async Task SlowDevice_ThrowsTimeoutException()
    {
        // Arrange
        using var handler = new FakeOnvifHandler(Answer) { Delay = TimeSpan.FromSeconds(10) };
        using var httpClient = new HttpClient(handler);
        var client = new OnvifClient(DeviceServiceUrl, Credential, httpClient) { Timeout = TimeSpan.FromMilliseconds(200) };

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => client.GetSystemDateAndTime(CancellationToken.None));
    }

    [Fact]
    public async Task Cancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var handler = new FakeOnvifHandler(Answer) { Delay = TimeSpan.FromSeconds(10) };
        using var httpClient = new HttpClient(handler);
        var client = new OnvifClient(DeviceServiceUrl, Credential, httpClient);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetSystemDateAndTime(cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Request_RecordsAnActivityPerOperation()
    {
        // Arrange
        using var telemetry = new TelemetryCollector();
        using var device = new FakeOnvifDevice(Answer);
        var client = device.Client;

        // Act
        await client.GetSystemDateAndTime(CancellationToken.None);

        // Assert
        Assert.Single(telemetry.Activities("OnvifClient.GetSystemDateAndTime", 2020));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesSentInstrumentName, "onvif_client", 2020));
    }

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task RealCamera_ReturnsItsModelAndStreams()
    {
        // Arrange - set ATC_ONVIF_URL (a device service URL), ATC_ONVIF_USERNAME and ATC_ONVIF_PASSWORD to run this.
        var url = Environment.GetEnvironmentVariable("ATC_ONVIF_URL");
        var userName = Environment.GetEnvironmentVariable("ATC_ONVIF_USERNAME");
        var password = Environment.GetEnvironmentVariable("ATC_ONVIF_PASSWORD");
        Assert.SkipWhen(string.IsNullOrEmpty(url) || string.IsNullOrEmpty(userName), "ATC_ONVIF_URL and ATC_ONVIF_USERNAME are not set.");
        var client = new OnvifClient(new Uri(url ?? string.Empty), new NetworkCredential(userName, password));

        // Act
        var information = await client.GetDeviceInformation(CancellationToken.None);
        var streams = await client.GetStreams(CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrEmpty(information.Model));
        Assert.NotEmpty(streams);
        Assert.All(streams, x => Assert.Equal("rtsp", x.StreamUri.Scheme));
    }

    private static (HttpStatusCode Status, string Body) Answer(
        string operation,
        string body)
        => operation switch
        {
            "GetSystemDateAndTime" => (HttpStatusCode.OK, OnvifClientFixtures.GetSystemDateAndTimeResponse(DateTime.UtcNow)),
            "GetDeviceInformation" => (HttpStatusCode.OK, OnvifClientFixtures.GetDeviceInformationResponse),
            "GetCapabilities" => (HttpStatusCode.OK, OnvifClientFixtures.GetCapabilitiesResponse),
            "GetProfiles" => (HttpStatusCode.OK, OnvifClientFixtures.GetProfilesResponse),
            "GetStreamUri" => (HttpStatusCode.OK, OnvifClientFixtures.GetStreamUriResponse(
                body.Contains("profile_2", StringComparison.Ordinal) ? "rtsp://192.168.1.132:554/stream2" : "rtsp://192.168.1.132:554/stream1")),
            "GetNetworkInterfaces" => (HttpStatusCode.OK, OnvifClientFixtures.GetNetworkInterfacesResponse),
            _ => (HttpStatusCode.BadRequest, OnvifFixtures.TapoNotAuthorizedFault),
        };

    private static XElement Descendant(
        XDocument document,
        string localName)
        => document.Descendants().First(x => x.Name.LocalName == localName);

    /// <summary>
    /// A fake ONVIF device: the handler answering requests, and an <see cref="OnvifClient"/> talking to it.
    /// </summary>
    private sealed class FakeOnvifDevice : IDisposable
    {
        private readonly HttpClient httpClient;

        public FakeOnvifDevice(
            Func<string, string, (HttpStatusCode Status, string Body)> respond)
        {
            Handler = new FakeOnvifHandler(respond);
            httpClient = new HttpClient(Handler);
            Client = new OnvifClient(DeviceServiceUrl, Credential, httpClient);
        }

        public FakeOnvifHandler Handler { get; }

        public OnvifClient Client { get; }

        public void Dispose()
        {
            httpClient.Dispose();
            Handler.Dispose();
        }
    }
}