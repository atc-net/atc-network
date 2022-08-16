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

    public void SetPortNumbers(
        IPServicePortExaminationLevel ipServicePortExaminationLevel)
    {
        var serviceProtocolTypes = ((ServiceProtocolType[])Enum.GetValues(typeof(ServiceProtocolType)))
            .Where(x => x != ServiceProtocolType.None && x != ServiceProtocolType.Unknown)
            .ToArray();

        SetPortNumbers(ipServicePortExaminationLevel, serviceProtocolTypes);
    }

    public void SetPortNumbers(
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType serviceProtocolType)
    {
        SetPortNumbers(ipServicePortExaminationLevel, new[] { serviceProtocolType });
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
        => $"{nameof(IcmpPing)}: {IcmpPing}, {nameof(ResolveHostName)}: {ResolveHostName}, {nameof(ResolveMacAddress)}: {ResolveMacAddress}, {nameof(ResolveVendorFromMacAddress)}: {ResolveVendorFromMacAddress}, {nameof(PortNumbers)}: {PortNumbers}, {nameof(TreatOpenPortsAsWebServices)}: {TreatOpenPortsAsWebServices}";
}