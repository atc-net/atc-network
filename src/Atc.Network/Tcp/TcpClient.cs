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
    private CancellationTokenSource? cancellationTokenSource;
    private CancellationTokenRegistration? cancellationTokenRegistration;
    private Task? receiveListenerTask;
    private System.Net.Sockets.TcpClient? tcpClient;
    private Stream? networkStream;
    private NetworkInstrumentation? instrumentation;
    private bool isConnected;

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
    public bool IsConnected
    {
        get => isConnected;
        private set => SetIsConnected(value);
    }

    /// <summary>
    /// Gets the metrics and activities of this instance. Created on first use, when the address and port are known.
    /// </summary>
    private NetworkInstrumentation Instrumentation
        => LazyInitializer.EnsureInitialized(ref instrumentation, () => new NetworkInstrumentation("tcp_client", IPAddressOrHostname, Port));

    /// <summary>
    /// Connect.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task<bool> Connect(
        CancellationToken cancellationToken = default)
    {
        using var activity = Instrumentation.StartActivity("TcpClient.Connect", ActivityKind.Client);
        var connected = await DoConnect(raiseEventsAndLog: true, cancellationToken);
        if (!connected && !IsConnected)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        return connected;
    }

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

        using var activity = Instrumentation.StartActivity("TcpClient.Send", ActivityKind.Client);
        var disconnectedDueToException = false;

        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(syncLockSendTimeoutInMs, cancellationToken);

            if (!lockAcquired)
            {
                return;
            }

            await networkStream!.WriteAsync(data.AsMemory(0, data.Length), cancellationToken);
            await networkStream.FlushAsync(cancellationToken);
            Instrumentation.Sent(data.Length);
        }
        catch (SocketException ex)
        {
            LogDataSendingSocketError(IPAddressOrHostname, Port, ex.SocketErrorCode.ToString(), ex.Message);
            disconnectedDueToException = true;
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
        catch (Exception ex)
        {
            LogDataSendingError(IPAddressOrHostname, Port, ex.Message);
            disconnectedDueToException = true;
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
        finally
        {
            if (lockAcquired)
            {
                syncLock.Release();
            }
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
    protected virtual void OnDataReceived(byte[] bytes) { }

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

        DisposeCancellationTokenAndTask();
        DisposeTcpClientAndStream();

        syncLock.Dispose();
    }

    private void InvokeConnected()
    {
        InvokeHandlers(Connected, nameof(Connected), handler => ((Action)handler)());
        InvokeHandler(nameof(OnConnected), OnConnected);
    }

    private void InvokeDisconnected()
    {
        InvokeHandlers(Disconnected, nameof(Disconnected), handler => ((Action)handler)());
        InvokeHandler(nameof(OnDisconnected), OnDisconnected);
    }

    private void InvokeConnectionStateChanged(
        ConnectionState connectionState,
        string? errorMessage = null)
    {
        var eventArgs = errorMessage is null
            ? new ConnectionStateEventArgs(connectionState)
            : new ConnectionStateEventArgs(connectionState, errorMessage);

        InvokeHandlers(
            ConnectionStateChanged,
            nameof(ConnectionStateChanged),
            handler => ((EventHandler<ConnectionStateEventArgs>)handler)(this, eventArgs));
        InvokeHandler(nameof(OnConnectionStateChanged), () => OnConnectionStateChanged(connectionState, errorMessage));
    }

    private void InvokeNoDataReceived()
    {
        InvokeHandlers(NoDataReceived, nameof(NoDataReceived), handler => ((Action)handler)());
        InvokeHandler(nameof(OnNoDataReceived), OnNoDataReceived);
    }

    /// <summary>
    /// Calls each subscriber on its own, so a failing handler is logged
    /// and neither skips the others nor breaks the client.
    /// </summary>
    private void InvokeHandlers(
        Delegate? handlers,
        string eventName,
        Action<Delegate> invoke)
    {
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList())
        {
            InvokeHandler(eventName, () => invoke(handler));
        }
    }

    private void SetIsConnected(bool value)
    {
        isConnected = value;
        Instrumentation.SetConnected(value);
    }

    private void InvokeHandler(
        string eventName,
        Action invoke)
    {
        try
        {
            invoke();
        }
        catch (Exception ex)
        {
            LogEventHandlerError(IPAddressOrHostname, Port, eventName, ex);
            Instrumentation.HandlerFailed(eventName);
        }
    }

    private void InvokeDataReceived(byte[] data)
    {
        // Call each handler on its own, so one failing handler
        // neither skips the others nor stops the receive loop.
        var handlers = DataReceived;
        if (handlers is not null)
        {
            foreach (var handler in handlers.GetInvocationList().Cast<Action<byte[]>>())
            {
                InvokeDataReceivedHandler(handler, data);
            }
        }

        InvokeDataReceivedHandler(OnDataReceived, data);
    }

    private void InvokeDataReceivedHandler(
        Action<byte[]> handler,
        byte[] data)
    {
        try
        {
            handler(data);
        }
        catch (Exception ex)
        {
            LogDataReceivedHandlerError(IPAddressOrHostname, Port, ex);
            Instrumentation.HandlerFailed(nameof(DataReceived));
        }
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

            // The TLS handshake runs here, so a failed handshake fails Connect before Connected is raised.
            networkStream = await CreateStream(cancellationToken);
        }
        catch (Exception ex)
        {
            if (raiseEventsAndLog)
            {
                Instrumentation.ConnectionFailed();
                LogConnectionError(IPAddressOrHostname, Port, ex.Message);
                InvokeConnectionStateChanged(ConnectionState.ConnectionFailed, ex.Message);
            }

            if (tcpClient is not null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
                tcpClient = null;
            }

            return false;
        }

        await SetConnected(raiseEventsAndLog, cancellationToken);
        PrepareKeepAliveInDoConnect();

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

        return SetDisconnected(raiseEvents: raiseEventsAndLog, dispose: dispose, cancellationToken: CancellationToken.None);
    }

    /// <summary>
    /// Tries to reconnect: one first attempt plus up to RetryMaxAttempts retries, RetryInterval apart.
    /// Each reconnect cycle starts its own attempt count.
    /// </summary>
    private async Task DoReconnect()
    {
        for (var attempt = 0; attempt <= clientReconnectConfig.RetryMaxAttempts; attempt++)
        {
            LogReconnecting(IPAddressOrHostname, Port);
            InvokeConnectionStateChanged(ConnectionState.Reconnecting);

            await SetDisconnected(raiseEvents: false, dispose: false, cancellationToken: CancellationToken.None);

            await Task.Delay(clientReconnectConfig.RetryInterval, CancellationToken.None);

            if (await DoConnect(raiseEventsAndLog: false, CancellationToken.None))
            {
                LogReconnected(IPAddressOrHostname, Port);
                Instrumentation.Reconnected(success: true);
                InvokeConnectionStateChanged(ConnectionState.Reconnected);
                return;
            }

            Instrumentation.Reconnected(success: false);
            if (attempt < clientReconnectConfig.RetryMaxAttempts)
            {
                LogReconnectionWarning(IPAddressOrHostname, Port, attempt, clientReconnectConfig.RetryMaxAttempts);
                InvokeConnectionStateChanged(ConnectionState.ReconnectionFailed);
            }
        }

        LogReconnectionMaxRetryExceededError(IPAddressOrHostname, Port);
        InvokeConnectionStateChanged(ConnectionState.ReconnectionFailed);
    }

    private async Task SetConnected(
        bool raiseEvents,
        CancellationToken cancellationToken = default)
    {
        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(syncLockConnectTimeoutInMs, cancellationToken);

            if (!lockAcquired)
            {
                return;
            }

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
            if (lockAcquired)
            {
                syncLock.Release();
            }
        }
    }

    private async Task SetDisconnected(
        bool raiseEvents,
        bool dispose,
        CancellationToken cancellationToken = default)
    {
        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(syncLockConnectTimeoutInMs, cancellationToken);

            if (!lockAcquired)
            {
                return;
            }

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
                    // Close the stream in use, which is the TLS stream when TLS is enabled.
                    networkStream?.Close();
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
            if (lockAcquired)
            {
                syncLock.Release();
            }
        }
    }

    private async Task DataReceiver(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsConnected ||
                networkStream is null)
            {
                await Task.Delay(TimeToWaitForDataReceiverInMs, cancellationToken);
                continue;
            }

            try
            {
                await HandleReadDataTaskResponse(ReadData(cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                await HandleDataReceiverUnexpectedError(ex);
            }
        }
    }

    private async Task HandleDataReceiverUnexpectedError(Exception exception)
    {
        LogDataReceiverUnexpectedError(IPAddressOrHostname, Port, exception);

        try
        {
            if (clientReconnectConfig.Enable)
            {
                await SetDisconnected(raiseEvents: true, dispose: false, cancellationToken: CancellationToken.None);
                await DoReconnect();
            }
            else
            {
                await SetDisconnected(raiseEvents: true, dispose: true, cancellationToken: CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            // An event handler may throw while we recover; log it so the loop keeps running.
            LogDataReceiverUnexpectedError(IPAddressOrHostname, Port, ex);
        }
    }

    private async Task HandleReadDataTaskResponse(Task<byte[]> readDataTask)
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
                await SetDisconnected(raiseEvents: true, dispose: false, cancellationToken: CancellationToken.None);
            }
            else
            {
                await SetDisconnected(raiseEvents: true, dispose: true, cancellationToken: CancellationToken.None);
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
                        await SetDisconnected(raiseEvents: true, dispose: true, cancellationToken: CancellationToken.None);
                    }
                }
            }
        }
        else
        {
            HandleDataRead(data);
        }
    }

    private void HandleDataRead(byte[] data)
    {
        LogDataReceivedByteLength(IPAddressOrHostname, Port, data.Length);
        Instrumentation.Received(data.Length);
        InvokeDataReceived(data);
    }

    private async Task<byte[]> ReadData(CancellationToken cancellationToken)
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

            return receiveBuffer.AsSpan(0, numberOfBytesToRead).ToArray();
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

            receiveListenerTask = NetworkInstrumentation.RunOutsideCurrentActivity(
                async () => await DataReceiver(cancellationTokenSource.Token),
                cancellationTokenSource.Token);
        }

        if (tcpClient is not null)
        {
            DisposeTcpClientAndStream();
        }
    }

    /// <summary>
    /// Returns the stream to read from and write to: the network stream, or, when
    /// <see cref="TcpClientConfig.SslOptions"/> is set, a TLS stream that is authenticated
    /// within <see cref="TcpClientConfig.ConnectTimeout"/>.
    /// </summary>
    private async Task<Stream> CreateStream(CancellationToken cancellationToken)
    {
        var stream = tcpClient!.GetStream();
        if (clientConfig.SslOptions is null)
        {
            return stream;
        }

        var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (clientConfig.ConnectTimeout > 0)
            {
                timeoutCts.CancelAfter(clientConfig.ConnectTimeout);
            }

            await sslStream.AuthenticateAsClientAsync(
                CreateSslClientAuthenticationOptions(clientConfig.SslOptions),
                timeoutCts.Token);

            return sslStream;
        }
        catch
        {
            await sslStream.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Returns the configured options, or a copy that uses the host this client connects to
    /// when <see cref="SslClientAuthenticationOptions.TargetHost"/> is empty.
    /// The configured options are not changed, as they may be shared by several clients.
    /// </summary>
    private SslClientAuthenticationOptions CreateSslClientAuthenticationOptions(
        SslClientAuthenticationOptions options)
    {
        if (!string.IsNullOrEmpty(options.TargetHost))
        {
            return options;
        }

        var copy = new SslClientAuthenticationOptions
        {
            AllowRenegotiation = options.AllowRenegotiation,
            AllowTlsResume = options.AllowTlsResume,
            ApplicationProtocols = options.ApplicationProtocols,
            CertificateChainPolicy = options.CertificateChainPolicy,
            CertificateRevocationCheckMode = options.CertificateRevocationCheckMode,
            CipherSuitesPolicy = options.CipherSuitesPolicy,
            ClientCertificateContext = options.ClientCertificateContext,
            ClientCertificates = options.ClientCertificates,
            EnabledSslProtocols = options.EnabledSslProtocols,
            EncryptionPolicy = options.EncryptionPolicy,
            LocalCertificateSelectionCallback = options.LocalCertificateSelectionCallback,
            RemoteCertificateValidationCallback = options.RemoteCertificateValidationCallback,
            TargetHost = IPAddressOrHostname,
        };

        // These can only be set on Windows and Linux, so elsewhere they always have their defaults.
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            copy.AllowRsaPkcs1Padding = options.AllowRsaPkcs1Padding;
            copy.AllowRsaPssPadding = options.AllowRsaPssPadding;
        }

        return copy;
    }

    private void PrepareKeepAliveInDoConnect()
    {
        var client = tcpClient!;
        if (clientKeepAliveConfig.Enable)
        {
            client.SetKeepAlive(
                clientKeepAliveConfig.Time,
                clientKeepAliveConfig.Interval,
                clientKeepAliveConfig.RetryCount);
        }
        else
        {
            client.DisableKeepAlive();
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
                receiveListenerTask.Wait(TimeSpan.FromMilliseconds(TimeToWaitForDisposeDisconnectionInMs), CancellationToken.None);
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