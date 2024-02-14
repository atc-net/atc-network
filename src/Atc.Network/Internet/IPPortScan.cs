// ReSharper disable InvertIf
// ReSharper disable MethodSupportsCancellation
namespace Atc.Network.Internet;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
public class IPPortScan : IIPPortScan, IDisposable
{
    private const int InternalDelayInMs = 5;
    private readonly SemaphoreSlim syncLock = new(1, 1);

    private IPAddress? ipAddress;
    private int timeoutInMs = 100;

    public IPPortScan()
    {
    }

    public IPPortScan(
        IPAddress ipAddress,
        int timeoutInMs = 100)
    {
        this.ipAddress = ipAddress;
        this.timeoutInMs = timeoutInMs;
    }

    public void SetIPAddress(
        IPAddress value)
        => this.ipAddress = value;

    public void SetTimeout(
        TimeSpan value)
        => this.timeoutInMs = (int)value.TotalMilliseconds;

    public async Task<bool> CanConnectWithTcp(
        int portNumber,
        CancellationToken cancellationToken = default)
    {
        if (ipAddress is null)
        {
            throw new TcpException("IPAddress is not set.");
        }

        try
        {
            await syncLock.WaitAsync(cancellationToken);

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

            if (client.Connected)
            {
                client.Close();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
        finally
        {
            syncLock.Release();
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
        if (ipAddress is null)
        {
            throw new TcpException("IPAddress is not set.");
        }

        try
        {
            await syncLock.WaitAsync(cancellationToken);

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
            syncLock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    /// <param name="disposing">Indicates if we are disposing or not.</param>
    protected virtual void Dispose(
        bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        syncLock.Dispose();
    }
}