namespace Atc.Network.Internet;

public class IPScannerProgressReport
{
    public int TasksToProcessCount { get; set; }

    public int TasksProcessedCount { get; set; }

    public IPScannerProgressReportingType Type { get; set; }

    public IPScanResult? LatestUpdate { get; set; }

    public double PercentageCompleted
        => MathHelper.Percentage(TasksToProcessCount, TasksProcessedCount);

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (LatestUpdate is not null)
        {
            sb.Append(GlobalizationConstants.EnglishCultureInfo, $"{LatestUpdate.IPAddress} # ");
        }

        sb.Append(GlobalizationConstants.EnglishCultureInfo, $"Event {Type} # ");
        sb.Append(GlobalizationConstants.EnglishCultureInfo, $"Progress {TasksProcessedCount} of {TasksToProcessCount} - {PercentageCompleted}%");

        if (LatestUpdate is not null)
        {
            sb.Append(GlobalizationConstants.EnglishCultureInfo, $" # {LatestUpdate}");
        }

        return sb.ToString();
    }
}