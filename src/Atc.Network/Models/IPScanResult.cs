namespace Atc.Network.Models;

public class IPScanResult
{
    public IPScanResult(
        IPAddress iPAddress)
    {
        this.IPAddress = iPAddress;
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

    public bool IsCompleted
        => End.HasValue;

    public string? TimeDiff
        => End.HasValue
            ? Start.GetPrettyTimeDiff(End.Value)
            : null;

    public IEnumerable<int> OpenPort
        => Ports
            .Where(x => x.TransportProtocol != TransportProtocolType.None && x.CanConnect)
            .Select(x => x.Port)
            .OrderBy(x => x);

    public bool HasConnection
        => PingStatus?.Status != IPStatus.Success ||
           !string.IsNullOrEmpty(Hostname) ||
           !string.IsNullOrEmpty(MacAddress) ||
           OpenPort.Any();

    public override string ToString()
    {
        var sb = new StringBuilder();

        var totalPortCount = Ports.Count;
        if (totalPortCount > 0)
        {
            var openPorts = Ports.Count(x => x.TransportProtocol != TransportProtocolType.None && x.CanConnect);
            sb.Append(GlobalizationConstants.EnglishCultureInfo, $"OpenPorts {openPorts} of {totalPortCount} # ");
        }

        if (IsCompleted)
        {
            sb.Append(GlobalizationConstants.EnglishCultureInfo, $"TimeDiff={TimeDiff}");
        }
        else
        {
            sb.Append("Not completed");
        }

        return sb.ToString();
    }
}