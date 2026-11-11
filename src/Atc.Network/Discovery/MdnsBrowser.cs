namespace Atc.Network.Discovery;

/// <summary>
/// Collects the records of mDNS answers into service instances, and works out which follow-up queries
/// are needed for records that did not come with the answer.
/// </summary>
internal sealed class MdnsBrowser
{
    private readonly bool browseEveryServiceType;
    private readonly HashSet<string> serviceTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InstanceState> instances = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<IPAddress>> addresses = new(StringComparer.OrdinalIgnoreCase);

    public MdnsBrowser(
        IEnumerable<string> serviceTypes,
        bool browseEveryServiceType)
    {
        this.serviceTypes.UnionWith(serviceTypes);
        this.browseEveryServiceType = browseEveryServiceType;
    }

    /// <summary>
    /// Adds the records of an answer.
    /// </summary>
    /// <param name="records">The records.</param>
    /// <returns>The questions to ask next: new service types to browse, and missing records.</returns>
    public IReadOnlyList<DnsQuestion> Add(IReadOnlyList<DnsRecord> records)
    {
        var questions = new List<DnsQuestion>();
        foreach (var record in records)
        {
            AddRecord(record, questions);
        }

        foreach (var (fullName, state) in instances)
        {
            AddFollowUpQuestions(fullName, state, questions);
        }

        return questions;
    }

    /// <summary>
    /// Returns the instances that are complete (host, port and an address) and were not returned before.
    /// </summary>
    /// <returns>The instances.</returns>
    public IReadOnlyList<MdnsServiceInstance> TakeCompleted()
        => Take(includeIncomplete: false);

    /// <summary>
    /// Returns every instance that was not returned before, complete or not.
    /// </summary>
    /// <returns>The instances.</returns>
    public IReadOnlyList<MdnsServiceInstance> TakeRemaining()
        => Take(includeIncomplete: true);

    private void AddRecord(
        DnsRecord record,
        List<DnsQuestion> questions)
    {
        switch (record.Type)
        {
            case DnsRecordType.Ptr when browseEveryServiceType &&
                                        string.Equals(record.Name, MdnsDiscovery.ServiceTypesName, StringComparison.OrdinalIgnoreCase):
                if (serviceTypes.Add(record.DomainName!))
                {
                    questions.Add(new DnsQuestion(record.DomainName!, DnsRecordType.Ptr));
                }

                break;
            case DnsRecordType.Ptr when serviceTypes.Contains(record.Name):
                GetOrAdd(record.DomainName!, record.Name);
                break;
            case DnsRecordType.Srv:
                if (FindServiceType(record.Name) is { } srvServiceType)
                {
                    var state = GetOrAdd(record.Name, srvServiceType);
                    state.HostName = record.DomainName;
                    state.Port = record.Port;
                }

                break;
            case DnsRecordType.Txt:
                if (FindServiceType(record.Name) is { } txtServiceType)
                {
                    GetOrAdd(record.Name, txtServiceType).Properties = ParseProperties(record.Text!);
                }

                break;
            case DnsRecordType.A:
            case DnsRecordType.Aaaa:
                AddAddress(record.Name, record.Address!);
                break;
        }
    }

    private void AddFollowUpQuestions(
        string fullName,
        InstanceState state,
        List<DnsQuestion> questions)
    {
        if (state.Returned)
        {
            return;
        }

        if (state.HostName is null && !state.ServiceQueried)
        {
            state.ServiceQueried = true;
            questions.Add(new DnsQuestion(fullName, DnsRecordType.Srv));
            questions.Add(new DnsQuestion(fullName, DnsRecordType.Txt));
        }
        else if (state.HostName is not null &&
                 !addresses.ContainsKey(state.HostName) &&
                 !state.AddressQueried)
        {
            state.AddressQueried = true;
            questions.Add(new DnsQuestion(state.HostName, DnsRecordType.A));
        }
    }

    private List<MdnsServiceInstance> Take(bool includeIncomplete)
    {
        var taken = new List<MdnsServiceInstance>();
        foreach (var (fullName, state) in instances)
        {
            var hostAddresses = state.HostName is null
                ? null
                : addresses.GetValueOrDefault(state.HostName);

            if (state.Returned ||
                (!includeIncomplete && hostAddresses is null))
            {
                continue;
            }

            state.Returned = true;
            taken.Add(new MdnsServiceInstance(
                fullName,
                state.ServiceType,
                state.HostName,
                state.Port,
                hostAddresses?.ToArray() ?? [],
                state.Properties));
        }

        return taken;
    }

    private InstanceState GetOrAdd(
        string fullName,
        string serviceType)
    {
        if (!instances.TryGetValue(fullName, out var state))
        {
            state = new InstanceState(serviceType);
            instances[fullName] = state;
        }

        return state;
    }

    private string? FindServiceType(string fullName)
        => serviceTypes.FirstOrDefault(x => fullName.EndsWith("." + x, StringComparison.OrdinalIgnoreCase));

    private void AddAddress(
        string hostName,
        IPAddress address)
    {
        if (!addresses.TryGetValue(hostName, out var hostAddresses))
        {
            hostAddresses = [];
            addresses[hostName] = hostAddresses;
        }

        if (!hostAddresses.Contains(address))
        {
            hostAddresses.Add(address);
        }
    }

    private static Dictionary<string, string?> ParseProperties(
        IReadOnlyList<string> text)
    {
        var properties = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in text)
        {
            var separator = entry.IndexOf('=', StringComparison.Ordinal);
            if (separator == 0)
            {
                continue;
            }

            // RFC 6763, section 6.4: only the first occurrence of a key counts.
            var key = separator < 0 ? entry : entry[..separator];
            properties.TryAdd(key, separator < 0 ? null : entry[(separator + 1)..]);
        }

        return properties;
    }

    private sealed class InstanceState
    {
        public InstanceState(string serviceType)
        {
            ServiceType = serviceType;
        }

        public string ServiceType { get; }

        public string? HostName { get; set; }

        public int Port { get; set; }

        public IReadOnlyDictionary<string, string?> Properties { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        public bool ServiceQueried { get; set; }

        public bool AddressQueried { get; set; }

        public bool Returned { get; set; }
    }
}