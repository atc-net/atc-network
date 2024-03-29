// ReSharper disable CommentTypo
// ReSharper disable InvertIf
// ReSharper disable InconsistentNaming
// ReSharper disable LocalizableElement
namespace Atc.Network.Tcp;

/// <summary>
/// The main TcpClient - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:Element should not be on a single line", Justification = "OK.")]
public partial class TcpClient : ITcpClient
{
    private const int TimeToWaitForDisconnectionInMs = 200;
    private const int TimeToWaitForDisposeDisconnectionInMs = 50;
    private const int TimeToWaitForDataReceiverInMs = 150;

    private readonly SemaphoreSlim syncLock = new(1, 1);
    private readonly TcpClientConfig clientConfig;
    private readonly TcpClientReconnectConfig clientReconnectConfig;
    private readonly TcpClientKeepAliveConfig clientKeepAliveConfig;
    private readonly byte[] receiveBuffer;

    private readonly int syncLockConnectTimeoutInMs;
    private readonly int syncLockSendTimeoutInMs;
    private int reconnectRetryCounter;
    private CancellationTokenSource? cancellationTokenSource;
    private CancellationTokenRegistration? cancellationTokenRegistration;
    private Task? receiveListenerTask;
    private System.Net.Sockets.TcpClient? tcpClient;
    private Stream? networkStream;

    /// <summary>
    /// Event to raise when connection is established.
    /// </summary>
    public event Action? Connected;

    /// <summary>
    /// Event to raise when connection is destroyed.
    /// </summary>
    public event Action? Disconnected;

    /// <summary>
    /// Event to raise when connection state is changed.
    /// </summary>
    public event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// Event to raise when no data received.
    /// </summary>
    public event Action? NoDataReceived;

    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    public event Action<byte[]>? DataReceived;

    private TcpClient(
        ILogger logger,
        TcpClientConfig? clientConfig,
        TcpClientReconnectConfig? reconnectConfig,
        TcpClientKeepAliveConfig? keepAliveConfig)
    {
        this.logger = logger;
        this.clientConfig = clientConfig ?? new TcpClientConfig();
        clientReconnectConfig = reconnectConfig ?? new TcpClientReconnectConfig();
        clientKeepAliveConfig = keepAliveConfig ?? new TcpClientKeepAliveConfig();

        receiveBuffer = new byte[this.clientConfig.ReceiveBufferSize];

        syncLockConnectTimeoutInMs = this.clientConfig.ConnectTimeout <= 0
                ? TcpConstants.DefaultConnectTimeout
                : this.clientConfig.ConnectTimeout + TcpConstants.GracePeriodTimeout;

        syncLockSendTimeoutInMs = this.clientConfig.SendTimeout <= 0
            ? TcpConstants.DefaultSendReceiveTimeout
            : this.clientConfig.SendTimeout + TcpConstants.GracePeriodTimeout;
    }

    public TcpClient(
        ILogger logger,
        string hostname,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientReconnectConfig? reconnectConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(logger, clientConfig, reconnectConfig, keepAliveConfig)
    {
        if (string.IsNullOrEmpty(hostname))
        {
            throw new ArgumentNullException(nameof(hostname));
        }

        IPAddressOrHostname = hostname;
        this.Port = port;
    }

    public TcpClient(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientReconnectConfig? reconnectConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(logger, clientConfig, reconnectConfig, keepAliveConfig)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        IPAddressOrHostname = ipAddress.ToString();
        this.Port = port;
    }

    public TcpClient(
        ILogger logger,
        IPEndPoint ipEndpoint,
        TcpClientConfig? clientConfig = default,
        TcpClientReconnectConfig? reconnectConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(logger, clientConfig, reconnectConfig, keepAliveConfig)
    {
        ArgumentNullException.ThrowIfNull(ipEndpoint);

        IPAddressOrHostname = ipEndpoint.Address.ToString();
        Port = ipEndpoint.Port;
    }

    public TcpClient(
        ILogger logger,
        IPEndPoint ipEndpoint,
        TerminationType terminationType)
        : this(
            logger,
            ipEndpoint,
            new TcpClientConfig
            {
                TerminationType = terminationType,
            },
            new TcpClientReconnectConfig(),
            new TcpClientKeepAliveConfig())
    {
    }

    public TcpClient(
        string hostname,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientReconnectConfig? reconnectConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(NullLogger.Instance, hostname, port, clientConfig, reconnectConfig, keepAliveConfig)
    {
    }

    public TcpClient(
        IPAddress ipAddress,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientReconnectConfig? reconnectConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(NullLogger.Instance, ipAddress, port, clientConfig, reconnectConfig, keepAliveConfig)
    {
    }

    public TcpClient(
        IPEndPoint ipEndpoint,
        TcpClientConfig? clientConfig = default,
        TcpClientReconnectConfig? reconnectConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(NullLogger.Instance, ipEndpoint, clientConfig, reconnectConfig, keepAliveConfig)
    {
    }

    public TcpClient(
        IPEndPoint ipEndpoint,
        TerminationType terminationType)
        : this(
            NullLogger.Instance,
            ipEndpoint,
            new TcpClientConfig
            {
                TerminationType = terminationType,
            },
            new TcpClientReconnectConfig(),
            new TcpClientKeepAliveConfig())
    {
    }

    /// <summary>
    /// IPAddress or hostname for server connection.
    /// </summary>
    public string IPAddressOrHostname { get; } = string.Empty;

    /// <summary>
    /// Port number for server connection.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Is client connected.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Connect.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task<bool> Connect(
        CancellationToken cancellationToken = default)
        => DoConnect(raiseEventsAndLog: true, cancellationToken);

    /// <summary>
    /// Disconnect.
    /// </summary>
    public Task Disconnect()
    {
        var dispose = !clientReconnectConfig.Enable;
        return DoDisconnect(raiseEventsAndLog: true, dispose: dispose);
    }

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <remarks>
    /// Data will be encoded as client-config default encoding.
    /// </remarks>
    public Task Send(
        string data,
        CancellationToken cancellationToken = default)
        => Send(clientConfig.DefaultEncoding, data, cancellationToken);

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

        return Send(
            encoding.GetBytes(data),
            clientConfig.TerminationType,
            cancellationToken);
    }

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <remarks>
    /// TerminationType is resolved from TcpClientConfig.
    /// </remarks>
    public Task Send(
        byte[] data,
        CancellationToken cancellationToken = default)
        => Send(data, clientConfig.TerminationType, cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="terminationType">The terminationType.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task Send(
        byte[] data,
        TerminationType terminationType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!IsConnected)
        {
            LogClientNotConnected(IPAddressOrHostname, Port);
            throw new TcpException("Client is not connected!");
        }

        TerminationHelper.AppendTerminationBytesIfNeeded(ref data, terminationType);

        LogDataSendingByteLength(IPAddressOrHostname, Port, data.Length);

        await syncLock.WaitAsync(syncLockSendTimeoutInMs, cancellationToken);

        var disconnectedDueToException = false;

        try
        {
            await networkStream!.WriteAsync(data.AsMemory(0, data.Length), cancellationToken);
            await networkStream.FlushAsync(cancellationToken);
        }
        catch (SocketException ex)
        {
            LogDataSendingSocketError(IPAddressOrHostname, Port, ex.SocketErrorCode.ToString(), ex.Message);
            disconnectedDueToException = true;
        }
        catch (Exception ex)
        {
            LogDataSendingError(IPAddressOrHostname, Port, ex.Message);
            disconnectedDueToException = true;
        }
        finally
        {
            syncLock.Release();
        }

        if (disconnectedDueToException)
        {
            await DoDisconnect(raiseEventsAndLog: true, dispose: true);
        }
    }

    /// <summary>
    /// Called when connection is established.
    /// </summary>
    protected virtual void OnConnected() { }

    /// <summary>
    /// Called when connection is destroyed.
    /// </summary>
    protected virtual void OnDisconnected() { }

    /// <summary>
    /// Called when connection state is changed.
    /// </summary>
    /// <param name="connectionState">The connection state.</param>
    /// <param name="errorMessage">The error message.</param>
    protected virtual void OnConnectionStateChanged(
        ConnectionState connectionState,
        string? errorMessage = null) { }

    /// <summary>
    /// Called when no data received.
    /// </summary>
    protected virtual void OnNoDataReceived() { }

    /// <summary>
    /// Called when data received.
    /// </summary>
    /// <param name="bytes">The received bytes.</param>
    protected virtual void OnDataReceived(
        byte[] bytes) { }

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

        DisposeCancellationTokenAndTask();
        DisposeTcpClientAndStream();

        syncLock.Dispose();
    }

    private void InvokeConnected()
    {
        Connected?.Invoke();
        OnConnected();
    }

    private void InvokeDisconnected()
    {
        Disconnected?.Invoke();
        OnDisconnected();
    }

    private void InvokeConnectionStateChanged(
        ConnectionState connectionState,
        string? errorMessage = null)
    {
        if (errorMessage is null)
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(connectionState));
            OnConnectionStateChanged(connectionState);
        }
        else
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(connectionState, errorMessage));
            OnConnectionStateChanged(connectionState, errorMessage);
        }
    }

    private void InvokeNoDataReceived()
    {
        NoDataReceived?.Invoke();
        OnNoDataReceived();
    }

    private void InvokeDataReceived(
        byte[] data)
    {
        DataReceived?.Invoke(data);
        OnDataReceived(data);
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private async Task<bool> DoConnect(
        bool raiseEventsAndLog,
        CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            return false;
        }

        if (raiseEventsAndLog)
        {
            LogConnecting(IPAddressOrHostname, Port);
            InvokeConnectionStateChanged(ConnectionState.Connecting);
        }

        CleanupIfNeededInDoConnect();

        CreateNewTcpClient();

        try
        {
            var connectTimeoutTask = Task.Delay(clientConfig.ConnectTimeout, cancellationToken);
            var connectTask = tcpClient!
                .ConnectAsync(IPAddressOrHostname, Port, cancellationToken)
                .AsTask();

            // Double await so if connectTimeoutTask throws exception, this throws it
            await await Task.WhenAny(connectTask, connectTimeoutTask);

            if (connectTimeoutTask.IsCompleted)
            {
                // If connectTimeoutTask and connectTask both finish at the same time,
                // we'll consider it to be a timeout.
                throw new TcpException("Timed out");
            }
        }
        catch (Exception ex)
        {
            if (raiseEventsAndLog)
            {
                LogConnectionError(IPAddressOrHostname, Port, ex.Message);
                InvokeConnectionStateChanged(ConnectionState.ConnectionFailed, ex.Message);
            }

            if (tcpClient is not null)
            {
                tcpClient!.Close();
                tcpClient.Dispose();
                tcpClient = null;
            }

            return false;
        }

        await SetConnected(raiseEventsAndLog, cancellationToken);
        PrepareNetworkStreamAndKeepAliveInDoConnect();

        if (raiseEventsAndLog)
        {
            LogConnected(IPAddressOrHostname, Port);
            InvokeConnectionStateChanged(ConnectionState.Connected);
        }

        return true;
    }

    private Task DoDisconnect(
        bool raiseEventsAndLog,
        bool dispose)
    {
        if (raiseEventsAndLog)
        {
            LogDisconnecting(IPAddressOrHostname, Port);
        }

        return SetDisconnected(raiseEvents: raiseEventsAndLog, dispose: dispose);
    }

    private async Task DoReconnect()
    {
        LogReconnecting(IPAddressOrHostname, Port);
        InvokeConnectionStateChanged(ConnectionState.Reconnecting);

        await SetDisconnected(raiseEvents: false, dispose: false);

        await Task.Delay(clientReconnectConfig.RetryInterval);

        if (await DoConnect(raiseEventsAndLog: false, CancellationToken.None))
        {
            reconnectRetryCounter = 0;
            LogReconnected(IPAddressOrHostname, Port);
            InvokeConnectionStateChanged(ConnectionState.Reconnected);
        }
        else
        {
            if (reconnectRetryCounter < clientReconnectConfig.RetryMaxAttempts)
            {
                LogReconnectionWarning(IPAddressOrHostname, Port, reconnectRetryCounter, clientReconnectConfig.RetryMaxAttempts);
                InvokeConnectionStateChanged(ConnectionState.ReconnectionFailed);

                reconnectRetryCounter++;

                // ReSharper disable once TailRecursiveCall
                await DoReconnect();
            }
            else
            {
                LogReconnectionMaxRetryExceededError(IPAddressOrHostname, Port);
                InvokeConnectionStateChanged(ConnectionState.ReconnectionFailed);
            }
        }
    }

    private async Task SetConnected(
        bool raiseEvents,
        CancellationToken cancellationToken = default)
    {
        await syncLock.WaitAsync(syncLockConnectTimeoutInMs, cancellationToken);

        try
        {
            if (IsConnected)
            {
                return;
            }

            IsConnected = true;
            if (raiseEvents)
            {
                InvokeConnected();
            }
        }
        finally
        {
            syncLock.Release();
        }
    }

    private async Task SetDisconnected(
        bool raiseEvents,
        bool dispose,
        CancellationToken cancellationToken = default)
    {
        await syncLock.WaitAsync(syncLockConnectTimeoutInMs, cancellationToken);

        try
        {
            if (!IsConnected)
            {
                return;
            }

            if (tcpClient is { Connected: true })
            {
                if (raiseEvents)
                {
                    InvokeConnectionStateChanged(ConnectionState.Disconnected);
                }

                if (dispose)
                {
                    DisposeCancellationTokenAndTask();
                    DisposeTcpClientAndStream();
                }
                else
                {
                    tcpClient.GetStream().Close();
                    tcpClient.Close();
                }
            }

            IsConnected = false;
            if (raiseEvents)
            {
                InvokeDisconnected();
                InvokeConnectionStateChanged(ConnectionState.Disconnected);
            }
        }
        finally
        {
            syncLock.Release();
        }
    }

    private async Task DataReceiver(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsConnected ||
                networkStream is null)
            {
                await Task.Delay(TimeToWaitForDataReceiverInMs, cancellationToken);
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
            LogDataReceiveTimeout(IPAddressOrHostname, Port);
        }

        if (readDataTask.IsFaulted)
        {
            if (readDataTask.Exception is not null)
            {
                var (isKnownException, socketError) = readDataTask.Exception.IsKnownExceptionForNetworkCableUnplugged();
                if (isKnownException)
                {
                    LogDataReceiveError(IPAddressOrHostname, Port, $"SocketErrorCode = {socketError?.GetDescription()}");
                }
            }
            else
            {
                LogDataReceiveError(IPAddressOrHostname, Port, "Unknown error");
            }

            if (clientReconnectConfig.Enable)
            {
                await SetDisconnected(raiseEvents: true, dispose: false);
            }
            else
            {
                await SetDisconnected(raiseEvents: true, dispose: true);
            }
        }

        var data = await readDataTask;
        if (data.Length == 0)
        {
            if (IsConnected)
            {
                await Task.Delay(TimeToWaitForDisconnectionInMs, CancellationToken.None);
                if (IsConnected)
                {
                    LogDataReceiveNoData(IPAddressOrHostname, Port);
                    InvokeNoDataReceived();

                    if (clientReconnectConfig.Enable)
                    {
                        InvokeConnectionStateChanged(ConnectionState.Disconnected);

                        await DoReconnect();
                    }
                    else
                    {
                        await SetDisconnected(raiseEvents: true, dispose: true);
                    }
                }
            }
        }
        else
        {
            LogDataReceivedByteLength(IPAddressOrHostname, Port, data.Length);
            InvokeDataReceived(data);
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
        catch (OperationCanceledException)
        {
            // Skip
        }
        catch (ObjectDisposedException)
        {
            // Skip
        }
        catch (Exception ex)
        {
            var (isKnownException, _) = ex.IsKnownExceptionForConsumerDisposed();
            if (!isKnownException)
            {
                LogDataReceiveError(IPAddressOrHostname, Port, ex.Message);
            }
        }

        return Array.Empty<byte>();
    }

    private void CreateNewTcpClient()
    {
        tcpClient = new System.Net.Sockets.TcpClient();

        tcpClient.LingerState = new LingerOption(enable: true, seconds: 0);

        tcpClient.SetBufferSizeAndTimeouts(
            clientConfig.SendTimeout,
            clientConfig.ReceiveTimeout,
            clientConfig.SendBufferSize,
            clientConfig.ReceiveBufferSize);
    }

    private void CleanupIfNeededInDoConnect()
    {
        if (cancellationTokenSource is null)
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenRegistration = cancellationTokenSource.Token.Register(CancellationTokenCallback);

            receiveListenerTask = Task.Run(
                async () => await DataReceiver(cancellationTokenSource.Token),
                cancellationTokenSource.Token);
        }

        if (tcpClient is not null)
        {
            DisposeTcpClientAndStream();
        }
    }

    private void PrepareNetworkStreamAndKeepAliveInDoConnect()
    {
        networkStream = tcpClient!.GetStream();

        if (clientKeepAliveConfig.Enable)
        {
            tcpClient.SetKeepAlive(
                clientKeepAliveConfig.Time,
                clientKeepAliveConfig.Interval,
                clientKeepAliveConfig.RetryCount);
        }
        else
        {
            tcpClient.DisableKeepAlive();
        }
    }

    private void DisposeCancellationTokenAndTask()
    {
        if (cancellationTokenSource is not null)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }

            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        if (receiveListenerTask is not null)
        {
            if (receiveListenerTask.Status == TaskStatus.Running)
            {
                receiveListenerTask.Wait(TimeSpan.FromMilliseconds(TimeToWaitForDisposeDisconnectionInMs));
            }

            receiveListenerTask = null;
        }

        if (cancellationTokenRegistration is not null)
        {
            cancellationTokenRegistration.Value.Dispose();
            cancellationTokenRegistration = null;
        }
    }

    private void DisposeTcpClientAndStream()
    {
        IsConnected = false;

        if (clientKeepAliveConfig.Enable)
        {
            tcpClient?.DisableKeepAlive();
        }

        if (networkStream is not null)
        {
            if (networkStream.CanWrite ||
                networkStream.CanRead ||
                networkStream.CanSeek)
            {
                networkStream.Flush();
                networkStream.Close();
            }

            networkStream.Dispose();
            networkStream = null;
        }

        if (tcpClient is not null)
        {
            tcpClient.Close();
            tcpClient.Dispose();
            tcpClient = null;
        }
    }

    private void CancellationTokenCallback()
    {
        if (networkStream is null)
        {
            return;
        }

        networkStream.Flush();
        networkStream.Close();
    }
}