namespace Atc.Network.Internet;

public class IPScannerConfig
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutInMs);

    public TimeSpan TimeoutPing { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutPingInMs);

    public TimeSpan TimeoutTcp { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutTcpInMs);

    public TimeSpan TimeoutHttp { get; set; } = TimeSpan.FromMilliseconds(IPScannerConstants.TimeoutHttpInMs);

    public bool ResolvePing { get; set; } = true;

    public bool ResolveHostName { get; set; } = true;

    public bool ResolveMacAddress { get; set; } = true;

    public bool ResolveVendorFromMacAddress { get; set; } = true;

    public ICollection<int> PortNumbers { get; set; } = new List<int>();

    public bool ResolveOnlyKnowTcpUdpPorts { get; set; } = true;

    public bool ResolveServiceProtocolHttp { get; set; } = true;
}