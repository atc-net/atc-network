namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// Answers UDP requests on loopback with the datagrams that <c>respond</c> returns for each request,
/// in place of a multicast responder (SSDP device or mDNS responder) on the network.
/// </summary>
public sealed class FakeResponder : IDisposable
{
    private readonly System.Net.Sockets.UdpClient socket = new(new IPEndPoint(IPAddress.Loopback, 0));
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly ConcurrentQueue<byte[]> requests = new();
    private readonly Task serveTask;

    public FakeResponder(Func<byte[], IReadOnlyList<byte[]>> respond)
    {
        ArgumentNullException.ThrowIfNull(respond);

        EndPoint = (IPEndPoint)socket.Client.LocalEndPoint;
        serveTask = Serve(respond);
    }

    public IPEndPoint EndPoint { get; }

    /// <summary>
    /// Gets the requests received, in order.
    /// </summary>
    public IReadOnlyList<byte[]> Requests
        => requests.ToArray();

    /// <summary>
    /// Gets the first request as ASCII text, or an empty string.
    /// </summary>
    public string ReceivedRequest
        => requests.TryPeek(out var request)
            ? Encoding.ASCII.GetString(request)
            : string.Empty;

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        socket.Dispose();
        cancellationTokenSource.Dispose();
        _ = serveTask.Exception;
    }

    private async Task Serve(Func<byte[], IReadOnlyList<byte[]>> respond)
    {
        try
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var request = await socket.ReceiveAsync(cancellationTokenSource.Token);
                requests.Enqueue(request.Buffer);

                foreach (var reply in respond(request.Buffer))
                {
                    await socket.SendAsync(reply, request.RemoteEndPoint, cancellationTokenSource.Token);
                }
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or System.Net.Sockets.SocketException)
        {
            // Disposed.
        }
    }
}