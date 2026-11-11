// ReSharper disable InvertIf
namespace Atc.Network.Udp;

/// <summary>
/// The main UdpServer - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:Element should not be on a single line", Justification = "OK.")]
public partial class UdpServer : IUdpServer
{
    private const int TimeToWaitForDisposeDisconnectionInMs = 50;
    private const int TimeToWaitForDataReceiverInMs = 150;

    private readonly UdpServerConfig serverConfig;
    private readonly ArraySegment<byte> receiveBufferSegment;

    private readonly Task? receiveListenerTask;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    private readonly Socket? socket;
    private NetworkInstrumentation? instrumentation;

    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    public event Action<byte[]>? DataReceived;

    /// <summary>
    /// Event to raise when a datagram is received, with the endpoint that sent it.
    /// </summary>
    public event EventHandler<UdpDatagramReceivedEventArgs>? DatagramReceived;

    /// <summary>
    /// Event to raise when an error was handled and logged, so the server keeps running:
    /// an event handler that threw, or a failed receive. A handler of this event that throws is only logged.
    /// </summary>
    public event EventHandler<NetworkErrorEventArgs>? ErrorOccurred;

    private UdpServer(
        ILogger logger,
        UdpServerConfig? serverConfig)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
        this.serverConfig = serverConfig ?? new UdpServerConfig();

        socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);

        socket.SetSocketOption(
            SocketOptionLevel.IP,
            SocketOptionName.ReuseAddress,
            optionValue: true);

        socket.SendTimeout = this.serverConfig.SendTimeout;
        socket.SendBufferSize = this.serverConfig.SendBufferSize;
        socket.ReceiveTimeout = this.serverConfig.ReceiveTimeout;
        socket.ReceiveBufferSize = this.serverConfig.ReceiveBufferSize;

        var receiveBuffer = new byte[this.serverConfig.ReceiveBufferSize];
        receiveBufferSegment = new ArraySegment<byte>(receiveBuffer);
    }

    public UdpServer(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        UdpServerConfig? serverConfig = default)
        : this(logger, serverConfig)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        var localPointFrom = new IPEndPoint(ipAddress, port);

        socket!.Bind(localPointFrom);

        receiveListenerTask = Task.Run(
            async () => await DataReceiver(cancellationTokenSource.Token),
            cancellationTokenSource.Token);
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public UdpServer(
        ILogger logger,
        IPEndPoint endpoint,
        UdpServerConfig? serverConfig = default)
        : this(logger, endpoint.Address, endpoint.Port, serverConfig)
    {
    }

    public UdpServer(
        IPAddress ipAddress,
        int port,
        UdpServerConfig? serverConfig = default)
        : this(NullLogger.Instance, ipAddress, port, serverConfig)
    {
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public UdpServer(
        IPEndPoint endpoint,
        UdpServerConfig? serverConfig = default)
        : this(NullLogger.Instance, endpoint.Address, endpoint.Port, serverConfig)
    {
    }

    /// <summary>
    /// Is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Gets the metrics and activities of this instance. Created on first use, when the address and port are known.
    /// </summary>
    private NetworkInstrumentation Instrumentation
        => LazyInitializer.EnsureInitialized(ref instrumentation, () => CreateInstrumentation());

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        IsRunning = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        IsRunning = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="recipient">The recipient endpoint.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task Send(
        EndPoint recipient,
        string data,
        CancellationToken cancellationToken)
        => Send(recipient, serverConfig.DefaultEncoding, data, cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="recipient">The recipient endpoint.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task Send(
        EndPoint recipient,
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
            recipient,
            encoding.GetBytes(data),
            serverConfig.TerminationType,
            cancellationToken);
    }

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="recipient">The recipient endpoint.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task Send(
        EndPoint recipient,
        byte[] data,
        CancellationToken cancellationToken)
        => Send(recipient, data, serverConfig.TerminationType, cancellationToken);

    /// <summary>
    /// Send data.
    /// </summary>
    /// <param name="recipient">The recipient endpoint.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="terminationType">The terminationType.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task Send(
        EndPoint recipient,
        byte[] data,
        TerminationType terminationType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!IsRunning)
        {
            LogUdpServerNotRunning();
            return;
        }

        AppendTerminationBytesIfNeeded(ref data, terminationType);

        var buffer = new ArraySegment<byte>(data);
        using var activity = Instrumentation.StartActivity("UdpServer.Send", ActivityKind.Internal);

        try
        {
            await socket!.SendToAsync(buffer, SocketFlags.None, recipient, cancellationToken);
            Instrumentation.Sent(data.Length);
        }
        catch (SocketException ex)
        {
            LogDataSendingSocketError(ex.SocketErrorCode.ToString(), ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
        catch (Exception ex)
        {
            LogDataSendingError(ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }
    }

    /// <summary>
    /// Called when data received.
    /// </summary>
    /// <param name="bytes">The received bytes.</param>
    protected virtual void OnDataReceived(byte[] bytes) { }

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

        if (!cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }

        cancellationTokenSource.Dispose();

        if (receiveListenerTask?.Status == TaskStatus.Running)
        {
            receiveListenerTask.Wait(TimeSpan.FromMilliseconds(TimeToWaitForDisposeDisconnectionInMs), CancellationToken.None);
        }

        if (socket is not null)
        {
            if (socket.Connected)
            {
                socket.Disconnect(reuseSocket: false);
            }

            socket.Dispose();
        }
    }

    private void InvokeDataReceived(
        byte[] data,
        IPEndPoint remoteEndPoint)
    {
        // Call each handler on its own, so one failing handler
        // neither skips the others nor stops the receive loop.
        var handlers = DataReceived;
        if (handlers is not null)
        {
            foreach (var handler in handlers.GetInvocationList().Cast<Action<byte[]>>())
            {
                InvokeHandler(nameof(DataReceived), () => handler(data));
            }
        }

        var datagramHandlers = DatagramReceived;
        if (datagramHandlers is not null)
        {
            var eventArgs = new UdpDatagramReceivedEventArgs(data, remoteEndPoint);
            foreach (var handler in datagramHandlers.GetInvocationList().Cast<EventHandler<UdpDatagramReceivedEventArgs>>())
            {
                InvokeHandler(nameof(DatagramReceived), () => handler(this, eventArgs));
            }
        }

        InvokeHandler(nameof(OnDataReceived), () => OnDataReceived(data));
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
                LogDataReceivedHandlerError(ex);
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
            LogDataReceivedHandlerError(ex);
            Instrumentation.HandlerFailed(eventName);
            InvokeError(NetworkErrorKind.EventHandler, ex, eventName);
        }
    }

    private NetworkInstrumentation CreateInstrumentation()
    {
        // The bound address and port, which is the actual port also when port 0 was asked for.
        var localEndPoint = socket?.LocalEndPoint as IPEndPoint;
        return new NetworkInstrumentation(
            "udp_server",
            localEndPoint?.Address.ToString() ?? string.Empty,
            localEndPoint?.Port ?? 0);
    }

    private static void AppendTerminationBytesIfNeeded(
        ref byte[] data,
        TerminationType terminationType)
    {
        if (terminationType != TerminationType.None)
        {
            var terminationTypeAsBytes = TerminationTypeHelper.ConvertToBytes(terminationType);
            if (data.Length >= terminationTypeAsBytes.Length)
            {
                var x = data[^terminationTypeAsBytes.Length..];
                if (!x.SequenceEqual(terminationTypeAsBytes))
                {
                    data = data
                        .Concat(terminationTypeAsBytes)
                        .ToArray();
                }
            }
        }
    }

    private async Task DataReceiver(
        CancellationToken cancellationToken = default)
    {
        var endPointFrom = new IPEndPoint(IPAddress.Any, 0);
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsRunning)
            {
                await Task.Delay(TimeToWaitForDataReceiverInMs, cancellationToken);
                continue;
            }

            try
            {
                await ReceiveAndHandleMessage(endPointFrom, cancellationToken);
            }
            catch (SocketException ex)
            {
                // For example ConnectionReset on Windows after a send to a closed port.
                // The socket is still usable, so log it and keep receiving.
                LogDataReceiveSocketError(ex.SocketErrorCode.ToString(), ex.Message);
                InvokeError(NetworkErrorKind.Receive, ex);
            }
        }
    }

    private async Task ReceiveAndHandleMessage(
        IPEndPoint endPointFrom,
        CancellationToken cancellationToken)
    {
        var res = await socket!.ReceiveMessageFromAsync(receiveBufferSegment, SocketFlags.None, endPointFrom);

        // Copy only the received bytes, not the whole receive buffer.
        var receivedBytes = receiveBufferSegment.AsSpan(0, res.ReceivedBytes).ToArray();

        LogDataReceived(receivedBytes.Length);
        Instrumentation.Received(receivedBytes.Length);
        InvokeDataReceived(receivedBytes, (IPEndPoint)res.RemoteEndPoint);

        if (!serverConfig.RespondToPing && !serverConfig.EchoOnReceivedData)
        {
            return;
        }

        var receivedStr = serverConfig.DefaultEncoding.GetString(receivedBytes);
        if (serverConfig.RespondToPing &&
            receivedStr.StartsWith("ping", StringComparison.OrdinalIgnoreCase))
        {
            await Send(res.RemoteEndPoint, "pong", cancellationToken);
        }
        else if (serverConfig.EchoOnReceivedData)
        {
            await Send(res.RemoteEndPoint, $"echo: {receivedStr}", cancellationToken);
        }
    }
}