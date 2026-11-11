namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// Collects the Atc.Network measurements and finished activities while it is alive.
/// Tests run in parallel, so every query filters on the component and port of the test's own client or server.
/// </summary>
public sealed class TelemetryCollector : IDisposable
{
    private readonly MeterListener meterListener = new();
    private readonly ActivityListener activityListener;
    private readonly ConcurrentQueue<CollectedMeasurement> measurements = new();
    private readonly ConcurrentQueue<Activity> activities = new();

    public TelemetryCollector()
    {
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == NetworkTelemetry.MeterName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        meterListener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            measurements.Enqueue(new CollectedMeasurement(instrument.Name, value, tags.ToArray())));
        meterListener.Start();

        activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == NetworkTelemetry.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => activities.Enqueue(activity),
        };
        ActivitySource.AddActivityListener(activityListener);
    }

    /// <summary>
    /// Returns the values recorded on the instrument for the component and port, optionally with one more tag.
    /// </summary>
    /// <param name="instrumentName">The instrument name.</param>
    /// <param name="component">The component tag value.</param>
    /// <param name="port">The server port tag value.</param>
    /// <param name="tagKey">An extra tag to match, or null.</param>
    /// <param name="tagValue">The extra tag's value.</param>
    /// <returns>The recorded values, in order.</returns>
    public IReadOnlyList<long> Values(
        string instrumentName,
        string component,
        int port,
        string? tagKey = null,
        string? tagValue = null)
        => measurements
            .Where(x => string.Equals(x.InstrumentName, instrumentName, StringComparison.Ordinal) &&
                        x.HasTag(NetworkTelemetry.ComponentTag, component) &&
                        x.HasTag(NetworkTelemetry.ServerPortTag, port) &&
                        (tagKey is null || x.HasTag(tagKey, tagValue)))
            .Select(x => x.Value)
            .ToList();

    /// <summary>
    /// Returns the sum of the values recorded on the instrument for the component and port.
    /// </summary>
    /// <param name="instrumentName">The instrument name.</param>
    /// <param name="component">The component tag value.</param>
    /// <param name="port">The server port tag value.</param>
    /// <param name="tagKey">An extra tag to match, or null.</param>
    /// <param name="tagValue">The extra tag's value.</param>
    /// <returns>The sum.</returns>
    public long Sum(
        string instrumentName,
        string component,
        int port,
        string? tagKey = null,
        string? tagValue = null)
        => Values(instrumentName, component, port, tagKey, tagValue).Sum();

    /// <summary>
    /// Returns the finished activities with the name, for the port.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="port">The server port tag value.</param>
    /// <returns>The activities.</returns>
    public IReadOnlyList<Activity> Activities(
        string name,
        int port)
        => activities
            .Where(x => string.Equals(x.OperationName, name, StringComparison.Ordinal) &&
                        Equals(x.GetTagItem(NetworkTelemetry.ServerPortTag), port))
            .ToList();

    public void Dispose()
    {
        meterListener.Dispose();
        activityListener.Dispose();
    }

    private sealed record CollectedMeasurement(
        string InstrumentName,
        long Value,
        KeyValuePair<string, object?>[] Tags)
    {
        public bool HasTag(
            string key,
            object? value)
            => Tags.Any(x => string.Equals(x.Key, key, StringComparison.Ordinal) && Equals(x.Value, value));
    }
}