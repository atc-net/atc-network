namespace Atc.Network.Test.TestHelpers;

public sealed record CapturedLogEntry(
    LogLevel Level,
    EventId EventId,
    Exception? Exception,
    string Message);