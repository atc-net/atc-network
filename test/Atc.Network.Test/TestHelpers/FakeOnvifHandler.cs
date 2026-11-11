namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// Answers ONVIF SOAP requests in tests, by the operation name (the "action" of the content type).
/// </summary>
internal sealed class FakeOnvifHandler : HttpMessageHandler
{
    private readonly Func<string, string, (HttpStatusCode Status, string Body)> respond;
    private readonly ConcurrentQueue<OnvifRequest> requests = new();

    /// <param name="respond">Returns the status and body for (operation, request body).</param>
    public FakeOnvifHandler(
        Func<string, string, (HttpStatusCode Status, string Body)> respond)
    {
        this.respond = respond;
    }

    public IReadOnlyList<OnvifRequest> Requests
        => requests.ToArray();

    public TimeSpan Delay { get; init; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);
        var action = request.Content?.Headers.ContentType?.Parameters
            .FirstOrDefault(x => string.Equals(x.Name, "action", StringComparison.OrdinalIgnoreCase))?.Value?.Trim('"') ?? string.Empty;
        var operation = action[(action.LastIndexOf('/') + 1)..];
        requests.Enqueue(new OnvifRequest(request.RequestUri ?? new Uri("http://localhost"), operation, body));

        if (Delay > TimeSpan.Zero)
        {
            await Task.Delay(Delay, cancellationToken);
        }

        var (status, responseBody) = respond(operation, body);
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, "application/soap+xml"),
        };
    }

    /// <summary>
    /// A request the fake received.
    /// </summary>
    /// <param name="Uri">Where it was sent.</param>
    /// <param name="Operation">The operation, such as "GetProfiles".</param>
    /// <param name="Body">The SOAP envelope.</param>
    internal sealed record OnvifRequest(
        Uri Uri,
        string Operation,
        string Body);
}