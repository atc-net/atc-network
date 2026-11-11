namespace Atc.Network.Onvif;

/// <summary>
/// A small ONVIF client: device information, service URLs, media profiles, stream URIs and network interfaces.
/// </summary>
/// <remarks>
/// <para>
/// Requests are SOAP 1.2 over HTTP, authenticated with a WS-Security UsernameToken (PasswordDigest). Before the first
/// authenticated request the client reads the device's clock (which needs no credentials), so the token's time matches
/// the device: a camera with a wrong clock rejects a token with the local time.
/// </para>
/// <para>
/// Limits: Media service version 1 only (which nearly all cameras offer), and no HTTP digest authentication; a device that
/// only accepts HTTP digest answers 401, which is reported as an <see cref="OnvifException"/>.
/// The password is never logged or included in exceptions.
/// </para>
/// </remarks>
public sealed class OnvifClient
{
    private const string SoapNamespace = "http://www.w3.org/2003/05/soap-envelope";
    private const string DeviceNamespace = "http://www.onvif.org/ver10/device/wsdl";
    private const string MediaNamespace = "http://www.onvif.org/ver10/media/wsdl";
    private const string SchemaNamespace = "http://www.onvif.org/ver10/schema";
    private const string WsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
    private const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
    private const string PasswordDigestType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest";
    private const string Base64EncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
    private const string CreatedFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";
    private const int NonceLength = 16;
    private const int IanaIeee80211 = 71;

    private readonly NetworkCredential? credential;
    private readonly HttpClient httpClient;
    private readonly NetworkInstrumentation instrumentation;
    private bool clockRead;
    private Uri? mediaServiceUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnvifClient"/> class.
    /// </summary>
    /// <param name="deviceServiceUrl">
    /// The device service URL, such as <c>http://192.168.1.132:2020/onvif/device_service</c>
    /// (see <see cref="Discovery.OnvifDevice.DeviceServiceUrl"/>).
    /// </param>
    /// <param name="credential">The ONVIF user, or <see langword="null"/> for requests that need none.</param>
    /// <param name="httpClient">The HTTP client to use; the caller keeps owning it. The default is a shared client.</param>
    public OnvifClient(
        Uri deviceServiceUrl,
        NetworkCredential? credential = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(deviceServiceUrl);

        DeviceServiceUrl = deviceServiceUrl;
        this.credential = credential;
        this.httpClient = httpClient ?? SharedHttpClients.Api;
        instrumentation = new NetworkInstrumentation("onvif_client", deviceServiceUrl.Host, deviceServiceUrl.Port);
    }

    /// <summary>
    /// Gets the device service URL.
    /// </summary>
    public Uri DeviceServiceUrl { get; }

    /// <summary>
    /// Gets how far the device's clock is ahead of the local clock, as read by <see cref="GetSystemDateAndTime"/>.
    /// </summary>
    public TimeSpan ClockOffset { get; private set; }

    /// <summary>
    /// Gets the time to wait for each response. The default is 5 seconds.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Reads the device's clock (UTC). Needs no credentials, so it also tells whether ONVIF answers at the URL.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The device's time, in UTC.</returns>
    /// <exception cref="OnvifException">The device answered with a fault, an HTTP error, or an unreadable response.</exception>
    /// <exception cref="TimeoutException">The device did not answer in time.</exception>
    public async Task<DateTime> GetSystemDateAndTime(
        CancellationToken cancellationToken = default)
    {
        var response = await Send(DeviceServiceUrl, Device("GetSystemDateAndTime"), authenticate: false, cancellationToken);
        var utc = response.Descendant("UTCDateTime");

        int Read(string name)
            => int.TryParse(utc?.DescendantValue(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : throw new OnvifException($"The device's clock has no {name}.");

        try
        {
            var deviceTime = new DateTime(Read("Year"), Read("Month"), Read("Day"), Read("Hour"), Read("Minute"), Read("Second"), DateTimeKind.Utc);
            ClockOffset = deviceTime - DateTime.UtcNow;
            clockRead = true;
            return deviceTime;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new OnvifException("The device's clock is not a valid time.", ex);
        }
    }

    /// <summary>
    /// Reads the manufacturer, model, firmware version and serial number.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The device information.</returns>
    /// <exception cref="OnvifException">The device answered with a fault (such as NotAuthorized), an HTTP error, or an unreadable response.</exception>
    /// <exception cref="TimeoutException">The device did not answer in time.</exception>
    public async Task<OnvifDeviceInformation> GetDeviceInformation(
        CancellationToken cancellationToken = default)
    {
        var response = await Send(DeviceServiceUrl, Device("GetDeviceInformation"), authenticate: true, cancellationToken);
        return new OnvifDeviceInformation(
            response.DescendantValue("Manufacturer"),
            response.DescendantValue("Model"),
            response.DescendantValue("FirmwareVersion"),
            response.DescendantValue("SerialNumber"),
            response.DescendantValue("HardwareId"));
    }

    /// <summary>
    /// Reads the URLs of the device's services.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The service URLs.</returns>
    /// <exception cref="OnvifException">The device answered with a fault, an HTTP error, or an unreadable response.</exception>
    /// <exception cref="TimeoutException">The device did not answer in time.</exception>
    public async Task<OnvifCapabilities> GetCapabilities(
        CancellationToken cancellationToken = default)
    {
        XNamespace tds = DeviceNamespace;
        var request = new XElement(tds + "GetCapabilities", new XElement(tds + "Category", "All"));
        var response = await Send(DeviceServiceUrl, request, authenticate: true, cancellationToken);

        Uri? ServiceUrl(string category)
            => Uri.TryCreate(response.Descendant(category)?.DescendantValue("XAddr"), UriKind.Absolute, out var url)
                ? url
                : null;

        return new OnvifCapabilities(ServiceUrl("Media"), ServiceUrl("PTZ"), ServiceUrl("Imaging"), ServiceUrl("Events"));
    }

    /// <summary>
    /// Reads the media profiles: one per stream setup, such as the main and the sub stream.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The profiles.</returns>
    /// <exception cref="OnvifException">The device answered with a fault, an HTTP error, or an unreadable response.</exception>
    /// <exception cref="TimeoutException">The device did not answer in time.</exception>
    public async Task<IReadOnlyList<OnvifMediaProfile>> GetProfiles(
        CancellationToken cancellationToken = default)
    {
        var url = await GetMediaServiceUrl(cancellationToken);
        var response = await Send(url, new XElement(XName.Get("GetProfiles", MediaNamespace)), authenticate: true, cancellationToken);
        return ReadProfiles(response);
    }

    /// <summary>
    /// Reads the RTSP URI of a media profile.
    /// </summary>
    /// <param name="profileToken">The profile token (<see cref="OnvifMediaProfile.Token"/>).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The RTSP URI as the device reports it, without credentials.</returns>
    /// <exception cref="OnvifException">The device answered with a fault, an HTTP error, or an unreadable response.</exception>
    /// <exception cref="TimeoutException">The device did not answer in time.</exception>
    public async Task<Uri> GetStreamUri(
        string profileToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileToken);

        XNamespace trt = MediaNamespace;
        XNamespace tt = SchemaNamespace;
        var request = new XElement(
            trt + "GetStreamUri",
            new XElement(
                trt + "StreamSetup",
                new XElement(tt + "Stream", "RTP-Unicast"),
                new XElement(tt + "Transport", new XElement(tt + "Protocol", "RTSP"))),
            new XElement(trt + "ProfileToken", profileToken));

        var url = await GetMediaServiceUrl(cancellationToken);
        var response = await Send(url, request, authenticate: true, cancellationToken);

        return Uri.TryCreate(response.Descendant("MediaUri")?.DescendantValue("Uri"), UriKind.Absolute, out var streamUri)
            ? streamUri
            : throw new OnvifException($"The device returned no stream URI for profile '{profileToken}'.");
    }

    /// <summary>
    /// Reads every media profile with its RTSP URI.
    /// </summary>
    /// <param name="replaceHostWithDeviceHost">
    /// Whether to replace the host in each stream URI with the host of <see cref="DeviceServiceUrl"/>. Cameras often report
    /// an old or internal address in their stream URIs. The default is <see langword="true"/>.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The profiles with their stream URIs, without credentials.</returns>
    /// <exception cref="OnvifException">The device answered with a fault, an HTTP error, or an unreadable response.</exception>
    /// <exception cref="TimeoutException">The device did not answer in time.</exception>
    public async Task<IReadOnlyList<OnvifStreamInfo>> GetStreams(
        bool replaceHostWithDeviceHost = true,
        CancellationToken cancellationToken = default)
    {
        var streams = new List<OnvifStreamInfo>();
        foreach (var profile in await GetProfiles(cancellationToken))
        {
            var streamUri = await GetStreamUri(profile.Token, cancellationToken);
            if (replaceHostWithDeviceHost)
            {
                streamUri = new UriBuilder(streamUri) { Host = DeviceServiceUrl.Host }.Uri;
            }

            streams.Add(new OnvifStreamInfo(profile, streamUri));
        }

        return streams;
    }

    /// <summary>
    /// Reads every media profile with its RTSP URI, with the device's host in each URI.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The profiles with their stream URIs, without credentials.</returns>
    /// <exception cref="OnvifException">The device answered with a fault, an HTTP error, or an unreadable response.</exception>
    /// <exception cref="TimeoutException">The device did not answer in time.</exception>
    public Task<IReadOnlyList<OnvifStreamInfo>> GetStreams(
        CancellationToken cancellationToken)
        => GetStreams(replaceHostWithDeviceHost: true, cancellationToken);

    /// <summary>
    /// Reads the device's network interfaces, including whether each is wired or Wi-Fi.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The network interfaces.</returns>
    /// <exception cref="OnvifException">The device answered with a fault, an HTTP error, or an unreadable response.</exception>
    /// <exception cref="TimeoutException">The device did not answer in time.</exception>
    public async Task<IReadOnlyList<OnvifNetworkInterface>> GetNetworkInterfaces(
        CancellationToken cancellationToken = default)
    {
        var response = await Send(DeviceServiceUrl, Device("GetNetworkInterfaces"), authenticate: true, cancellationToken);
        return ReadNetworkInterfaces(response);
    }

    /// <summary>
    /// WS-Security PasswordDigest: Base64(SHA-1(nonce + created + password)), with the raw nonce bytes.
    /// </summary>
    /// <param name="nonce">The nonce bytes.</param>
    /// <param name="created">The created time, as written in the token.</param>
    /// <param name="password">The password.</param>
    /// <returns>The digest.</returns>
    [SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "SHA-1 is required by the WS-Security UsernameToken profile that ONVIF devices implement.")]
    [SuppressMessage("Security", "S4790:Using weak hashing algorithms is security-sensitive", Justification = "SHA-1 is required by the WS-Security UsernameToken profile that ONVIF devices implement.")]
    internal static string ComputePasswordDigest(
        byte[] nonce,
        string created,
        string password)
    {
        var createdBytes = Encoding.UTF8.GetBytes(created);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var buffer = new byte[nonce.Length + createdBytes.Length + passwordBytes.Length];
        nonce.CopyTo(buffer, 0);
        createdBytes.CopyTo(buffer, nonce.Length);
        passwordBytes.CopyTo(buffer, nonce.Length + createdBytes.Length);
        return Convert.ToBase64String(SHA1.HashData(buffer));
    }

    /// <summary>
    /// Creates a SOAP 1.2 envelope with the body, and a WS-Security UsernameToken when there is a credential.
    /// </summary>
    /// <param name="body">The operation element.</param>
    /// <param name="credential">The credential, or <see langword="null"/>.</param>
    /// <param name="createdUtc">The token's created time, by the device's clock.</param>
    /// <param name="nonce">The token's nonce.</param>
    /// <returns>The envelope.</returns>
    internal static string CreateEnvelope(
        XElement body,
        NetworkCredential? credential,
        DateTime createdUtc,
        byte[] nonce)
    {
        XNamespace s = SoapNamespace;
        var header = new XElement(s + "Header");
        if (credential is not null && !string.IsNullOrEmpty(credential.UserName))
        {
            XNamespace wsse = WsseNamespace;
            XNamespace wsu = WsuNamespace;
            var created = createdUtc.ToString(CreatedFormat, CultureInfo.InvariantCulture);
            header.Add(new XElement(
                wsse + "Security",
                new XAttribute(s + "mustUnderstand", "1"),
                new XElement(
                    wsse + "UsernameToken",
                    new XElement(wsse + "Username", credential.UserName),
                    new XElement(
                        wsse + "Password",
                        new XAttribute("Type", PasswordDigestType),
                        ComputePasswordDigest(nonce, created, credential.Password ?? string.Empty)),
                    new XElement(wsse + "Nonce", new XAttribute("EncodingType", Base64EncodingType), Convert.ToBase64String(nonce)),
                    new XElement(wsu + "Created", created))));
        }

        var envelope = new XElement(
            s + "Envelope",
            new XAttribute(XNamespace.Xmlns + "s", SoapNamespace),
            header,
            new XElement(s + "Body", body));

        return envelope.ToString(SaveOptions.DisableFormatting);
    }

    private static XElement Device(string operation)
        => new(XName.Get(operation, DeviceNamespace));

    private static List<OnvifMediaProfile> ReadProfiles(XDocument response)
        => response
            .Descendants()
            .Where(x => x.Name.LocalName == "Profiles")
            .Select(ReadProfile)
            .ToList();

    private static List<OnvifNetworkInterface> ReadNetworkInterfaces(
        XDocument response)
        => response
            .Descendants()
            .Where(x => x.Name.LocalName == "NetworkInterfaces")
            .Select(ReadNetworkInterface)
            .ToList();

    private static OnvifMediaProfile ReadProfile(XElement profile)
    {
        var encoder = profile.Descendant("VideoEncoderConfiguration");
        int Read(
            XElement? element,
            string name)
            => int.TryParse(element?.DescendantValue(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;

        return new OnvifMediaProfile(
            profile.Attribute("token")?.Value ?? string.Empty,
            profile.Elements().FirstOrDefault(x => x.Name.LocalName == "Name")?.Value.Trim(),
            encoder?.DescendantValue("Encoding"),
            Read(encoder?.Descendant("Resolution"), "Width"),
            Read(encoder?.Descendant("Resolution"), "Height"),
            Read(encoder?.Descendant("RateControl"), "FrameRateLimit"));
    }

    private static OnvifNetworkInterface ReadNetworkInterface(
        XElement networkInterface)
    {
        var info = networkInterface.Descendant("Info");
        var extension = networkInterface.Elements().FirstOrDefault(x => x.Name.LocalName == "Extension");
        var isWireless = extension is not null &&
                         (extension.Descendant("Dot11") is not null ||
                          string.Equals(extension.DescendantValue("InterfaceType"), IanaIeee80211.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));

        var addresses = networkInterface
            .Descendant("IPv4")?
            .Descendants()
            .Where(x => x.Name.LocalName == "Address")
            .Select(x => IPAddress.TryParse(x.Value.Trim(), out var address) ? address : null)
            .OfType<IPAddress>()
            .Distinct()
            .ToList() ?? [];

        return new OnvifNetworkInterface(
            networkInterface.Attribute("token")?.Value ?? string.Empty,
            info?.DescendantValue("Name"),
            !string.Equals(networkInterface.Elements().FirstOrDefault(x => x.Name.LocalName == "Enabled")?.Value.Trim(), "false", StringComparison.OrdinalIgnoreCase),
            info?.DescendantValue("HwAddress"),
            addresses,
            isWireless);
    }

    private static OnvifException CreateException(
        HttpStatusCode statusCode,
        string body)
    {
        if (SafeXmlHelper.TryLoad(body, out var document) && document.Descendant("Fault") is { } fault)
        {
            var codeElement = fault.Descendant("Code");
            var code = codeElement?.Elements().FirstOrDefault(x => x.Name.LocalName == "Value")?.Value.Trim();
            var subcode = codeElement?.Descendant("Subcode")?.DescendantValue("Value");
            var reason = fault.Descendant("Reason")?.DescendantValue("Text");
            return new OnvifException($"The device answered with a fault: {subcode ?? code} {reason}".TrimEnd(), statusCode, code, subcode, reason);
        }

        var message = statusCode == HttpStatusCode.Unauthorized
            ? "The device answered HTTP 401: it rejected the credentials, or it only accepts HTTP digest authentication, which is not supported."
            : $"The device answered HTTP {(int)statusCode}.";
        return new OnvifException(message, statusCode, code: null, subcode: null, reason: null);
    }

    private async Task<Uri> GetMediaServiceUrl(
        CancellationToken cancellationToken)
    {
        if (mediaServiceUrl is null)
        {
            // Many devices serve every service on the device service URL; use that when the media URL is missing.
            mediaServiceUrl = (await GetCapabilities(cancellationToken)).MediaServiceUrl ?? DeviceServiceUrl;
        }

        return mediaServiceUrl;
    }

    private async Task<XDocument> Send(
        Uri url,
        XElement body,
        bool authenticate,
        CancellationToken cancellationToken)
    {
        var operation = body.Name.LocalName;
        var useCredential = authenticate && credential is not null;
        if (useCredential && !clockRead)
        {
            await ReadClockForToken(cancellationToken);
        }

        using var activity = instrumentation.StartActivity("OnvifClient." + operation, ActivityKind.Client);
        var envelope = CreateEnvelope(
            body,
            useCredential ? credential : null,
            DateTime.UtcNow + ClockOffset,
            RandomNumberGenerator.GetBytes(NonceLength));

        using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellationTokenSource.CancelAfter(Timeout);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(envelope, Encoding.UTF8);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(
                $"application/soap+xml; charset=utf-8; action=\"{body.Name.NamespaceName}/{operation}\"");

            using var response = await httpClient.SendAsync(request, timeoutCancellationTokenSource.Token);
            instrumentation.Sent(envelope.Length);
            var responseBody = await response.Content.ReadAsStringAsync(timeoutCancellationTokenSource.Token);
            instrumentation.Received(responseBody.Length);

            if (!response.IsSuccessStatusCode)
            {
                throw CreateException(response.StatusCode, responseBody);
            }

            if (!SafeXmlHelper.TryLoad(responseBody, out var document))
            {
                throw new OnvifException($"The device's answer to {operation} is not valid XML.");
            }

            return document.Descendant("Fault") is null
                ? document
                : throw CreateException(response.StatusCode, responseBody);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Timeout");
            throw new TimeoutException($"The device at {url} did not answer {operation} within {Timeout.TotalSeconds:0.#} seconds.", ex);
        }
        catch (HttpRequestException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw new OnvifException($"The request {operation} to {url} failed: {ex.Message}", ex);
        }
        catch (OnvifException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Reads the device's clock for the token's created time. When that fails, the local clock is used.
    /// </summary>
    private async Task ReadClockForToken(CancellationToken cancellationToken)
    {
        try
        {
            await GetSystemDateAndTime(cancellationToken);
        }
        catch (OnvifException)
        {
            clockRead = true;
        }
    }
}