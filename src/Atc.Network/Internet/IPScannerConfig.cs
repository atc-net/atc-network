namespace Atc.Network.Internet;

public class IPScannerConfig
{
    public IPScannerConfig()
    {
    }

    public IPScannerConfig(
        IPServicePortExaminationLevel ipServicePortExaminationLevel)
    {
        SetPortNumbers(ipServicePortExaminationLevel);
    }

    public IPScannerConfig(
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType serviceProtocolType)
    {
        SetPortNumbers(ipServicePortExaminationLevel, serviceProtocolType);
    }

    public IPScannerConfig(
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType[] serviceProtocolTypes)
    {
        SetPortNumbers(ipServicePortExaminationLevel, serviceProtocolTypes);
    }

    public IPScannerConfig(
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ICollection<ushort> portNumbers)
    {
        TreatOpenPortsAsWebServices = ipServicePortExaminationLevel;
        PortNumbers = portNumbers;
    }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutInMs);

    public TimeSpan TimeoutPing { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutPingInMs);

    public TimeSpan TimeoutTcp { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutTcpInMs);

    public TimeSpan TimeoutHttp { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutHttpInMs);

    public bool IcmpPing { get; set; } = true;

    public bool ResolveHostName { get; set; } = true;

    public bool ResolveMacAddress { get; set; } = true;

    public bool ResolveVendorFromMacAddress { get; set; } = true;

    public ICollection<ushort> PortNumbers { get; set; } = new List<ushort>();

    public IPServicePortExaminationLevel TreatOpenPortsAsWebServices { get; set; } = IPServicePortExaminationLevel.WellKnownAndCommon;

    /// <summary>
    /// Gets or sets a value indicating whether the scan also finds services with mDNS / DNS-SD and SSDP (UPnP),
    /// such as printers, cameras, media players and NAS boxes, and attaches them to the scanned addresses
    /// (<see cref="IPScanResult.MdnsServices"/> and <see cref="IPScanResult.SsdpDevices"/>).
    /// The default is <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// Discovery runs once per scan, next to the other checks, and adds up to <see cref="TimeoutDiscovery"/> to a short scan.
    /// </remarks>
    public bool DiscoverServices { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether each address is asked for its NetBIOS names and MAC address
    /// (<see cref="IPScanResult.NetBiosStatus"/>). Windows computers and Samba servers (NAS boxes) answer with their
    /// computer name and workgroup, even when DNS knows nothing about them. The default is <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// Addresses that don't answer wait up to <see cref="TimeoutNetBios"/>.
    /// </remarks>
    public bool ResolveNetBiosName { get; set; }

    /// <summary>
    /// Gets or sets how long to wait for a NetBIOS answer. The default is 1 second.
    /// </summary>
    public TimeSpan TimeoutNetBios { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutNetBiosInMs);

    /// <summary>
    /// Gets or sets how long service discovery collects answers. The default is 3 seconds.
    /// </summary>
    public TimeSpan TimeoutDiscovery { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutDiscoveryInMs);

    public void SetPortNumbers(
        IPServicePortExaminationLevel ipServicePortExaminationLevel)
    {
        var serviceProtocolTypes = Enum.GetValues<ServiceProtocolType>()
            .Where(x => x is not ServiceProtocolType.None and not ServiceProtocolType.Unknown)
            .ToArray();

        SetPortNumbers(ipServicePortExaminationLevel, serviceProtocolTypes);
    }

    public void SetPortNumbers(
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType serviceProtocolType)
    {
        SetPortNumbers(ipServicePortExaminationLevel, [serviceProtocolType]);
    }

    public void SetPortNumbers(
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType[] serviceProtocolTypes)
    {
        ArgumentNullException.ThrowIfNull(serviceProtocolTypes);

        PortNumbers.Clear();

        switch (ipServicePortExaminationLevel)
        {
            case IPServicePortExaminationLevel.None:
                break;
            case IPServicePortExaminationLevel.WellKnown:
                foreach (var serviceProtocolType in serviceProtocolTypes)
                {
                    foreach (var port in IPServicePortLists.GetWellKnown(serviceProtocolType))
                    {
                        PortNumbers.Add(port);
                    }
                }

                break;
            case IPServicePortExaminationLevel.WellKnownAndCommon:
                foreach (var serviceProtocolType in serviceProtocolTypes)
                {
                    foreach (var port in IPServicePortLists.GetWellKnownOrCommon(serviceProtocolType))
                    {
                        PortNumbers.Add(port);
                    }
                }

                break;
            case IPServicePortExaminationLevel.All:
                for (ushort i = 0; i < ushort.MaxValue; i++)
                {
                    PortNumbers.Add((ushort)(i + 1));
                }

                break;
            default:
                throw new SwitchCaseDefaultException(ipServicePortExaminationLevel);
        }
    }

    public override string ToString()
        => $"{nameof(Timeout)}: {Timeout}, {nameof(TimeoutPing)}: {TimeoutPing}, {nameof(TimeoutTcp)}: {TimeoutTcp}, {nameof(TimeoutHttp)}: {TimeoutHttp}, {nameof(IcmpPing)}: {IcmpPing}, {nameof(ResolveHostName)}: {ResolveHostName}, {nameof(ResolveMacAddress)}: {ResolveMacAddress}, {nameof(ResolveVendorFromMacAddress)}: {ResolveVendorFromMacAddress}, {nameof(PortNumbers)}: {PortNumbers.Count} ports, {nameof(TreatOpenPortsAsWebServices)}: {TreatOpenPortsAsWebServices}, {nameof(DiscoverServices)}: {DiscoverServices}, {nameof(TimeoutDiscovery)}: {TimeoutDiscovery}, {nameof(ResolveNetBiosName)}: {ResolveNetBiosName}, {nameof(TimeoutNetBios)}: {TimeoutNetBios}";
}