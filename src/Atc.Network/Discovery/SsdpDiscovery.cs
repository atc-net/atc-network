namespace Atc.Network.Discovery;

/// <summary>
/// Finds UPnP devices and services on the local network with SSDP (Simple Service Discovery Protocol):
/// routers, TVs, media servers, printers, NAS boxes and other devices.
/// </summary>
/// <remarks>
/// An M-SEARCH request is sent to 239.255.255.250:1900 on every IPv4 network interface; devices answer
/// by unicast with their headers, such as SERVER and LOCATION. IPv4 only.
/// </remarks>
public static class SsdpDiscovery
{
    /// <summary>
    /// The search target for all devices and services.
    /// </summary>
    public const string DefaultSearchTarget = "ssdp:all";

    /// <summary>
    /// The default time to collect answers, in milliseconds.
    /// </summary>
    public const int DefaultDurationInMs = 3_000;

    private const int MulticastTimeToLive = 4;
    private const int MaxWaitInSeconds = 3;

    /// <summary>
    /// Gets the SSDP multicast endpoint: 239.255.255.250:1900.
    /// </summary>
    public static IPEndPoint MulticastEndPoint
        => new(new IPAddress([239, 255, 255, 250]), 1900);

    /// <summary>
    /// Searches the local network, and returns each device or service as it answers.
    /// </summary>
    /// <param name="searchTarget">
    /// What to search for: <c>ssdp:all</c> (the default), <c>upnp:rootdevice</c> for one answer per device,
    /// or a device or service type such as <c>urn:schemas-upnp-org:device:MediaRenderer:1</c>.
    /// </param>
    /// <param name="duration">How long to collect answers. The default is 3 seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The devices and services, each unique service name (USN) once. The sequence ends when the duration is over.
    /// </returns>
    /// <exception cref="OperationCanceledException">The cancellation token was cancelled before the duration was over.</exception>
    /// <exception cref="SocketException">The search could not be sent.</exception>
    public static IAsyncEnumerable<SsdpDevice> Search(
        string searchTarget = DefaultSearchTarget,
        TimeSpan? duration = null,
        CancellationToken cancellationToken = default)
        => Search(
            searchTarget,
            duration ?? TimeSpan.FromMilliseconds(DefaultDurationInMs),
            MulticastEndPoint,
            cancellationToken);

    /// <summary>
    /// Searches by sending the request to <paramref name="target"/>, a multicast or (in tests) a unicast endpoint.
    /// </summary>
    /// <param name="searchTarget">The search target (ST).</param>
    /// <param name="duration">How long to collect answers.</param>
    /// <param name="target">Where to send the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The devices and services, each USN once.</returns>
    internal static async IAsyncEnumerable<SsdpDevice> Search(
        string searchTarget,
        TimeSpan duration,
        IPEndPoint target,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTarget);

        using var session = new DiscoverySession(target, MulticastTimeToLive, "ssdp_discovery", duration, cancellationToken);
        using var activity = session.Instrumentation.StartActivity("SsdpDiscovery.Search", ActivityKind.Client);

        // Devices wait a random time up to MX seconds before answering, so keep MX below the duration.
        var maxWaitInSeconds = System.Math.Clamp((int)duration.TotalSeconds - 1, 1, MaxWaitInSeconds);
        await session.SendAsync(CreateSearchRequest(searchTarget, maxWaitInSeconds));

        var seen = new HashSet<string>(StringComparer.Ordinal);
        while (await session.ReceiveAsync() is { } answer)
        {
            var device = ParseResponse(Encoding.UTF8.GetString(answer.Data), answer.From.Address);
            if (device is not null && seen.Add(device.UniqueServiceName))
            {
                yield return device;
            }
        }
    }

    /// <summary>
    /// Creates an M-SEARCH request.
    /// </summary>
    /// <param name="searchTarget">The search target (ST).</param>
    /// <param name="maxWaitInSeconds">The longest time a device may wait before answering (MX).</param>
    /// <returns>The request.</returns>
    internal static byte[] CreateSearchRequest(
        string searchTarget,
        int maxWaitInSeconds)
        => Encoding.ASCII.GetBytes(
            "M-SEARCH * HTTP/1.1\r\n" +
            "HOST: 239.255.255.250:1900\r\n" +
            "MAN: \"ssdp:discover\"\r\n" +
            $"MX: {maxWaitInSeconds.ToString(CultureInfo.InvariantCulture)}\r\n" +
            $"ST: {searchTarget}\r\n" +
            "\r\n");

    /// <summary>
    /// Reads a search response. Returns <see langword="null"/> for anything else, such as a NOTIFY
    /// announcement, an error status or a response without a USN header.
    /// </summary>
    /// <param name="response">The response text.</param>
    /// <param name="ipAddress">The address the response came from.</param>
    /// <returns>The device, or <see langword="null"/>.</returns>
    internal static SsdpDevice? ParseResponse(
        string response,
        IPAddress ipAddress)
    {
        var lines = response.Split('\n');
        var statusLine = lines[0].Trim();
        if (!statusLine.StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase) ||
            statusLine.Split(' ', StringSplitOptions.RemoveEmptyEntries) is not [_, "200", ..])
        {
            return null;
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines.Skip(1))
        {
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator > 0)
            {
                headers[line[..separator].Trim()] = line[(separator + 1)..].Trim();
            }
        }

        return headers.TryGetValue("USN", out var usn) && usn.Length > 0
            ? new SsdpDevice(ipAddress, headers)
            : null;
    }
}