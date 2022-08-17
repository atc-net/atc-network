// ReSharper disable MethodSupportsCancellation
namespace Atc.Network.Internet;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
public class IPPortScan : IIPPortScan
{
    private const int InternalDelayInMs = 5;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private readonly IPAddress ipAddress;
    private readonly int timeoutInMs;

    public IPPortScan(
        IPAddress ipAddress,
        int timeoutInMs = 100)
    {
        this.ipAddress = ipAddress;
        this.timeoutInMs = timeoutInMs;
    }

    public async Task<bool> CanConnectWithTcp(
        int portNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            await Task.Delay(InternalDelayInMs, cancellationToken);
            var cancellationCompletionSource = new TaskCompletionSource<bool>();

            using var cts = new CancellationTokenSource(timeoutInMs);
            using var client = new System.Net.Sockets.TcpClient();
            var task = client.ConnectAsync(ipAddress, portNumber);

            await using (cts.Token.Register(() => cancellationCompletionSource.TrySetResult(true)))
            {
                if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            SyncLock.Release();
        }
    }

    public Task<bool> CanConnectWithHttp(
        int portNumber = 80,
        CancellationToken cancellationToken = default)
        => CanConnectWithHttpOrHttps(portNumber, useHttps: false, cancellationToken);

    public Task<bool> CanConnectWithHttps(
        int portNumber = 443,
        CancellationToken cancellationToken = default)
        => CanConnectWithHttpOrHttps(portNumber, useHttps: true, cancellationToken);

    public async Task<bool> CanConnectWithHttpOrHttps(
        int portNumber = 80,
        bool useHttps = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            await Task.Delay(InternalDelayInMs, cancellationToken);
            var cancellationCompletionSource = new TaskCompletionSource<bool>();
            var uri = useHttps
                ? new Uri($"https://{ipAddress}:{portNumber}")
                : new Uri($"http://{ipAddress}:{portNumber}");

            using var cts = new CancellationTokenSource(timeoutInMs);
            using var client = new HttpClient();
            var task = client.GetAsync(uri, cancellationToken);

            await using (cts.Token.Register(() => cancellationCompletionSource.TrySetResult(true)))
            {
                if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                {
                    return false;
                }
            }

            var httpResponseMessage = await task;
            return httpResponseMessage.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
        finally
        {
            SyncLock.Release();
        }
    }
}