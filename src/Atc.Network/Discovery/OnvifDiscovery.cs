namespace Atc.Network.Discovery;

/// <summary>
/// Finds ONVIF devices on the local network with WS-Discovery: IP cameras of every brand that support ONVIF,
/// video encoders, NVRs and other ONVIF devices.
/// </summary>
/// <remarks>
/// <para>
/// Two probes are sent to 239.255.255.250:3702 on every IPv4 network interface, from a random port: one for cameras
/// (<c>NetworkVideoTransmitter</c>) and one for every ONVIF device (<c>Device</c>), since some devices only answer one
/// of them. Devices answer by unicast with a ProbeMatch: their name, hardware and device service URL.
/// </para>
/// <para>
/// The sequence ends normally when the duration is over. Cancelling the token before that throws an
/// <see cref="OperationCanceledException"/>. Many cameras ship with ONVIF turned off; those don't answer.
/// </para>
/// </remarks>
public static class OnvifDiscovery
{
    /// <summary>
    /// The device type of a camera or video encoder.
    /// </summary>
    public const string NetworkVideoTransmitterType = "NetworkVideoTransmitter";

    /// <summary>
    /// The device type every ONVIF device has.
    /// </summary>
    public const string DeviceType = "Device";

    /// <summary>
    /// The default time to collect answers, in milliseconds.
    /// </summary>
    public const int DefaultDurationInMs = 3_000;

    private const string Component = "onvif_discovery";
    private const int MulticastTimeToLive = 4;
    private const string SoapNamespace = "http://www.w3.org/2003/05/soap-envelope";
    private const string AddressingNamespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
    private const string DiscoveryNamespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery";
    private const string NetworkNamespace = "http://www.onvif.org/ver10/network/wsdl";
    private const string DeviceNamespace = "http://www.onvif.org/ver10/device/wsdl";

    /// <summary>
    /// Gets the WS-Discovery multicast endpoint: 239.255.255.250:3702.
    /// </summary>
    public static IPEndPoint MulticastEndPoint
        => new(new IPAddress([239, 255, 255, 250]), 3702);

    /// <summary>
    /// Finds the ONVIF devices on the local network, and returns each as it answers.
    /// </summary>
    /// <param name="duration">How long to collect answers. The default is 3 seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Each device once (by its endpoint reference). The sequence ends when the duration is over.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was cancelled before the duration was over.</exception>
    /// <exception cref="SocketException">The probe could not be sent.</exception>
    public static IAsyncEnumerable<OnvifDevice> Probe(
        TimeSpan? duration = null,
        CancellationToken cancellationToken = default)
        => Probe(duration ?? TimeSpan.FromMilliseconds(DefaultDurationInMs), MulticastEndPoint, cancellationToken);

    /// <summary>
    /// Probes by sending to <paramref name="target"/>, a multicast or (in tests) a unicast endpoint.
    /// </summary>
    /// <param name="duration">How long to collect answers.</param>
    /// <param name="target">Where to send the probes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The devices, each once.</returns>
    internal static async IAsyncEnumerable<OnvifDevice> Probe(
        TimeSpan duration,
        IPEndPoint target,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var session = new DiscoverySession(target, MulticastTimeToLive, Component, duration, cancellationToken);
        using var activity = session.Instrumentation.StartActivity("OnvifDiscovery.Probe", ActivityKind.Client);

        var messageIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in new[] { NetworkVideoTransmitterType, DeviceType })
        {
            var messageId = "uuid:" + Guid.NewGuid().ToString("D");
            messageIds.Add(messageId);
            await session.SendAsync(CreateProbe(messageId, type));
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await session.ReceiveAsync() is { } answer)
        {
            foreach (var device in ParseProbeMatches(answer.Data, answer.From.Address, messageIds.Contains))
            {
                var key = device.EndpointReference.Length > 0
                    ? device.EndpointReference
                    : device.DeviceServiceUrl?.ToString() ?? device.IPAddress.ToString();
                if (seen.Add(key))
                {
                    yield return device;
                }
            }
        }
    }

    /// <summary>
    /// Creates a WS-Discovery probe for one device type.
    /// </summary>
    /// <param name="messageId">The probe's message id, which answers repeat as RelatesTo.</param>
    /// <param name="type"><see cref="NetworkVideoTransmitterType"/> or <see cref="DeviceType"/>.</param>
    /// <returns>The probe.</returns>
    internal static byte[] CreateProbe(
        string messageId,
        string type)
    {
        XNamespace s = SoapNamespace;
        XNamespace a = AddressingNamespace;
        XNamespace d = DiscoveryNamespace;
        var typeNamespace = string.Equals(type, DeviceType, StringComparison.Ordinal)
            ? DeviceNamespace
            : NetworkNamespace;

        var envelope = new XElement(
            s + "Envelope",
            new XAttribute(XNamespace.Xmlns + "s", SoapNamespace),
            new XAttribute(XNamespace.Xmlns + "a", AddressingNamespace),
            new XElement(
                s + "Header",
                new XElement(a + "Action", new XAttribute(s + "mustUnderstand", "1"), DiscoveryNamespace + "/Probe"),
                new XElement(a + "MessageID", messageId),
                new XElement(a + "ReplyTo", new XElement(a + "Address", AddressingNamespace + "/role/anonymous")),
                new XElement(a + "To", new XAttribute(s + "mustUnderstand", "1"), "urn:schemas-xmlsoap-org:ws:2005:04:discovery")),
            new XElement(
                s + "Body",
                new XElement(
                    d + "Probe",
                    new XAttribute(XNamespace.Xmlns + "d", DiscoveryNamespace),
                    new XElement(
                        d + "Types",
                        new XAttribute(XNamespace.Xmlns + "dp0", typeNamespace),
                        "dp0:" + type))));

        return Encoding.UTF8.GetBytes(envelope.ToString(SaveOptions.DisableFormatting));
    }

    /// <summary>
    /// Reads the devices of a ProbeMatches answer to the probe with <paramref name="messageId"/>.
    /// </summary>
    /// <param name="answer">The datagram.</param>
    /// <param name="ipAddress">The address the answer came from.</param>
    /// <param name="messageId">The probe's message id.</param>
    /// <returns>The devices; empty for an answer to another probe, an empty ProbeMatches or malformed XML.</returns>
    internal static IReadOnlyList<OnvifDevice> ParseProbeMatches(
        byte[] answer,
        IPAddress ipAddress,
        string messageId)
        => ParseProbeMatches(answer, ipAddress, x => string.Equals(x, messageId, StringComparison.Ordinal));

    private static List<OnvifDevice> ParseProbeMatches(
        byte[] answer,
        IPAddress ipAddress,
        Func<string, bool> isOurProbe)
    {
        var devices = new List<OnvifDevice>();
        if (!SafeXmlHelper.TryLoad(Encoding.UTF8.GetString(answer), out var document))
        {
            return devices;
        }

        // Some devices leave RelatesTo out; when it is there, it must name one of our probes.
        var relatesTo = document.DescendantValue("RelatesTo");
        if (relatesTo is not null && !isOurProbe(relatesTo))
        {
            return devices;
        }

        foreach (var probeMatch in document.Descendants().Where(x => x.Name.LocalName == "ProbeMatch"))
        {
            var endpointReference = probeMatch.Descendant("EndpointReference")?.DescendantValue("Address") ?? string.Empty;
            var xAddrs = SplitList(probeMatch.DescendantValue("XAddrs"))
                .Select(x => Uri.TryCreate(x, UriKind.Absolute, out var uri) ? uri : null)
                .OfType<Uri>()
                .ToList();

            if (endpointReference.Length == 0 && xAddrs.Count == 0)
            {
                continue;
            }

            devices.Add(new OnvifDevice(
                ipAddress,
                endpointReference,
                SplitList(probeMatch.DescendantValue("Types")),
                SplitList(probeMatch.DescendantValue("Scopes")).Select(Uri.UnescapeDataString).ToList(),
                xAddrs));
        }

        return devices;
    }

    private static List<string> SplitList(string? value)
        => value is null
            ? []
            : value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).ToList();
}