namespace Atc.Network.Tcp;

/// <summary>
/// The main TcpServer - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:Element should not be on a single line", Justification = "OK.")]
public partial class TcpServer : ITcpServer
{
    private readonly TcpServerConfig serverConfig;
    private readonly TcpListener? tcpListener;
    private readonly string ipAddressStr;
    private readonly ConcurrentDictionary<Guid, TcpServerConnection> clients = new();

    /// <summary>
    /// Event to raise when a message is received from any client.
    /// </summary>
    public event Action<byte[]>? DataReceived;

    /// <summary>
    /// Event to raise when a message is received, with the client that sent it.
    /// </summary>
    public event EventHandler<TcpServerDataReceivedEventArgs>? ClientDataReceived;

    /// <summary>
    /// Event to raise when a client connects.
    /// </summary>
    public event EventHandler<TcpServerClientEventArgs>? ClientConnected;

    /// <summary>
    /// Event to raise when a client disconnects or is closed by the server.
    /// </summary>
    public event EventHandler<TcpServerClientEventArgs>? ClientDisconnected;

    private TcpServer(
        ILogger logger,
        TcpServerConfig? serverConfig)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
        this.serverConfig = serverConfig ?? new TcpServerConfig();

        IpAddress = IPAddress.Any;
        Port = -1;
        ipAddressStr = string.Empty;
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public TcpServer(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        TcpServerConfig? serverConfig = default)
        : this(logger, serverConfig)
    {
        IpAddress = ipAddress;
        Port = port;
        ipAddressStr = ipAddress.ToString();

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
    /// <exception cref="SocketException">The listener could not be started, for example because the port is in use.</exception>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsRunning || tcpListener is null)
        {
            return Task.CompletedTask;
        }

        LogStarting(ipAddressStr, Port);

        try
        {
            // Start (bind) here, so a port that is in use fails StartAsync instead of a background task.
            tcpListener.Start();
        }
        catch (SocketException ex)
        {
            LogListenerError(ipAddressStr, Port, ex);
            throw;
        }

        IsRunning = true;
        LogStarted(ipAddressStr, Port);

        _ = Task.Run(AcceptClients, CancellationToken.None);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// Stops accepting clients and closes the connected ones.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!IsRunning)
        {
            LogTcpServerNotRunning(ipAddressStr, Port);
            return Task.CompletedTask;
        }

        LogStopping(ipAddressStr, Port);

        // Clear IsRunning first, so the accept loop treats the stop as expected.
        IsRunning = false;
        tcpListener?.Stop();
        CloseClients();
        LogStopped(ipAddressStr, Port);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Send text to one connected client, encoded with <see cref="TcpConfigBase.DefaultEncoding"/>.
    /// </summary>
    /// <param name="clientId">The client id from <see cref="ClientConnected"/> or <see cref="ClientDataReceived"/>.</param>
    /// <param name="data">The text to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns><see langword="true"/> if the data was sent; <see langword="false"/> if the client is not connected or the send failed.</returns>
    public Task<bool> Send(
        Guid clientId,
        string data,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);

        return Send(clientId, serverConfig.DefaultEncoding.GetBytes(data), cancellationToken);
    }

    /// <summary>
    /// Send data to one connected client. The configured <see cref="TcpConfigBase.TerminationType"/> is appended if missing.
    /// </summary>
    /// <param name="clientId">The client id from <see cref="ClientConnected"/> or <see cref="ClientDataReceived"/>.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns><see langword="true"/> if the data was sent; <see langword="false"/> if the client is not connected or the send failed.</returns>
    public async Task<bool> Send(
        Guid clientId,
        byte[] data,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!clients.TryGetValue(clientId, out var connection))
        {
            return false;
        }

        TerminationHelper.AppendTerminationBytesIfNeeded(ref data, serverConfig.TerminationType);

        var lockAcquired = false;
        try
        {
            await connection.SendLock.WaitAsync(cancellationToken);
            lockAcquired = true;

            await connection.Client.GetStream().WriteAsync(data, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or InvalidOperationException)
        {
            LogClientConnectionError(ipAddressStr, Port, ex.Message);
            return false;
        }
        finally
        {
            if (lockAcquired)
            {
                ReleaseSendLock(connection);
            }
        }
    }

    /// <summary>
    /// Called when data received.
    /// </summary>
    /// <param name="bytes">The received bytes.</param>
    protected virtual void OnDataReceived(byte[] bytes)
    { }

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

        IsRunning = false;
        CloseClients();
        tcpListener?.Server.Dispose();
        tcpListener?.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static void ReleaseSendLock(TcpServerConnection connection)
    {
        try
        {
            connection.SendLock.Release();
        }
        catch (ObjectDisposedException)
        {
            // The client was closed while sending.
        }
    }

    private async Task AcceptClients()
    {
        var cancellationToken = CancellationToken.None;

        try
        {
            while (IsRunning)
            {
                var serverClient = await tcpListener!.AcceptTcpClientAsync(cancellationToken);
                var connection = new TcpServerConnection(Guid.NewGuid(), serverClient, serverConfig.ReceiveBufferSize);
                clients[connection.ClientId] = connection;

                var eventArgs = new TcpServerClientEventArgs(connection.ClientId, connection.RemoteEndPoint);
                InvokeHandlers(
                    ClientConnected,
                    nameof(ClientConnected),
                    handler => ((EventHandler<TcpServerClientEventArgs>)handler)(this, eventArgs));

                // Each client is read in its own task, so the next client is accepted right away.
                _ = Task.Run(() => HandleServerClient(connection, cancellationToken), CancellationToken.None);
            }
        }
        catch (Exception ex) when ((ex is SocketException or ObjectDisposedException) && !IsRunning)
        {
            // StopAsync stopped the listener.
        }
        catch (Exception ex)
        {
            LogListenerError(ipAddressStr, Port, ex);
        }
        finally
        {
            IsRunning = false;
        }
    }

    /// <summary>
    /// Reads from one client until it disconnects. A failing client connection
    /// is logged and closed, so the server goes on serving the other clients.
    /// </summary>
    private async Task HandleServerClient(
        TcpServerConnection connection,
        CancellationToken cancellationToken)
    {
        try
        {
            while (IsRunning && connection.Client.Connected)
            {
                await HandleServerClientIncomingStream(connection, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // The client task runs in the background, so log every failure here.
            LogClientConnectionError(ipAddressStr, Port, ex.Message);
        }
        finally
        {
            clients.TryRemove(connection.ClientId, out _);
            connection.Dispose();

            var eventArgs = new TcpServerClientEventArgs(connection.ClientId, connection.RemoteEndPoint);
            InvokeHandlers(
                ClientDisconnected,
                nameof(ClientDisconnected),
                handler => ((EventHandler<TcpServerClientEventArgs>)handler)(this, eventArgs));
        }
    }

    /// <summary>
    /// Closes every connected client. Each client's own task then ends and raises <see cref="ClientDisconnected"/>.
    /// </summary>
    private void CloseClients()
    {
        foreach (var connection in clients.Values)
        {
            connection.Client.Dispose();
        }
    }

    private async Task HandleServerClientIncomingStream(
        TcpServerConnection connection,
        CancellationToken cancellationToken)
    {
        var buffer = connection.ReceiveBuffer;
        using var memoryStream = new MemoryStream();
        bool messageHasEnded;

        do
        {
            var readCount = await connection.Client
                .GetStream()
                .ReadAsync(
                    buffer.AsMemory(0, buffer.Length),
                    cancellationToken);

            if (readCount == 0)
            {
                connection.Client.Dispose();
                return;
            }

            LogDataReceivedChunk(ipAddressStr, Port, readCount);

            var previousLength = (int)memoryStream.Length;
            await memoryStream.WriteAsync(
                buffer.AsMemory(0, readCount),
                cancellationToken);

            // The terminator was not in the earlier reads, so only look at this read plus the
            // byte before it (a two-byte terminator such as CR LF can be split across two reads).
            var searchStart = System.Math.Max(0, previousLength - 1);
            var newData = memoryStream
                .GetBuffer()
                .AsSpan(searchStart, (int)memoryStream.Length - searchStart)
                .ToArray();

            messageHasEnded = TerminationTypeHelper.HasTerminationType(
                serverConfig.TerminationType,
                newData);
        }
        while (!messageHasEnded);

        LogDataReceived(ipAddressStr, Port, (int)memoryStream.Length);
        InvokeDataReceived(memoryStream.ToArray(), connection);
    }

    private void InvokeDataReceived(
        byte[] data,
        TcpServerConnection connection)
    {
        // Call each handler on its own, so one failing handler
        // neither skips the others nor stops the server.
        var handlers = DataReceived;
        if (handlers is not null)
        {
            foreach (var handler in handlers.GetInvocationList().Cast<Action<byte[]>>())
            {
                InvokeDataReceivedHandler(() => handler(data));
            }
        }

        var clientHandlers = ClientDataReceived;
        if (clientHandlers is not null)
        {
            var eventArgs = new TcpServerDataReceivedEventArgs(connection.ClientId, connection.RemoteEndPoint, data);
            foreach (var handler in clientHandlers.GetInvocationList().Cast<EventHandler<TcpServerDataReceivedEventArgs>>())
            {
                InvokeDataReceivedHandler(() => handler(this, eventArgs));
            }
        }

        InvokeDataReceivedHandler(() => OnDataReceived(data));
    }

    private void InvokeDataReceivedHandler(Action invoke)
    {
        try
        {
            invoke();
        }
        catch (Exception ex)
        {
            LogDataReceivedHandlerError(ipAddressStr, Port, ex);
        }
    }

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
            try
            {
                invoke(handler);
            }
            catch (Exception ex)
            {
                LogEventHandlerError(ipAddressStr, Port, eventName, ex);
            }
        }
    }
}