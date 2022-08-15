namespace Atc.Network.Internet;

public class IPScannerConfig
{
    public IPScannerConfig()
    {
    }

    public IPScannerConfig(
        IPServicePortExaminationLevel ipServicePortExaminationLevel)
    {
        switch (ipServicePortExaminationLevel)
        {
            case IPServicePortExaminationLevel.None:
                break;
            case IPServicePortExaminationLevel.WellKnown:
                foreach (var port in IPServicePortLists.GetWellKnown())
                {
                    PortNumbers.Add(port);
                }

                break;
            case IPServicePortExaminationLevel.WellKnownAndCommon:
                foreach (var port in IPServicePortLists.GetWellKnownOrCommon())
                {
                    PortNumbers.Add(port);
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

    public IPScannerConfig(
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType serviceProtocolType)
    {
        switch (ipServicePortExaminationLevel)
        {
            case IPServicePortExaminationLevel.None:
                break;
            case IPServicePortExaminationLevel.WellKnown:
                foreach (var port in IPServicePortLists.GetWellKnown(serviceProtocolType))
                {
                    PortNumbers.Add(port);
                }

                break;
            case IPServicePortExaminationLevel.WellKnownAndCommon:
                foreach (var port in IPServicePortLists.GetWellKnownOrCommon(serviceProtocolType))
                {
                    PortNumbers.Add(port);
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

    public override string ToString()
        => $"{nameof(IcmpPing)}: {IcmpPing}, {nameof(ResolveHostName)}: {ResolveHostName}, {nameof(ResolveMacAddress)}: {ResolveMacAddress}, {nameof(ResolveVendorFromMacAddress)}: {ResolveVendorFromMacAddress}, {nameof(PortNumbers)}: {PortNumbers}, {nameof(TreatOpenPortsAsWebServices)}: {TreatOpenPortsAsWebServices}";
}