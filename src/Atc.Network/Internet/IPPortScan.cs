// ReSharper disable InvertIf
// ReSharper disable MethodSupportsCancellation
namespace Atc.Network.Internet;

/// <summary>
/// Provides functionality for scanning IP ports to check for TCP, HTTP, and HTTPS connectivity.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
public class IPPortScan : IIPPortScan, IDisposable
{
    private const int InternalDelayInMs = 5;
    private const int SyncLockTimeoutInMs = 30_000;

    private readonly SemaphoreSlim syncLock = new(1, 1);
    private IPAddress? ipAddress;
    private int timeoutInMs = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="IPPortScan"/> class.
    /// </summary>
    public IPPortScan()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IPPortScan"/> class with specified IP address and timeout.
    /// </summary>
    /// <param name="ipAddress">The IP address to scan.</param>
    /// <param name="timeoutInMs">The timeout in milliseconds for each connection attempt.</param>
    public IPPortScan(
        IPAddress ipAddress,
        int timeoutInMs = 100)
    {
        this.ipAddress = ipAddress;
        this.timeoutInMs = timeoutInMs;
    }

    /// <summary>
    /// Sets the IP address to scan.
    /// </summary>
    /// <param name="value">The IP address.</param>
    public void SetIPAddress(IPAddress value)
        => ipAddress = value;

    /// <summary>
    /// Sets the timeout for connection attempts.
    /// </summary>
    /// <param name="value">The timeout as a <see cref="TimeSpan"/>.</param>
    public void SetTimeout(TimeSpan value)
        => timeoutInMs = (int)value.TotalMilliseconds;

    /// <summary>
    /// Attempts to connect to a specified port using TCP.
    /// </summary>
    /// <param name="portNumber">The port number to attempt connection.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in a boolean indicating connection success.
    /// </returns>
    public async Task<bool> CanConnectWithTcp(
        int portNumber,
        CancellationToken cancellationToken = default)
    {
        if (ipAddress is null)
        {
            throw new TcpException("IPAddress is not set.");
        }

        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(SyncLockTimeoutInMs, cancellationToken);

            if (!lockAcquired)
            {
                return false;
            }

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
            if (lockAcquired)
            {
                syncLock.Release();
            }
        }
    }

    /// <summary>
    /// Attempts to connect to a specified port using HTTP.
    /// </summary>
    /// <param name="portNumber">The port number to attempt connection, default is 80.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in a boolean indicating connection success.
    /// </returns>
    public Task<bool> CanConnectWithHttp(
        int portNumber = 80,
        CancellationToken cancellationToken = default)
        => CanConnectWithHttpOrHttps(portNumber, useHttps: false, cancellationToken);

    /// <summary>
    /// Attempts to connect to a specified port using HTTPS.
    /// </summary>
    /// <param name="portNumber">The port number to attempt connection, default is 443.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in a boolean indicating connection success.
    /// </returns>
    public Task<bool> CanConnectWithHttps(
        int portNumber = 443,
        CancellationToken cancellationToken = default)
        => CanConnectWithHttpOrHttps(portNumber, useHttps: true, cancellationToken);

    /// <summary>
    /// Attempts to connect to a specified port using HTTP or HTTPS.
    /// </summary>
    /// <param name="portNumber">The port number to attempt connection, default is 80 for HTTP and 443 for HTTPS.</param>
    /// <param name="useHttps">Specifies whether to use HTTPS (true) or HTTP (false).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in a boolean indicating connection success.
    /// </returns>
    [SuppressMessage("Security", "S5332:Use secure protocols", Justification = "OK - the scan deliberately probes plain HTTP ports.")]
    [SuppressMessage("Security", "S4830:Server certificates should be verified during SSL/TLS connections", Justification = "OK - only reachability is checked and no data is sent.")]
    [SuppressMessage("Security", "MA0039:Do not write your own certificate validation method", Justification = "OK - only reachability is checked and no data is sent.")]
    public async Task<bool> CanConnectWithHttpOrHttps(
        int portNumber = 80,
        bool useHttps = false,
        CancellationToken cancellationToken = default)
    {
        if (ipAddress is null)
        {
            throw new TcpException("IPAddress is not set.");
        }

        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(SyncLockTimeoutInMs, cancellationToken);

            if (!lockAcquired)
            {
                return false;
            }

            await Task.Delay(InternalDelayInMs, cancellationToken);
            var uri = useHttps
                ? new Uri($"https://{ipAddress}:{portNumber}")
                : new Uri($"http://{ipAddress}:{portNumber}");

            // The request is cancelled when the timeout passes or the caller cancels.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeoutInMs);

            using var handler = new HttpClientHandler
            {
                // Only reachability is checked and no data is sent, so devices with
                // self-signed or otherwise untrusted certificates count as reachable.
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };

            using var client = new HttpClient(handler);
            client.Timeout = Timeout.InfiniteTimeSpan;

            // Any HTTP response, also 401, 404 or 500, means an HTTP server answered.
            using var httpResponseMessage = await client.GetAsync(
                uri,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (lockAcquired)
            {
                syncLock.Release();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    /// <param name="disposing">Indicates if we are disposing or not.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        syncLock.Dispose();
    }
}