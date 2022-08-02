// ReSharper disable InvertIf
namespace Atc.Network.Tcp;

/// <summary>
/// The main TcpClient - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
public partial class TcpClient : IDisposable
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private readonly TcpClientConfig clientConfig;
    private readonly TcpClientKeepAliveConfig keepAliveConfig;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly CancellationTokenRegistration cancellationTokenRegistration;

    private readonly string ipAddressOrHostname = string.Empty;
    private readonly int port;

    private readonly byte[] receiveBuffer;
    private readonly Task receiveListenerTask;

    private System.Net.Sockets.TcpClient? tcpClient;
    private Stream? networkStream;

    /// <summary>
    /// Is client connected
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Event to raise when connection is established.
    /// </summary>
    public event Action? Connected;

    /// <summary>
    /// Event to raise when connection is destroyed.
    /// </summary>
    public event Action? Disconnected;

    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    public event Action<byte[]>? DataReceived;

    private TcpClient(
        ILogger<TcpClient> logger,
        TcpClientConfig? clientConfig,
        TcpClientKeepAliveConfig? keepAliveConfig)
    {
        this.logger = logger;
        this.clientConfig = clientConfig ?? new TcpClientConfig();
        this.keepAliveConfig = keepAliveConfig ?? new TcpClientKeepAliveConfig();

        cancellationTokenSource = new CancellationTokenSource();

        receiveBuffer = new byte[this.clientConfig.ReceiveBufferSize];

        cancellationTokenRegistration = cancellationTokenSource.Token.Register(CancellationTokenCallback);

        this.receiveListenerTask = Task.Run(
            async () => await this.DataReceiver(
                cancellationTokenSource.Token),
            cancellationTokenSource.Token);
    }

    public TcpClient(
        ILogger<TcpClient> logger,
        string hostname,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(logger, clientConfig, keepAliveConfig)
    {
        if (string.IsNullOrEmpty(hostname))
        {
            throw new ArgumentNullException(nameof(hostname));
        }

        this.ipAddressOrHostname = hostname;
        this.port = port;
    }

    public TcpClient(
        ILogger<TcpClient> logger,
        IPAddress ipAddress,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(logger, clientConfig, keepAliveConfig)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        this.ipAddressOrHostname = ipAddress.ToString();
        this.port = port;
    }

    public TcpClient(
        ILogger<TcpClient> logger,
        IPEndPoint ipEndpoint,
        TcpClientConfig? clientConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(logger, clientConfig, keepAliveConfig)
    {
        ArgumentNullException.ThrowIfNull(ipEndpoint);

        this.ipAddressOrHostname = ipEndpoint.Address.ToString();
        this.port = ipEndpoint.Port;
    }

    /// <summary>
    /// Connect.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task<bool> Connect(
        CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            return false;
        }

        tcpClient = new System.Net.Sockets.TcpClient();
        SetBufferSizeAndTimeouts();

        LogConnecting(ipAddressOrHostname, port);

        try
        {
            var connectTimeoutTask = Task.Delay(clientConfig.ConnectTimeout, cancellationToken);
            var connectTask = tcpClient
                .ConnectAsync(ipAddressOrHostname, port, cancellationToken)
                .AsTask();

            // Double await so if cancelTask throws exception, this throws it
            await await Task.WhenAny(connectTask, connectTimeoutTask);

            if (connectTimeoutTask.IsCompleted)
            {
                // If cancelTask and connectTask both finish at the same time,
                // we'll consider it to be a timeout.
                throw new TcpException("Timed out");
            }
        }
        catch (Exception ex)
        {
            LogConnectionError(ipAddressOrHostname, port, ex.Message);
            return false;
        }

        LogConnected(ipAddressOrHostname, port);

        await SetConnected();
        PrepareNetworkStream();

        return true;
    }

    /// <summary>
    /// Disconnect.
    /// </summary>
    public async Task Disconnect()
    {
        LogDisconnecting(ipAddressOrHostname, port);

        DisposeTcpClientAndStream();
        await SetDisconnected();

        LogDisconnected(ipAddressOrHostname, port);
    }

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task Send(
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!IsConnected)
        {
            LogClientNotConnected(ipAddressOrHostname, port);
            throw new TcpException("Client is not connected!");
        }

        LogDataSendingByteLength(data.Length);

        await networkStream!.WriteAsync(data.AsMemory(0, data.Length), cancellationToken);
        await networkStream.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <remarks>
    /// Data will be encoded as ASCII.
    /// </remarks>
    public Task Send(
        string data,
        CancellationToken cancellationToken = default)
        => Send(Encoding.ASCII, data, cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="encoding">The encoding.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task Send(
        Encoding encoding,
        string data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentException("Data is empty.", nameof(data));
        }

        if (clientConfig.TerminationType != TcpTerminationType.None)
        {
            data += TcpTerminationHelper.GetTermination(clientConfig.TerminationType);
        }

        return Send(encoding.GetBytes(data), cancellationToken);
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

        if (!cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }

        cancellationTokenSource.Dispose();

        if (receiveListenerTask.Status == TaskStatus.Running)
        {
            receiveListenerTask.Wait(TimeSpan.FromMilliseconds(50));
        }

        cancellationTokenRegistration.Dispose();

        DisposeTcpClientAndStream();
    }

    private async Task SetConnected()
    {
        try
        {
            await SyncLock.WaitAsync();

            if (IsConnected)
            {
                return;
            }

            IsConnected = true;
            Connected?.Invoke();
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private async Task SetDisconnected()
    {
        try
        {
            await SyncLock.WaitAsync();

            if (!IsConnected)
            {
                return;
            }

            if (tcpClient is { Connected: true })
            {
                tcpClient.Close();
            }

            IsConnected = false;
            Disconnected?.Invoke();
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private void SetBufferSizeAndTimeouts()
    {
        tcpClient!.SetBufferSizeAndTimeouts(
            clientConfig.SendTimeout,
            clientConfig.ReceiveTimeout,
            clientConfig.SendBufferSize,
            clientConfig.ReceiveBufferSize);
    }

    private void PrepareNetworkStream()
    {
        networkStream = tcpClient!.GetStream();

        tcpClient.SetKeepAlive(
            keepAliveConfig.KeepAliveTime,
            keepAliveConfig.KeepAliveInterval,
            keepAliveConfig.KeepAliveRetryCount);
    }

    private async Task DataReceiver(
        CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsConnected ||
                networkStream is null)
            {
                continue;
            }

            await HandleReadDataTaskResponse(ReadData(cancellationToken));
        }
    }

    private async Task HandleReadDataTaskResponse(
        Task<byte[]> readDataTask)
    {
        if (readDataTask.IsCanceled)
        {
            LogDataReceiveTimeout();
        }

        if (readDataTask.IsFaulted)
        {
            if (readDataTask.Exception is not null)
            {
                var (isKnownException, socketError) = readDataTask.Exception.IsKnownException();
                if (isKnownException)
                {
                    LogDataReceiveError($"SocketErrorCode = {socketError?.GetDescription()}");
                }
            }
            else
            {
                LogDataReceiveError("Unknown error");
            }

            await SetDisconnected();
        }

        var data = await readDataTask;
        if (data.Length == 0)
        {
            if (IsConnected)
            {
                LogDataReceiveNoData();
                await SetDisconnected();
            }
        }
        else
        {
            LogDataReceivedByteLength(data.Length);

            DataReceived?.Invoke(data);
        }
    }

    private async Task<byte[]> ReadData(
        CancellationToken cancellationToken)
        {
            if (networkStream is null ||
                !networkStream.CanRead)
            {
                return Array.Empty<byte>();
            }

            try
            {
                var numberOfBytesToRead = await networkStream.ReadAsync(
                    receiveBuffer.AsMemory(0, receiveBuffer.Length),
                    cancellationToken);

                if (numberOfBytesToRead == 0)
                {
                    return Array.Empty<byte>();
                }

                using var memoryStream = new MemoryStream();
                await memoryStream.WriteAsync(
                    receiveBuffer.AsMemory(0, numberOfBytesToRead),
                    cancellationToken);

                return memoryStream.ToArray();
            }
            catch (Exception exception)
            {
                if (IsConnected)
                {
                    LogDataReceiveError(exception.Message);
                }
            }

            return Array.Empty<byte>();
        }

    private void DisposeTcpClientAndStream()
    {
        if (networkStream is not null)
        {
            if (networkStream.CanWrite ||
                networkStream.CanRead ||
                networkStream.CanSeek)
            {
                networkStream.Close();
            }

            networkStream.Dispose();
        }

        if (tcpClient is not null)
        {
            if (tcpClient.Connected)
            {
                tcpClient.Close();
            }

            tcpClient.Dispose();
        }
    }

    private void CancellationTokenCallback()
    {
        if (networkStream is null)
        {
            return;
        }

        this.networkStream.Close();
    }
}