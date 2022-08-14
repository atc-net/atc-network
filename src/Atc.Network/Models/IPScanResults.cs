namespace Atc.Network.Models;

public class IPScanResults
{
    public DateTime Start { get; } = DateTime.Now;

    public DateTime? End { get; set; }

    public ConcurrentBag<IPScanResult> CollectedResults { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public double PercentageCompleted { get; set; }

    public bool IsCompleted
        => End.HasValue &&
           CollectedResults.All(x => x.IsCompleted);

    public string? TimeDiff
        => End.HasValue
            ? Start.GetPrettyTimeDiff(End.Value)
            : null;

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append("Connected ");
        sb.Append(CollectedResults.Count(ipScanResult => ipScanResult.HasConnection));
        sb.Append(" of ");
        sb.Append(CollectedResults.Count);
        sb.Append(" # ");

        var totalPortCount = CollectedResults.Sum(ipScanResult => ipScanResult.Ports.Count);
        if (totalPortCount > 0)
        {
            sb.Append("OpenPorts ");
            sb.Append(CollectedResults.Sum(ipScanResult => ipScanResult.OpenPort.Count()));
            sb.Append(" of ");
            sb.Append(totalPortCount);
            sb.Append(" # ");
        }

        if (!IsCompleted)
        {
            sb.Append("Not completed - missing ");
            sb.Append(100 - PercentageCompleted);
            sb.Append("% # ");
        }

        sb.Append("Execution time ");
        sb.Append(TimeDiff);

        return sb.ToString();
    }
}