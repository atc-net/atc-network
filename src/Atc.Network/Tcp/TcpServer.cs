namespace Atc.Network.Tcp;

/// <summary>
/// The main TcpServer - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:Element should not be on a single line", Justification = "OK.")]
public partial class TcpServer : ITcpServer
{
    private const int TimeToWaitForStartTcpListenerInMs = 500;

    private readonly TcpServerConfig serverConfig;
    private readonly TcpListener? tcpListener;

    public event Action<byte[]>? DataReceived;

    private TcpServer(
        ILogger logger,
        TcpServerConfig? serverConfig)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
        this.serverConfig = serverConfig ?? new TcpServerConfig();

        IpAddress = IPAddress.Any;
        Port = -1;
    }

    public TcpServer(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        TcpServerConfig? serverConfig = default)
        : this(logger, serverConfig)
    {
        IpAddress = ipAddress;
        Port = port;

        tcpListener = new TcpListener(IpAddress, Port)
        {
            Server =
            {
                SendTimeout = this.serverConfig.SendTimeout,
                SendBufferSize = this.serverConfig.SendBufferSize,
                ReceiveTimeout = this.serverConfig.ReceiveTimeout,
                ReceiveBufferSize = this.serverConfig.ReceiveBufferSize,
            },
        };
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public TcpServer(
        ILogger logger,
        IPEndPoint endpoint,
        TcpServerConfig? serverConfig = default)
        : this(logger, endpoint.Address, endpoint.Port, serverConfig)
    {
    }

    public TcpServer(
        IPAddress ipAddress,
        int port,
        TcpServerConfig? serverConfig = default)
        : this(NullLogger.Instance, ipAddress, port, serverConfig)
    {
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public TcpServer(
        IPEndPoint endpoint,
        TcpServerConfig? serverConfig = default)
        : this(NullLogger.Instance, endpoint.Address, endpoint.Port, serverConfig)
    {
    }

    public IPAddress IpAddress { get; }

    public int Port { get; }

    /// <summary>
    /// Is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        _ = Task.Run(StartTcpListener, cancellationToken);
        return Task.Delay(TimeToWaitForStartTcpListenerInMs, cancellationToken);
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (!IsRunning)
        {
            LogTcpServerNotRunning();
            return Task.CompletedTask;
        }

        LogStopping(IpAddress.ToString(), Port);
        tcpListener?.Stop();
        IsRunning = false;
        LogStopped(IpAddress.ToString(), Port);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when data received.
    /// </summary>
    /// <param name="bytes">The received bytes.</param>
    protected virtual void OnDataReceived(
        byte[] bytes)
    { }

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

        tcpListener?.Server.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private async Task StartTcpListener()
    {
        if (IsRunning)
        {
            return;
        }

        LogStarting(IpAddress.ToString(), Port);
        if (tcpListener is null)
        {
            return;
        }

        var cancellationToken = CancellationToken.None;

        try
        {
            tcpListener.Start();
            IsRunning = true;
            LogStarted(IpAddress.ToString(), Port);

            while (IsRunning)
            {
                var serverClient = await tcpListener.AcceptTcpClientAsync(cancellationToken);

                while (IsRunning && serverClient.Connected)
                {
                    await HandleServerClientIncomingStream(serverClient, cancellationToken);
                }
            }
        }
        catch (SocketException)
        {
            IsRunning = false;
        }
    }

    private async Task HandleServerClientIncomingStream(
        System.Net.Sockets.TcpClient serverClient,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[serverConfig.ReceiveBufferSize];
        var memoryStream = new MemoryStream();
        byte[] receivedBuffer;
        bool messageHasEnded;

        do
        {
            var readCount = await serverClient
                .GetStream()
                .ReadAsync(
                    buffer.AsMemory(0, buffer.Length),
                    cancellationToken);

            if (readCount == 0)
            {
                serverClient.Dispose();
                return;
            }

            LogDataReceivedChunk(readCount);

            await memoryStream.WriteAsync(
                buffer.AsMemory(0, readCount),
                cancellationToken);

            receivedBuffer = memoryStream.ToArray();

            messageHasEnded = TerminationTypeHelper.HasTerminationType(
                serverConfig.TerminationType,
                receivedBuffer);
        }
        while (!messageHasEnded);

        LogDataReceived((int)memoryStream.Length);
        InvokeDataReceived(receivedBuffer);
    }

    private void InvokeDataReceived(
        byte[] data)
    {
        DataReceived?.Invoke(data);
        OnDataReceived(data);
    }
}