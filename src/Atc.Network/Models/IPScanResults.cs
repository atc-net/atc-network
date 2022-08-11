namespace Atc.Network.Models;

public class IPScanResults
{
    public DateTime Start { get; } = DateTime.Now;

    public DateTime? End { get; set; }

    public ConcurrentBag<IPScanResult> CollectedResults { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public bool IsCompleted
        => End.HasValue &&
           CollectedResults.All(x => x.IsCompleted);

    public string? TimeDiff
        => End.HasValue
            ? Start.GetPrettyTimeDiff(End.Value)
            : null;

    public override string ToString()
    {
        var openPorts = CollectedResults.Sum(ipScanResult => ipScanResult.OpenPort.Count());
        var totalPorts = CollectedResults.Sum(ipScanResult => ipScanResult.Ports.Count);
        var completedPart = IsCompleted
            ? $"Time={TimeDiff}"
            : "Not completed";

        return $"OpenPorts {openPorts} of {totalPorts} # {completedPart}";
    }
}