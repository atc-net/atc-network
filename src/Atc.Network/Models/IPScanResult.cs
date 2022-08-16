namespace Atc.Network.Models;

public class IPScanResult
{
    public IPScanResult(
        IPAddress ipAddress)
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
           OpenPortNumbers.Any();

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(IPAddress);
        sb.Append(" # ");

        if (HasConnection)
        {
            if (!string.IsNullOrEmpty(Hostname))
            {
                sb.Append(Hostname);
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