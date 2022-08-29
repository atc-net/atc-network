namespace Atc.Network.Udp;

/// <summary>
/// The main UdpClient - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
public partial class UdpClient : IDisposable
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);

    private readonly string ipAddressOrHostname = string.Empty;
    private readonly int port;

    private System.Net.Sockets.UdpClient? udpClient;

    public UdpClient(
        ILogger logger,
        string hostname,
        int port)
    {
        ArgumentNullException.ThrowIfNull(hostname);

        this.logger = logger;
        this.ipAddressOrHostname = hostname;
        this.port = port;
    }

    public UdpClient(
        ILogger logger,
        IPAddress ipAddress,
        int port)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        this.logger = logger;
        this.ipAddressOrHostname = ipAddress.ToString();
        this.port = port;
    }

    public UdpClient(
        string hostname,
        int port)
        : this(NullLogger.Instance, hostname, port)
    {
    }

    public UdpClient(
        IPAddress ipAddress,
        int port)
        : this(NullLogger.Instance, ipAddress, port)
    {
    }

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

    public Task Send(
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!IsConnected)
        {
            throw new TcpException("Client is not connected!");
        }

        return udpClient!.SendAsync(data, data.Length);
    }

    public Task Send(
        string data,
        CancellationToken cancellationToken = default)
        => Send(Encoding.ASCII, data, cancellationToken);

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

        return Send(encoding.GetBytes(data), cancellationToken);
    }

    private async Task<bool> DoConnect(
        bool raiseEventsAndLog,
        CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            return false;
        }

        udpClient = new System.Net.Sockets.UdpClient();

        try
        {
            udpClient.Connect(ipAddressOrHostname, port);
            udpClient.BeginReceive(DataReceiver, null);
        }
        catch (Exception ex)
        {
            return false;
        }

        await SetConnected(raiseEventsAndLog);

        return true;
    }

    private async Task DoDisconnect(
        bool raiseEventsAndLog)
    {
        if (!IsConnected)
        {
            return;
        }

        udpClient!.Close();
        await SetDisconnected(raiseEvents: raiseEventsAndLog);
    }

    private async Task SetConnected(
        bool raiseEvents)
    {
        try
        {
            await SyncLock.WaitAsync();

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
        bool raiseEvents = true)
    {
        try
        {
            await SyncLock.WaitAsync();

            if (!IsConnected)
            {
                return;
            }

            if (udpClient is { Client.Connected: true })
            {
                if (raiseEvents)
                {
                    ConnectionStateChanged?.Invoke(this, new ConnectionStateEventArgs(ConnectionState.Disconnecting));
                }

                udpClient.Close();
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

        DisposeUdpClientAndStream();
    }

    private void DisposeUdpClientAndStream()
    {
        if (udpClient is not null)
        {
            udpClient.Dispose();
        }
    }

    private void DataReceiver(IAsyncResult result)
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Any, port);

        try
        {
            var data = udpClient!.EndReceive(result, ref ipEndPoint);
            var msg = Encoding.Unicode.GetString(data);

            udpClient!.BeginReceive(DataReceiver, null);
        }
        catch (ObjectDisposedException)
        {
            // Skip
        }
    }
}