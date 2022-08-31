// ReSharper disable InvertIf
namespace Atc.Network.Udp;

/// <summary>
/// The main UdpServer - Handles call execution.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
public partial class UdpServer : IUdpServer
{
    private const int TimeToWaitForDisposeDisconnectionInMs = 50;
    private readonly UdpServerConfig udpServerConfig;
    private readonly Socket? socket;
    private readonly ArraySegment<byte> receiveBufferSegment;
    private readonly Task? receiveListenerTask;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    /// <summary>
    /// Event to raise when data has become available from the server.
    /// </summary>
    public event Action<byte[]>? DataReceived;

    private UdpServer(
        ILogger logger,
        UdpServerConfig? udpServerConfig)
    {
        ArgumentNullException.ThrowIfNull(logger);

        this.logger = logger;
        this.udpServerConfig = udpServerConfig ?? new UdpServerConfig();

        socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);

        socket.SetSocketOption(
            SocketOptionLevel.IP,
            SocketOptionName.ReuseAddress,
            optionValue: true);

        var receiveBuffer = new byte[this.udpServerConfig.ReceiveBufferSize];
        receiveBufferSegment = new ArraySegment<byte>(receiveBuffer);
    }

    public UdpServer(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        UdpServerConfig? udpServerConfig = default)
        : this(logger, udpServerConfig)
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
        UdpServerConfig? udpServerConfig = default)
        : this(logger, endpoint.Address, endpoint.Port, udpServerConfig)
    {
    }

    public UdpServer(
        IPAddress ipAddress,
        int port,
        UdpServerConfig? udpServerConfig = default)
        : this(NullLogger.Instance, ipAddress, port, udpServerConfig)
    {
    }

    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
    public UdpServer(
        IPEndPoint endpoint,
        UdpServerConfig? udpServerConfig = default)
        : this(NullLogger.Instance, endpoint.Address, endpoint.Port, udpServerConfig)
    {
    }

    /// <summary>
    /// Is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        IsRunning = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
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
        => Send(recipient, udpServerConfig.DefaultEncoding, data, cancellationToken);

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
            udpServerConfig.TerminationType,
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
        => Send(recipient, data, udpServerConfig.TerminationType, cancellationToken);

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
            LogServiceNotRunning();
            return;
        }

        AppendTerminationBytesIfNeeded(ref data, terminationType);

        var buffer = new ArraySegment<byte>(data);
        await socket!.SendToAsync(buffer, SocketFlags.None, recipient, cancellationToken);
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

        if (receiveListenerTask?.Status == TaskStatus.Running)
        {
            receiveListenerTask.Wait(TimeSpan.FromMilliseconds(TimeToWaitForDisposeDisconnectionInMs));
        }

        if (socket is not null)
        {
            if (socket.Connected)
            {
                socket.Disconnect(false);
            }

            socket.Dispose();
        }
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
                continue;
            }

            var res = await socket!.ReceiveMessageFromAsync(receiveBufferSegment, SocketFlags.None, endPointFrom);
            var receivedBytes = new byte[res.ReceivedBytes];
            Array.Copy(receiveBufferSegment.ToArray(), 0, receivedBytes, 0, res.ReceivedBytes);

            DataReceived?.Invoke(receivedBytes);

            var receivedStr = udpServerConfig.DefaultEncoding.GetString(receivedBytes);
            if (receivedStr.StartsWith("ping", StringComparison.OrdinalIgnoreCase))
            {
                await Send(res.RemoteEndPoint, "pong", cancellationToken);
            }
            else if (udpServerConfig.EchoOnReceivedData)
            {
                await Send(res.RemoteEndPoint, $"echo: {receivedStr}", cancellationToken);
            }
        }
    }
}