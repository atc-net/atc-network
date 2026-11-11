namespace Atc.Network.Console.Scanner;

/// <summary>
/// Keeps the scanner's warnings and errors, to show them after the scan instead of in the middle of the progress bar.
/// </summary>
public sealed class CollectingLogger : ILogger
{
    private readonly ConcurrentQueue<string> messages = new();

    /// <summary>
    /// Gets the warnings and errors, in order.
    /// </summary>
    public IReadOnlyCollection<string> Messages
        => messages.ToArray();

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= LogLevel.Warning;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        if (IsEnabled(logLevel))
        {
            messages.Enqueue($"{logLevel}: {formatter(state, exception)}");
        }
    }
}