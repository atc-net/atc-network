namespace Atc.Network.Test.TestHelpers;

public sealed class CapturingLogger : ILogger
{
    private readonly ConcurrentQueue<CapturedLogEntry> entries = new();

    public IReadOnlyCollection<CapturedLogEntry> Entries => entries.ToArray();

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        entries.Enqueue(new CapturedLogEntry(logLevel, eventId, exception, formatter(state, exception)));
    }
}