namespace Atc.Network.Internet;

public class IPScannerProgressReport
{
    public int TasksToProcessCount { get; set; }

    public int TasksProcessedCount { get; set; }

    public IPScanResult? LatestUpdate { get; set; }

    public double PercentageCompleted
        => MathHelper.Percentage(TasksToProcessCount, TasksProcessedCount);

    public override string ToString()
        => $"{nameof(PercentageCompleted)}: {PercentageCompleted}, {nameof(TasksToProcessCount)}: {TasksToProcessCount}, {nameof(TasksProcessedCount)}: {TasksProcessedCount}, {nameof(LatestUpdate)}: ({LatestUpdate})";
}