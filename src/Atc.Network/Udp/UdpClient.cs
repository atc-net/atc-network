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

    private static readonly SemaphoreSlim SyncLock = new(1, 1);

    private readonly UdpClientConfig clientConfig;
    private readonly ArraySegment<byte> receiveBufferSegment;

    private Task? receiveListenerTask;
    private CancellationTokenSource? cancellationTokenSource;
    private CancellationTokenRegistration? cancellationTokenRegistration;
    private Socket? socket;

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

    private UdpClient(
        ILogger logger,
        UdpClientConfig? clientConfig)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
        this.clientConfig = clientConfig ?? new UdpClientConfig();

        var receiveBuffer = new byte[this.clientConfig.ReceiveBufferSize];
        receiveBufferSegment = new ArraySegment<byte>(receiveBuffer);
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
            LogClientNotConnected(RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port);
            return;
        }

        TerminationHelper.AppendTerminationBytesIfNeeded(ref data, terminationType);

        var buffer = new ArraySegment<byte>(data);
        await socket!.SendToAsync(buffer, SocketFlags.None, RemoteEndPoint, cancellationToken);
        await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
    }

    protected virtual void OnConnected() { }

    protected virtual void OnDisconnected() { }

    protected virtual void OnConnectionStateChanged(
        ConnectionState connectionState,
        string? errorMessage = null) { }

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
        DisposeSocket();
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

    private void InvokeDataReceived(
        byte[] data)
    {
        DataReceived?.Invoke(data);
        OnDataReceived(data);
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
            LogConnecting(RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port);
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
                LogConnectionError(RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port, ex.Message);
                InvokeConnectionStateChanged(ConnectionState.ConnectionFailed, ex.Message);
            }

            if (socket is not null)
            {
                socket!.Close();
                socket.Dispose();
                socket = null;
            }

            return false;
        }

        await SetConnected(raiseEventsAndLog, cancellationToken);

        if (raiseEventsAndLog)
        {
            LogConnected(RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port);
            InvokeConnectionStateChanged(ConnectionState.Connected);
        }

        return true;
    }

    private Task DoDisconnect(
        bool raiseEventsAndLog)
    {
        if (raiseEventsAndLog)
        {
            InvokeConnectionStateChanged(ConnectionState.Disconnecting);

            LogDisconnecting(RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port);
        }

        return SetDisconnected(raiseEvents: raiseEventsAndLog);
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
                InvokeConnected();
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
            SyncLock.Release();
        }
    }

    private async Task DataReceiver(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsConnected ||
                socket is null)
            {
                await Task.Delay(TimeToWaitForDataReceiverInMs, cancellationToken);
                continue;
            }

            await HandleReceiveMessage();
        }
    }

    private async Task HandleReceiveMessage()
    {
        SocketReceiveMessageFromResult? res;

        try
        {
            res = await socket!.ReceiveMessageFromAsync(receiveBufferSegment, SocketFlags.None, RemoteEndPoint);
        }
        catch
        {
            return;
        }

        var receivedBytes = new byte[res.Value.ReceivedBytes];
        Array.Copy(receiveBufferSegment.ToArray(), 0, receivedBytes, 0, res.Value.ReceivedBytes);

        InvokeDataReceived(receivedBytes);
    }

    private void CreateNewSocket()
    {
        socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);

        socket.SetSocketOption(
            SocketOptionLevel.IP,
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

            receiveListenerTask = Task.Run(
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