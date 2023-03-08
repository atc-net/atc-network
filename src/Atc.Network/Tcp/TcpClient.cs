// ReSharper disable InvertIf
// ReSharper disable InvertIf
namespace Atc.Network.Tcp;

/// <summary>
/// The main TcpClient - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
public partial class TcpClient : IDisposable
{
    private const int TimeToWaitForDisconnectionInMs = 200;
    private const int TimeToWaitForDisposeDisconnectionInMs = 50;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private readonly TcpClientConfig clientConfig;
    private readonly TcpClientKeepAliveConfig keepAliveConfig;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly CancellationTokenRegistration cancellationTokenRegistration;

    private readonly byte[] receiveBuffer;
    private readonly Task receiveListenerTask;

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
        TcpClientKeepAliveConfig? keepAliveConfig)
    {
        this.logger = logger;
        this.clientConfig = clientConfig ?? new TcpClientConfig();
        this.keepAliveConfig = keepAliveConfig ?? new TcpClientKeepAliveConfig();

        receiveBuffer = new byte[this.clientConfig.ReceiveBufferSize];

        cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenRegistration = cancellationTokenSource.Token.Register(CancellationTokenCallback);

        receiveListenerTask = Task.Run(
            async () => await DataReceiver(cancellationTokenSource.Token),
            cancellationTokenSource.Token);
    }

    public TcpClient(
        ILogger logger,
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

        IPAddressOrHostname = hostname;
        this.Port = port;
    }

    public TcpClient(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(logger, clientConfig, keepAliveConfig)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        IPAddressOrHostname = ipAddress.ToString();
        this.Port = port;
    }

    public TcpClient(
        ILogger logger,
        IPEndPoint ipEndpoint,
        TcpClientConfig? clientConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(logger, clientConfig, keepAliveConfig)
    {
        ArgumentNullException.ThrowIfNull(ipEndpoint);

        IPAddressOrHostname = ipEndpoint.Address.ToString();
        Port = ipEndpoint.Port;
    }

    public TcpClient(
        string hostname,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(NullLogger.Instance, hostname, port, clientConfig, keepAliveConfig)
    {
    }

    public TcpClient(
        IPAddress ipAddress,
        int port,
        TcpClientConfig? clientConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(NullLogger.Instance, ipAddress, port, clientConfig, keepAliveConfig)
    {
    }

    public TcpClient(
        IPEndPoint ipEndpoint,
        TcpClientConfig? clientConfig = default,
        TcpClientKeepAliveConfig? keepAliveConfig = default)
            : this(NullLogger.Instance, ipEndpoint, clientConfig, keepAliveConfig)
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
        => DoDisconnect(raiseEventsAndLog: true);

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

        LogDataSendingByteLength(data.Length);

        await networkStream!.WriteAsync(data.AsMemory(0, data.Length), cancellationToken);
        await networkStream.FlushAsync(cancellationToken);
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

        if (!cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }

        cancellationTokenSource.Dispose();

        if (receiveListenerTask.Status == TaskStatus.Running)
        {
            receiveListenerTask.Wait(TimeSpan.FromMilliseconds(TimeToWaitForDisposeDisconnectionInMs));
        }

        cancellationTokenRegistration.Dispose();

        DisposeTcpClientAndStream();
    }

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
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Connecting));
        }

        tcpClient = new System.Net.Sockets.TcpClient();
        SetBufferSizeAndTimeouts();

        try
        {
            var connectTimeoutTask = Task.Delay(clientConfig.ConnectTimeout, cancellationToken);
            var connectTask = tcpClient
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
                ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.ConnectionFailed, ex.Message));
            }

            return false;
        }

        await SetConnected(raiseEventsAndLog, cancellationToken);
        PrepareNetworkStream();

        if (raiseEventsAndLog)
        {
            LogConnected(IPAddressOrHostname, Port);
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Connected));
        }

        return true;
    }

    private Task DoDisconnect(
        bool raiseEventsAndLog)
    {
        if (raiseEventsAndLog)
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Disconnecting));

            LogDisconnecting(IPAddressOrHostname, Port);
        }

        DisposeTcpClientAndStream();
        return SetDisconnected(raiseEvents: raiseEventsAndLog);
    }

    private async Task DoReconnect()
    {
        LogReconnecting(IPAddressOrHostname, Port);
        ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Reconnecting));

        DisposeTcpClientAndStream();
        await SetDisconnected(raiseEvents: false);

        if (clientConfig.ReconnectDelay.HasValue)
        {
            await Task.Delay(clientConfig.ReconnectDelay.Value);
        }

        if (await DoConnect(raiseEventsAndLog: false, CancellationToken.None))
        {
            LogReconnected(IPAddressOrHostname, Port);
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Reconnected));
        }
        else
        {
            LogConnectionError(IPAddressOrHostname, Port, "Could not reconnect");
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.ConnectionFailed));
        }
    }

    private async Task SetConnected(
        bool raiseEvents,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            if (IsConnected)
            {
                return;
            }

            IsConnected = true;
            if (raiseEvents)
            {
                Connected?.Invoke();
            }
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private async Task SetDisconnected(
        bool raiseEvents = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            if (!IsConnected)
            {
                return;
            }

            if (tcpClient is { Connected: true })
            {
                if (raiseEvents)
                {
                    ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Disconnecting));
                }

                tcpClient.Close();
            }

            IsConnected = false;
            if (raiseEvents)
            {
                Disconnected?.Invoke();
                ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Disconnected));
            }
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
        CancellationToken cancellationToken)
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
                var (isKnownException, socketError) = readDataTask.Exception.IsKnownExceptionForNetworkCableUnplugged();
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
                await Task.Delay(TimeToWaitForDisconnectionInMs, CancellationToken.None);
                if (IsConnected)
                {
                    LogDataReceiveNoData();
                    NoDataReceived?.Invoke();

                    if (clientConfig.ReconnectOnSenderSocketClosed)
                    {
                        ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Disconnected));

                        await DoReconnect();
                    }
                    else
                    {
                        await SetDisconnected(raiseEvents: true);
                    }
                }
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
        catch (ObjectDisposedException)
        {
            // Skip
        }
        catch (Exception exception)
        {
            var (isKnownException, _) = exception.IsKnownExceptionForConsumerDisposed();
            if (!isKnownException)
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

        networkStream.Close();
    }
}