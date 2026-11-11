namespace Atc.Network.Discovery;

/// <summary>
/// Finds services on the local network with mDNS / DNS-SD (Bonjour, Avahi): printers, web interfaces,
/// Chromecasts, AirPlay devices, NAS boxes and other devices that announce themselves.
/// </summary>
/// <remarks>
/// <para>
/// Queries are sent to 224.0.0.251:5353 on every IPv4 network interface, from a random port, so the
/// responders answer by unicast ("one-shot" queries, RFC 6762, section 5.1). Port 5353 itself is not used,
/// because the operating system often has it already. IPv4 only.
/// </para>
/// <para>
/// Each sequence ends normally when the duration is over. Cancelling the token before that throws an
/// <see cref="OperationCanceledException"/>.
/// </para>
/// </remarks>
public static class MdnsDiscovery
{
    /// <summary>
    /// The DNS-SD name that lists every service type on the network (RFC 6763, section 9).
    /// </summary>
    public const string ServiceTypesName = "_services._dns-sd._udp.local";

    /// <summary>
    /// The default time to collect answers, in milliseconds.
    /// </summary>
    public const int DefaultDurationInMs = 3_000;

    private const string LocalDomain = ".local";
    private const int MulticastTimeToLive = 255;
    private const string Component = "mdns_discovery";

    /// <summary>
    /// Gets the mDNS multicast endpoint: 224.0.0.251:5353.
    /// </summary>
    public static IPEndPoint MulticastEndPoint
        => new(new IPAddress([224, 0, 0, 251]), 5353);

    /// <summary>
    /// Finds the instances of one service type, such as <c>_http._tcp.local</c> or <c>_ipp._tcp</c>.
    /// </summary>
    /// <param name="serviceType">The service type. ".local" is added when it is missing.</param>
    /// <param name="duration">How long to collect answers. The default is 3 seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Each instance once, as soon as its host, port and address are known. Instances that are still
    /// incomplete when the duration is over are returned at the end, with what is known.
    /// </returns>
    /// <exception cref="OperationCanceledException">The cancellation token was cancelled before the duration was over.</exception>
    /// <exception cref="SocketException">The query could not be sent.</exception>
    public static IAsyncEnumerable<MdnsServiceInstance> Browse(
        string serviceType,
        TimeSpan? duration = null,
        CancellationToken cancellationToken = default)
        => Browse(serviceType, duration ?? DefaultDuration, MulticastEndPoint, cancellationToken);

    /// <summary>
    /// Finds every service type on the network, and the instances of each.
    /// </summary>
    /// <param name="duration">How long to collect answers. The default is 3 seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Each instance once, as soon as its host, port and address are known. Instances that are still
    /// incomplete when the duration is over are returned at the end, with what is known.
    /// </returns>
    /// <exception cref="OperationCanceledException">The cancellation token was cancelled before the duration was over.</exception>
    /// <exception cref="SocketException">The query could not be sent.</exception>
    public static IAsyncEnumerable<MdnsServiceInstance> BrowseAll(
        TimeSpan? duration = null,
        CancellationToken cancellationToken = default)
        => BrowseAll(duration ?? DefaultDuration, MulticastEndPoint, cancellationToken);

    /// <summary>
    /// Finds the service types on the network, such as <c>_http._tcp.local</c> and <c>_googlecast._tcp.local</c>.
    /// </summary>
    /// <param name="duration">How long to collect answers. The default is 3 seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Each service type once, as it is found.</returns>
    /// <exception cref="OperationCanceledException">The cancellation token was cancelled before the duration was over.</exception>
    /// <exception cref="SocketException">The query could not be sent.</exception>
    public static IAsyncEnumerable<string> GetServiceTypes(
        TimeSpan? duration = null,
        CancellationToken cancellationToken = default)
        => GetServiceTypes(duration ?? DefaultDuration, MulticastEndPoint, cancellationToken);

    /// <summary>
    /// Browses one service type, querying <paramref name="target"/>: a multicast or (in tests) a unicast endpoint.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="duration">How long to collect answers.</param>
    /// <param name="target">Where to send the queries.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The instances.</returns>
    internal static IAsyncEnumerable<MdnsServiceInstance> Browse(
        string serviceType,
        TimeSpan duration,
        IPEndPoint target,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceType);

        var normalized = NormalizeServiceType(serviceType);
        return Run(new MdnsBrowser([normalized], browseEveryServiceType: false), normalized, "MdnsDiscovery.Browse", duration, target, cancellationToken);
    }

    /// <summary>
    /// Browses every service type, querying <paramref name="target"/>.
    /// </summary>
    /// <param name="duration">How long to collect answers.</param>
    /// <param name="target">Where to send the queries.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The instances.</returns>
    internal static IAsyncEnumerable<MdnsServiceInstance> BrowseAll(
        TimeSpan duration,
        IPEndPoint target,
        CancellationToken cancellationToken)
        => Run(new MdnsBrowser([], browseEveryServiceType: true), ServiceTypesName, "MdnsDiscovery.BrowseAll", duration, target, cancellationToken);

    /// <summary>
    /// Finds the service types, querying <paramref name="target"/>.
    /// </summary>
    /// <param name="duration">How long to collect answers.</param>
    /// <param name="target">Where to send the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The service types.</returns>
    internal static async IAsyncEnumerable<string> GetServiceTypes(
        TimeSpan duration,
        IPEndPoint target,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var session = new DiscoverySession(target, MulticastTimeToLive, Component, duration, cancellationToken);
        using var activity = session.Instrumentation.StartActivity("MdnsDiscovery.GetServiceTypes", ActivityKind.Client);

        await session.SendAsync(DnsMessage.CreateQuery([new DnsQuestion(ServiceTypesName, DnsRecordType.Ptr)]));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await session.ReceiveAsync() is { } answer)
        {
            if (!DnsMessage.TryParse(answer.Data, out var message) || !message.IsResponse)
            {
                continue;
            }

            foreach (var record in message.Records)
            {
                if (record.Type == DnsRecordType.Ptr &&
                    string.Equals(record.Name, ServiceTypesName, StringComparison.OrdinalIgnoreCase) &&
                    seen.Add(record.DomainName!))
                {
                    yield return record.DomainName!;
                }
            }
        }
    }

    private static TimeSpan DefaultDuration
        => TimeSpan.FromMilliseconds(DefaultDurationInMs);

    private static async IAsyncEnumerable<MdnsServiceInstance> Run(
        MdnsBrowser browser,
        string firstQuestion,
        string activityName,
        TimeSpan duration,
        IPEndPoint target,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var session = new DiscoverySession(target, MulticastTimeToLive, Component, duration, cancellationToken);
        using var activity = session.Instrumentation.StartActivity(activityName, ActivityKind.Client);

        await session.SendAsync(DnsMessage.CreateQuery([new DnsQuestion(firstQuestion, DnsRecordType.Ptr)]));

        while (await session.ReceiveAsync() is { } answer)
        {
            if (!DnsMessage.TryParse(answer.Data, out var message) || !message.IsResponse)
            {
                continue;
            }

            var questions = browser.Add(message.Records);
            if (questions.Count > 0)
            {
                await session.SendAsync(DnsMessage.CreateQuery(questions));
            }

            foreach (var instance in browser.TakeCompleted())
            {
                yield return instance;
            }
        }

        foreach (var instance in browser.TakeRemaining())
        {
            yield return instance;
        }
    }

    private static string NormalizeServiceType(string serviceType)
    {
        var trimmed = serviceType.Trim().TrimEnd('.');
        return trimmed.EndsWith(LocalDomain, StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : trimmed + LocalDomain;
    }
}