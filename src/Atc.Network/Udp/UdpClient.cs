// ReSharper disable InvertIf
namespace Atc.Network.Udp;

/// <summary>
/// The main UdpClient - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
public partial class UdpClient : IUdpClient
{
    private const int TimeToWaitForDisposeDisconnectionInMs = 50;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private readonly UdpClientConfig udpClientConfig;
    private readonly Socket? socket;
    private readonly ArraySegment<byte> receiveBufferSegment;
    private readonly Task? receiveListenerTask;
    private readonly CancellationTokenSource cancellationTokenSource = new();

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
        UdpClientConfig? udpClientConfig)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
        this.udpClientConfig = udpClientConfig ?? new UdpClientConfig();

        socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);

        socket.SetSocketOption(
            SocketOptionLevel.IP,
            SocketOptionName.PacketInformation,
            optionValue: true);

        if (OperatingSystem.IsWindows())
        {
            socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
        }

        var receiveBuffer = new byte[this.udpClientConfig.ReceiveBufferSize];
        receiveBufferSegment = new ArraySegment<byte>(receiveBuffer);
    }

    public UdpClient(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        UdpClientConfig? udpClientConfig = default)
        : this(logger, udpClientConfig)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        RemoteEndPoint = new IPEndPoint(ipAddress, port);

        receiveListenerTask = Task.Run(
            async () => await DataReceiver(cancellationTokenSource.Token),
            cancellationTokenSource.Token);
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public UdpClient(
        ILogger logger,
        IPEndPoint endpoint,
        UdpClientConfig? udpClientConfig = default)
        : this(logger, endpoint.Address, endpoint.Port, udpClientConfig)
    {
    }

    public UdpClient(
        IPAddress ipAddress,
        int port,
        UdpClientConfig? udpClientConfig = default)
        : this(NullLogger.Instance, ipAddress, port, udpClientConfig)
    {
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public UdpClient(
        IPEndPoint endpoint,
        UdpClientConfig? udpClientConfig = default)
        : this(NullLogger.Instance, endpoint.Address, endpoint.Port, udpClientConfig)
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
        => Send(udpClientConfig.DefaultEncoding, data, cancellationToken);

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
            udpClientConfig.TerminationType,
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
        => Send(data, udpClientConfig.TerminationType, cancellationToken);

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

        if (receiveListenerTask!.Status == TaskStatus.Running)
        {
            receiveListenerTask.Wait(TimeSpan.FromMilliseconds(TimeToWaitForDisposeDisconnectionInMs));
        }

        socket?.Dispose();
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
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Connecting));
        }

        try
        {
            await socket!.ConnectAsync(RemoteEndPoint, cancellationToken);
        }
        catch (Exception ex)
        {
            if (raiseEventsAndLog)
            {
                LogConnectionError(RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port, ex.Message);
                ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.ConnectionFailed, ex.Message));
            }

            return false;
        }

        await SetConnected(raiseEventsAndLog, cancellationToken);

        if (raiseEventsAndLog)
        {
            LogConnected(RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port);
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

            if (socket is { Connected: true })
            {
                if (raiseEvents)
                {
                    ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Disconnecting));
                }

                socket.Close();
            }

            socket?.Dispose();

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

    private async Task DataReceiver(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsConnected)
            {
                continue;
            }

            var res = await socket!.ReceiveMessageFromAsync(receiveBufferSegment, SocketFlags.None, RemoteEndPoint);
            var receivedBytes = new byte[res.ReceivedBytes];
            Array.Copy(receiveBufferSegment.ToArray(), 0, receivedBytes, 0, res.ReceivedBytes);

            DataReceived?.Invoke(receivedBytes);
        }
    }
}