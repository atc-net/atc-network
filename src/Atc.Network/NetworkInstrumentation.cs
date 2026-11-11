namespace Atc.Network;

/// <summary>
/// Records the metrics and starts the activities of one client or server,
/// with its component, address and port as tags.
/// </summary>
internal sealed class NetworkInstrumentation
{
    private readonly KeyValuePair<string, object?>[] tags;
    private int isConnected;

    public NetworkInstrumentation(
        string component,
        string serverAddress,
        int serverPort)
    {
        tags =
        [
            new(NetworkTelemetry.ComponentTag, component),
            new(NetworkTelemetry.ServerAddressTag, serverAddress),
            new(NetworkTelemetry.ServerPortTag, serverPort),
        ];
    }

    /// <summary>
    /// Records a client that connected or disconnected. Only changes are counted,
    /// so calling it again with the same value is safe.
    /// </summary>
    /// <param name="connected">Whether the client is connected now.</param>
    public void SetConnected(bool connected)
    {
        var value = connected ? 1 : 0;
        if (Interlocked.Exchange(ref isConnected, value) != value)
        {
            NetworkTelemetry.ActiveConnections.Add(connected ? 1 : -1, tags);
        }
    }

    /// <summary>
    /// Records a client connection that a server accepted.
    /// </summary>
    public void ConnectionOpened()
        => NetworkTelemetry.ActiveConnections.Add(1, tags);

    /// <summary>
    /// Records a client connection that a server closed.
    /// </summary>
    public void ConnectionClosed()
        => NetworkTelemetry.ActiveConnections.Add(-1, tags);

    public void ConnectionFailed()
        => NetworkTelemetry.ConnectionFailures.Add(1, tags);

    public void Reconnected(bool success)
        => NetworkTelemetry.Reconnects.Add(1, WithTag(NetworkTelemetry.OutcomeTag, success ? "success" : "failure"));

    public void Sent(int byteCount)
    {
        NetworkTelemetry.BytesSent.Add(byteCount, tags);
        NetworkTelemetry.MessagesSent.Add(1, tags);
    }

    public void Received(int byteCount)
    {
        NetworkTelemetry.BytesReceived.Add(byteCount, tags);
        NetworkTelemetry.MessagesReceived.Add(1, tags);
    }

    public void HandlerFailed(string eventName)
        => NetworkTelemetry.HandlerErrors.Add(1, WithTag(NetworkTelemetry.EventTag, eventName));

    /// <summary>
    /// Starts an activity with this component's tags, or returns <see langword="null"/> when nobody listens.
    /// </summary>
    /// <param name="name">The activity name, such as "TcpClient.Send".</param>
    /// <param name="kind">The activity kind.</param>
    /// <returns>The started activity, or <see langword="null"/>.</returns>
    public Activity? StartActivity(
        string name,
        ActivityKind kind)
        => NetworkTelemetry.ActivitySource.StartActivity(name, kind, parentContext: default, tags);

    /// <summary>
    /// Starts a long-running background loop, such as a receive loop, without the current activity.
    /// Otherwise the loop's logs and activities would belong to the activity that started it,
    /// for example a connect activity that finished long ago.
    /// </summary>
    /// <param name="work">The loop to run.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The task running the loop.</returns>
    public static Task RunOutsideCurrentActivity(
        Func<Task> work,
        CancellationToken cancellationToken)
    {
        var current = Activity.Current;
        Activity.Current = null;
        try
        {
            return Task.Run(work, cancellationToken);
        }
        finally
        {
            Activity.Current = current;
        }
    }

    private TagList WithTag(
        string key,
        string value)
    {
        var tagList = new TagList(tags.AsSpan());
        tagList.Add(key, value);
        return tagList;
    }
}