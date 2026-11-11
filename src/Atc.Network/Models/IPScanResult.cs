namespace Atc.Network.Models;

public class IPScanResult
{
    public IPScanResult(IPAddress ipAddress)
    {
        this.IPAddress = ipAddress;
    }

    public IPAddress IPAddress { get; }

    public DateTime Start { get; } = DateTime.Now;

    public DateTime? End { get; set; }

    public string? ErrorMessage { get; set; }

    public PingStatusResult? PingStatus { get; set; }

    public string? Hostname { get; set; }

    public string? MacAddress { get; set; }

    public string? MacVendor { get; set; }

    public ConcurrentBag<IPScanPortResult> Ports { get; set; } = new();

    /// <summary>
    /// Gets or sets the mDNS / DNS-SD services found on this address, when <see cref="IPScannerConfig.DiscoverServices"/> is on.
    /// </summary>
    public IList<MdnsServiceInstance> MdnsServices { get; set; } = new List<MdnsServiceInstance>();

    /// <summary>
    /// Gets or sets the SSDP (UPnP) devices and services found on this address, when <see cref="IPScannerConfig.DiscoverServices"/> is on.
    /// </summary>
    public IList<SsdpDevice> SsdpDevices { get; set; } = new List<SsdpDevice>();

    /// <summary>
    /// Gets or sets the NetBIOS names and MAC address the address reported, when <see cref="IPScannerConfig.ResolveNetBiosName"/>
    /// is on and it answered; otherwise <see langword="null"/>.
    /// </summary>
    public NetBiosNodeStatus? NetBiosStatus { get; set; }

    /// <summary>
    /// Gets or sets the ONVIF devices (cameras, NVRs) found on this address, when <see cref="IPScannerConfig.DiscoverOnvif"/> is on.
    /// </summary>
    public IList<OnvifDevice> OnvifDevices { get; set; } = new List<OnvifDevice>();

    /// <summary>
    /// Gets the host name the address announces with mDNS, such as "printer.local", or <see langword="null"/>.
    /// It often names devices that reverse DNS (<see cref="Hostname"/>) does not know.
    /// </summary>
    public string? MdnsHostName
        => MdnsServices
            .Select(x => x.HostName)
            .FirstOrDefault(x => !string.IsNullOrEmpty(x));

    public bool IsCompleted
        => End.HasValue;

    public string? TimeDiff
        => End.HasValue
            ? Start.GetPrettyTimeDiff(End.Value)
            : null;

    public IEnumerable<ushort> OpenPortNumbers
        => Ports
            .Where(x => x.TransportProtocol != TransportProtocolType.None && x.CanConnect)
            .Select(x => x.Port)
            .OrderBy(x => x);

    public bool HasConnection
        => PingStatus?.Status == IPStatus.Success ||
           !string.IsNullOrEmpty(Hostname) ||
           !string.IsNullOrEmpty(MacAddress) ||
           OpenPortNumbers.Any() ||
           MdnsServices.Count > 0 ||
           SsdpDevices.Count > 0 ||
           OnvifDevices.Count > 0 ||
           NetBiosStatus is not null;

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(IPAddress);
        sb.Append(" # ");

        if (HasConnection)
        {
            var hostname = new[] { Hostname, MdnsHostName, NetBiosStatus?.ComputerName, OnvifDevices.FirstOrDefault()?.Name }
                .FirstOrDefault(x => !string.IsNullOrEmpty(x));
            if (!string.IsNullOrEmpty(hostname))
            {
                sb.Append(hostname);
                sb.Append(" # ");
            }

            if (MdnsServices.Count > 0 || SsdpDevices.Count > 0 || OnvifDevices.Count > 0)
            {
                sb.Append("Services ");
                sb.Append(MdnsServices.Count + SsdpDevices.Count + OnvifDevices.Count);
                sb.Append(" # ");
            }

            var totalPortCount = Ports.Count;
            if (totalPortCount > 0)
            {
                sb.Append("OpenPorts ");
                sb.Append(OpenPortNumbers.Count());
                sb.Append(" of ");
                sb.Append(totalPortCount);
                sb.Append(" # ");
            }
        }
        else
        {
            sb.Append("No connection # ");
        }

        if (IsCompleted)
        {
            sb.Append("Execution time ");
            sb.Append(TimeDiff);
        }
        else
        {
            sb.Append("Not completed");
        }

        return sb.ToString();
    }
}