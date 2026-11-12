// ReSharper disable InvertIf
namespace Atc.Network.Udp;

/// <summary>
/// The main UdpClient - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:Element should not be on a single line", Justification = "OK.")]
public partial class UdpClient : IUdpClient
{
    private const int TimeToWaitForDisposeDisconnectionInMs = 50;
    private const int TimeToWaitForDataReceiverInMs = 150;

    private readonly int syncLockConnectTimeoutInMs;
    private readonly int syncLockSendTimeoutInMs;
    private readonly SemaphoreSlim syncLock = new(1, 1);
    private readonly UdpClientConfig clientConfig;
    private readonly ArraySegment<byte> receiveBufferSegment;
    private readonly string remoteEndPointAddressStr;
    private readonly ReceivedDataReaders receivedDataReaders = new();

    private Task? receiveListenerTask;
    private CancellationTokenSource? cancellationTokenSource;
    private CancellationTokenRegistration? cancellationTokenRegistration;
    private Socket? socket;
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
    /// Event to raise when data has become available from the server.
    /// </summary>
    public event Action<byte[]>? DataReceived;

    /// <summary>
    /// Event to raise when an error was handled and logged, so the client keeps running:
    /// an event handler that threw, or a failed receive. A handler of this event that throws is only logged.
    /// </summary>
    public event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

    private UdpClient(
        ILogger logger,
        UdpClientConfig? clientConfig)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
        this.clientConfig = clientConfig ?? new UdpClientConfig();

        var receiveBuffer = new byte[this.clientConfig.ReceiveBufferSize];
        receiveBufferSegment = new ArraySegment<byte>(receiveBuffer);

        syncLockConnectTimeoutInMs = this.clientConfig.ConnectTimeout <= 0
            ? UdpConstants.DefaultConnectTimeout
            : this.clientConfig.ConnectTimeout + UdpConstants.GracePeriodTimeout;

        syncLockSendTimeoutInMs = this.clientConfig.SendTimeout <= 0
            ? UdpConstants.DefaultSendReceiveTimeout
            : this.clientConfig.SendTimeout + UdpConstants.GracePeriodTimeout;

        remoteEndPointAddressStr = RemoteEndPoint.Address.ToString();
    }

    public UdpClient(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        UdpClientConfig? clientConfig = default)
        : this(logger, clientConfig)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        RemoteEndPoint = new IPEndPoint(ipAddress, port);
        remoteEndPointAddressStr = RemoteEndPoint.Address.ToString();
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public UdpClient(
        ILogger logger,
        IPEndPoint endpoint,
        UdpClientConfig? clientConfig = default)
        : this(logger, endpoint.Address, endpoint.Port, clientConfig)
    {
    }

    public UdpClient(
        IPAddress ipAddress,
        int port,
        UdpClientConfig? clientConfig = default)
        : this(NullLogger.Instance, ipAddress, port, clientConfig)
    {
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public UdpClient(
        IPEndPoint endpoint,
        UdpClientConfig? clientConfig = default)
        : this(NullLogger.Instance, endpoint.Address, endpoint.Port, clientConfig)
    {
    }

    /// <summary>
    /// IPEndPoint for server connection.
    /// </summary>
    public IPEndPoint RemoteEndPoint { get; } = new(IPAddress.Loopback, 0);

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
        => LazyInitializer.EnsureInitialized(ref instrumentation, () => new NetworkInstrumentation("udp_client", remoteEndPointAddressStr, RemoteEndPoint.Port));

    /// <summary>
    /// Connect.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task<bool> Connect(
        CancellationToken cancellationToken = default)
    {
        using var activity = Instrumentation.StartActivity("UdpClient.Connect", ActivityKind.Client);
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
        => DoDisconnect(raiseEventsAndLog: true);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task Send(
        string data,
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
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
    public Task Send(
        byte[] data,
        CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!IsConnected)
        {
            LogClientNotConnected(remoteEndPointAddressStr, RemoteEndPoint.Port);
            return;
        }

        TerminationHelper.AppendTerminationBytesIfNeeded(ref data, terminationType);

        var buffer = new ArraySegment<byte>(data);
        using var activity = Instrumentation.StartActivity("UdpClient.Send", ActivityKind.Client);
        var disconnectedDueToException = false;
        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(syncLockSendTimeoutInMs, cancellationToken);

            if (!lockAcquired)
            {
                return;
            }

            // The socket is connected; macOS rejects SendTo with an address on a connected socket (EISCONN).
            await socket!.SendAsync(buffer, SocketFlags.None, cancellationToken);
            Instrumentation.Sent(data.Length);
            await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
        }
        catch (SocketException ex)
        {
            LogDataSendingSocketError(ex.SocketErrorCode.ToString(), ex.Message);
            disconnectedDueToException = true;
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
        catch (Exception ex)
        {
            LogDataSendingError(ex.Message);
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
            await DoDisconnect(raiseEventsAndLog: true);
        }
    }

    /// <summary>
    /// Reads received data as an async stream, as an alternative to <see cref="DataReceived"/>: one item per
    /// <see cref="DataReceived"/> event, in the same order. The stream ends when the cancellationToken is cancelled
    /// or the client is disposed; it continues across reconnects.
    /// </summary>
    /// <remarks>
    /// Data received before the enumeration starts is not included. Each enumeration buffers up to 256 items;
    /// when it falls behind, the client waits before reading more, so a slow reader slows down the sender
    /// instead of using more memory.
    /// </remarks>
    /// <param name="cancellationToken">Ends the stream.</param>
    /// <returns>The received data.</returns>
    public IAsyncEnumerable<ReadOnlyMemory<byte>> ReadAllAsync(
        CancellationToken cancellationToken = default)
        => receivedDataReaders.ReadAllAsync(cancellationToken);

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

        receivedDataReaders.CompleteAll();
        DisposeCancellationTokenAndTask();
        DisposeSocket();

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

    /// <summary>
    /// Raises <see cref="ErrorOccurred"/>. Each subscriber is called on its own; one that throws is only logged,
    /// so an error handler can't cause another error.
    /// </summary>
    private void InvokeError(
        NetworkErrorKind kind,
        Exception exception,
        string? eventName = null)
    {
        var handlers = ErrorOccurred;
        if (handlers is null)
        {
            return;
        }

        var eventArgs = new NetworkErrorEventArgs(kind, exception, eventName);
        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler<NetworkErrorEventArgs>>())
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception ex)
            {
                LogEventHandlerError(remoteEndPointAddressStr, RemoteEndPoint.Port, nameof(ErrorOccurred), ex);
                Instrumentation.HandlerFailed(nameof(ErrorOccurred));
            }
        }
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
            LogEventHandlerError(remoteEndPointAddressStr, RemoteEndPoint.Port, eventName, ex);
            Instrumentation.HandlerFailed(eventName);
            InvokeError(NetworkErrorKind.EventHandler, ex, eventName);
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
            LogDataReceivedHandlerError(remoteEndPointAddressStr, RemoteEndPoint.Port, ex);
            Instrumentation.HandlerFailed(nameof(DataReceived));
            InvokeError(NetworkErrorKind.EventHandler, ex, nameof(DataReceived));
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
            LogConnecting(remoteEndPointAddressStr, RemoteEndPoint.Port);
            InvokeConnectionStateChanged(ConnectionState.Connecting);
        }

        CleanupIfNeededInDoConnect();

        CreateNewSocket();

        try
        {
            var connectTimeoutTask = Task.Delay(clientConfig.ConnectTimeout, cancellationToken);
            var connectTask = socket!
                .ConnectAsync(RemoteEndPoint, cancellationToken)
                .AsTask();

            // Double await so if connectTimeoutTask throws exception, this throws it
            await await Task.WhenAny(connectTask, connectTimeoutTask);

            if (connectTimeoutTask.IsCompleted)
            {
                // If connectTimeoutTask and connectTask both finish at the same time,
                // we'll consider it to be a timeout.
                throw new SocketException((int)SocketError.TimedOut);
            }
        }
        catch (Exception ex)
        {
            if (raiseEventsAndLog)
            {
                Instrumentation.ConnectionFailed();
                LogConnectionError(remoteEndPointAddressStr, RemoteEndPoint.Port, ex.Message);
                InvokeConnectionStateChanged(ConnectionState.ConnectionFailed, ex.Message);
            }

            if (socket is not null)
            {
                socket.Close();
                socket.Dispose();
                socket = null;
            }

            return false;
        }

        await SetConnected(raiseEventsAndLog, cancellationToken);

        if (raiseEventsAndLog)
        {
            LogConnected(remoteEndPointAddressStr, RemoteEndPoint.Port);
            InvokeConnectionStateChanged(ConnectionState.Connected);
        }

        return true;
    }

    private Task DoDisconnect(bool raiseEventsAndLog)
    {
        if (raiseEventsAndLog)
        {
            InvokeConnectionStateChanged(ConnectionState.Disconnecting);

            LogDisconnecting(remoteEndPointAddressStr, RemoteEndPoint.Port);
        }

        return SetDisconnected(raiseEvents: raiseEventsAndLog, cancellationToken: CancellationToken.None);
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
        bool raiseEvents = true,
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

            if (socket is { Connected: true })
            {
                if (raiseEvents)
                {
                    InvokeConnectionStateChanged(ConnectionState.Disconnecting);
                }

                DisposeSocket();
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
                socket is null)
            {
                await Task.Delay(TimeToWaitForDataReceiverInMs, cancellationToken);
                continue;
            }

            await HandleReceiveMessage(cancellationToken);
        }
    }

    private async Task HandleReceiveMessage(CancellationToken cancellationToken)
    {
        SocketReceiveMessageFromResult? res;

        try
        {
            res = await socket!.ReceiveMessageFromAsync(receiveBufferSegment, SocketFlags.None, RemoteEndPoint);
        }
        catch (Exception ex)
        {
            // Errors caused by disconnecting or disposing the client are expected; report the others.
            if (ex is not (OperationCanceledException or ObjectDisposedException) &&
                !ex.IsKnownExceptionForConsumerDisposed().IsKnownSocketException)
            {
                InvokeError(NetworkErrorKind.Receive, ex);
            }

            return;
        }

        // Copy only the received bytes, not the whole receive buffer.
        var receivedBytes = receiveBufferSegment.AsSpan(0, res.Value.ReceivedBytes).ToArray();

        Instrumentation.Received(receivedBytes.Length);
        InvokeDataReceived(receivedBytes);

        try
        {
            // Waits while a ReadAllAsync reader's buffer is full.
            await receivedDataReaders.WriteAsync(receivedBytes, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The client is disconnecting.
        }
    }

    private void CreateNewSocket()
    {
        // The socket's address family follows the server's address, so IPv6 servers work.
        socket = new Socket(
            RemoteEndPoint.AddressFamily,
            SocketType.Dgram,
            ProtocolType.Udp);

        socket.SetSocketOption(
            RemoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP,
            SocketOptionName.PacketInformation,
            optionValue: true);

        socket.SendTimeout = clientConfig.SendTimeout;
        socket.SendBufferSize = clientConfig.SendBufferSize;
        socket.ReceiveTimeout = clientConfig.ReceiveTimeout;
        socket.ReceiveBufferSize = clientConfig.ReceiveBufferSize;

        if (OperatingSystem.IsWindows())
        {
            socket.SetIPProtectionLevel(clientConfig.IPProtectionLevel);
        }
    }

    private void CleanupIfNeededInDoConnect()
    {
        if (cancellationTokenSource is null)
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenRegistration = cancellationTokenSource.Token.Register(() => { });

            receiveListenerTask = NetworkInstrumentation.RunOutsideCurrentActivity(
                async () => await DataReceiver(cancellationTokenSource.Token),
                cancellationTokenSource.Token);
        }

        if (socket is not null)
        {
            DisposeSocket();
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

    private void DisposeSocket()
    {
        IsConnected = false;

        if (socket is not null)
        {
            socket.Close();
            socket.Dispose();
            socket = null;
        }
    }
}