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
    public void SetIPAddress(
        IPAddress value)
        => ipAddress = value;

    /// <summary>
    /// Sets the timeout for connection attempts.
    /// </summary>
    /// <param name="value">The timeout as a <see cref="TimeSpan"/>.</param>
    public void SetTimeout(
        TimeSpan value)
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
        Dispose(disposing: true);
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