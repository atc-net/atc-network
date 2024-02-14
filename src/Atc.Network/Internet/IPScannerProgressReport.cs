namespace Atc.Network.Internet;

/// <summary>
/// Represents a progress report for an ongoing IP scanning operation.
/// </summary>
public class IPScannerProgressReport
{
    /// <summary>
    /// Gets or sets the total number of tasks to process during the scan.
    /// </summary>
    /// <value>The total number of scanning tasks.</value>
    public int TasksToProcessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of tasks that have been processed so far.
    /// </summary>
    /// <value>The number of completed scanning tasks.</value>
    public int TasksProcessedCount { get; set; }

    /// <summary>
    /// Gets or sets the type of the progress reporting event.
    /// </summary>
    /// <value>The type of event being reported.</value>
    public IPScannerProgressReportingType Type { get; set; }

    /// <summary>
    /// Gets or sets the latest update or result from the scanning operation.
    /// </summary>
    /// <value>The latest individual scan result.</value>
    public IPScanResult? LatestUpdate { get; set; }

    /// <summary>
    /// Calculates and returns the percentage of tasks completed.
    /// </summary>
    /// <value>The percentage of scanning tasks that have been completed.</value>
    public double PercentageCompleted
        => MathHelper.Percentage(TasksToProcessCount, TasksProcessedCount);

    /// <summary>
    /// Provides a string representation of the current progress report.
    /// </summary>
    /// <returns>
    /// A string that represents the current state of the progress report,
    /// including details about the latest update, the type of event, and the current progress.
    /// </returns>
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