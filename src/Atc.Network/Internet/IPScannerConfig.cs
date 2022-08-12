namespace Atc.Network.Internet;

public class IPScannerConfig
{
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
}