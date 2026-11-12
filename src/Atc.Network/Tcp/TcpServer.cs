namespace Atc.Network.Tcp;

/// <summary>
/// The main TcpServer - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:Element should not be on a single line", Justification = "OK.")]
public partial class TcpServer : ITcpServer
{
    private const int TlsHandshakeTimeoutInMs = 10_000;

    private readonly TcpServerConfig serverConfig;
    private readonly TcpListener? tcpListener;
    private readonly string ipAddressStr;
    private readonly ConcurrentDictionary<Guid, TcpServerConnection> clients = new();
    private NetworkInstrumentation? instrumentation;

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

    /// <summary>
    /// Event to raise when an error was handled and logged, so the server keeps running:
    /// an event handler that threw, a client connection that failed, or the listener stopping. A handler of this event that throws is only logged.
    /// </summary>
    public event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

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

        IPEndPointHelper.ThrowIfDualModeWithoutIPv6(this.serverConfig.DualMode, ipAddress, nameof(serverConfig));

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

        if (this.serverConfig.DualMode)
        {
            tcpListener.Server.DualMode = true;
        }
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
    /// Gets the metrics and activities of this instance. Created on first use, when the address and port are known.
    /// </summary>
    private NetworkInstrumentation Instrumentation
        => LazyInitializer.EnsureInitialized(ref instrumentation, () => new NetworkInstrumentation("tcp_server", ipAddressStr, Port));

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

        using var activity = Instrumentation.StartActivity("TcpServer.Send", ActivityKind.Internal);
        var lockAcquired = false;
        try
        {
            await connection.SendLock.WaitAsync(cancellationToken);
            lockAcquired = true;

            await connection.Stream.WriteAsync(data, cancellationToken);
            Instrumentation.Sent(data.Length);
            return true;
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or InvalidOperationException)
        {
            LogClientConnectionError(ipAddressStr, Port, ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
        finally
        {
            if (lockAcquired)
            {
                connection.SendLock.Release();
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

    private async Task AcceptClients()
    {
        var cancellationToken = CancellationToken.None;

        try
        {
            while (IsRunning)
            {
                var serverClient = await tcpListener!.AcceptTcpClientAsync(cancellationToken);

                // Each client is set up (including any TLS handshake) and read in its own task,
                // so the next client is accepted right away.
                _ = Task.Run(() => HandleServerClient(serverClient, cancellationToken), CancellationToken.None);
            }
        }
        catch (Exception ex) when ((ex is SocketException or ObjectDisposedException) && !IsRunning)
        {
            // StopAsync stopped the listener.
        }
        catch (Exception ex)
        {
            LogListenerError(ipAddressStr, Port, ex);
            InvokeError(NetworkErrorKind.Listener, ex);
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
        System.Net.Sockets.TcpClient serverClient,
        CancellationToken cancellationToken)
    {
        var connection = await CreateConnection(serverClient);
        if (connection is null)
        {
            return;
        }

        clients[connection.ClientId] = connection;
        Instrumentation.ConnectionOpened();

        var connectedEventArgs = new TcpServerClientEventArgs(connection.ClientId, connection.RemoteEndPoint);
        InvokeHandlers(
            ClientConnected,
            nameof(ClientConnected),
            handler => ((EventHandler<TcpServerClientEventArgs>)handler)(this, connectedEventArgs));

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
            InvokeError(NetworkErrorKind.ClientConnection, ex);
        }
        finally
        {
            clients.TryRemove(connection.ClientId, out _);
            Instrumentation.ConnectionClosed();
            connection.Dispose();

            var eventArgs = new TcpServerClientEventArgs(connection.ClientId, connection.RemoteEndPoint);
            InvokeHandlers(
                ClientDisconnected,
                nameof(ClientDisconnected),
                handler => ((EventHandler<TcpServerClientEventArgs>)handler)(this, eventArgs));
        }
    }

    /// <summary>
    /// Sets up a newly accepted client and, when <see cref="TcpServerConfig.SslOptions"/> is set,
    /// runs the TLS handshake. Returns <see langword="null"/> after logging and closing the client
    /// when it can't be served, for example because the handshake failed or timed out.
    /// </summary>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The stream is owned by the returned connection, or disposed below on failure.")]
    private async Task<TcpServerConnection?> CreateConnection(
        System.Net.Sockets.TcpClient serverClient)
    {
        Stream? stream = null;
        try
        {
            stream = serverClient.GetStream();

            var sslOptions = serverConfig.SslOptions;
            if (sslOptions is not null)
            {
                var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
                stream = sslStream;

                using var timeoutCts = new CancellationTokenSource(TlsHandshakeTimeoutInMs);
                await sslStream.AuthenticateAsServerAsync(sslOptions, timeoutCts.Token);
            }

            if (IsRunning)
            {
                return new TcpServerConnection(Guid.NewGuid(), serverClient, stream, serverConfig.ReceiveBufferSize);
            }
        }
        catch (Exception ex)
        {
            Instrumentation.ConnectionFailed();
            InvokeError(NetworkErrorKind.ClientConnection, ex);
            if (serverConfig.SslOptions is null)
            {
                LogClientConnectionError(ipAddressStr, Port, ex.Message);
            }
            else
            {
                LogClientTlsHandshakeError(ipAddressStr, Port, ex.Message);
            }
        }

        // The client failed, or the server stopped while the client was being set up.
        if (stream is not null)
        {
            await stream.DisposeAsync();
        }

        serverClient.Dispose();
        return null;
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
            var readCount = await connection.Stream
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
        Instrumentation.Received((int)memoryStream.Length);
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
                InvokeDataReceivedHandler(nameof(DataReceived), () => handler(data));
            }
        }

        var clientHandlers = ClientDataReceived;
        if (clientHandlers is not null)
        {
            var eventArgs = new TcpServerDataReceivedEventArgs(connection.ClientId, connection.RemoteEndPoint, data);
            foreach (var handler in clientHandlers.GetInvocationList().Cast<EventHandler<TcpServerDataReceivedEventArgs>>())
            {
                InvokeDataReceivedHandler(nameof(ClientDataReceived), () => handler(this, eventArgs));
            }
        }

        InvokeDataReceivedHandler(nameof(OnDataReceived), () => OnDataReceived(data));
    }

    private void InvokeDataReceivedHandler(
        string eventName,
        Action invoke)
    {
        try
        {
            invoke();
        }
        catch (Exception ex)
        {
            LogDataReceivedHandlerError(ipAddressStr, Port, ex);
            Instrumentation.HandlerFailed(eventName);
            InvokeError(NetworkErrorKind.EventHandler, ex, eventName);
        }
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
                LogEventHandlerError(ipAddressStr, Port, nameof(ErrorOccurred), ex);
                Instrumentation.HandlerFailed(nameof(ErrorOccurred));
            }
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
                Instrumentation.HandlerFailed(eventName);
                InvokeError(NetworkErrorKind.EventHandler, ex, eventName);
            }
        }
    }
}