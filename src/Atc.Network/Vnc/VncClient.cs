// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable InvertIf
// ReSharper disable LocalizableElement
namespace Atc.Network.Vnc;

/// <summary>
/// The main VncClient - Handles VNC/RFB protocol communication.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
public partial class VncClient : IVncClient
{
    private const int TimeToWaitForDisconnectionInMs = 200;
    private const int TimeToWaitForDisposeDisconnectionInMs = 50;

    private readonly SemaphoreSlim syncLock = new(1, 1);
    private readonly VncClientConfig clientConfig;
    private readonly VncClientReconnectConfig clientReconnectConfig;
    private readonly VncClientKeepAliveConfig clientKeepAliveConfig;
    private readonly IVncInputPolicy inputPolicy;
    private readonly RfbProtocol rfb;
    private readonly IO.ZrleCompressedReader zrleReader;
    private readonly int syncLockTimeoutInMs;

    private System.Net.Sockets.TcpClient? tcpClient;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? updateListenerTask;
    private string? storedPassword;
    private int reconnectRetryCounter;
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
    /// Event to raise when a framebuffer update is received.
    /// </summary>
    public event EventHandler<VncFramebufferUpdateEventArgs>? FramebufferUpdated;

    /// <summary>
    /// Event to raise when the server sends a bell notification.
    /// </summary>
    public event Action? BellReceived;

    /// <summary>
    /// Event to raise when the server sends clipboard text.
    /// </summary>
    public event Action<string>? ServerCutText;

    /// <summary>
    /// Event to raise when the connection is lost unexpectedly.
    /// </summary>
    public event Action? ConnectionLost;

    private VncClient(
        ILogger logger,
        VncClientConfig? clientConfig,
        VncClientReconnectConfig? reconnectConfig,
        VncClientKeepAliveConfig? keepAliveConfig)
    {
        this.logger = logger;
        this.clientConfig = clientConfig ?? new VncClientConfig();
        clientReconnectConfig = reconnectConfig ?? new VncClientReconnectConfig();
        clientKeepAliveConfig = keepAliveConfig ?? new VncClientKeepAliveConfig();
        inputPolicy = this.clientConfig.ViewOnly
            ? new VncViewInputPolicy()
            : new VncDefaultInputPolicy();

        rfb = new RfbProtocol();
        zrleReader = new IO.ZrleCompressedReader();

        syncLockTimeoutInMs = this.clientConfig.ConnectTimeout <= 0
            ? VncConstants.DefaultConnectTimeout
            : this.clientConfig.ConnectTimeout + VncConstants.GracePeriodTimeout;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VncClient"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="hostname">The hostname or IP address.</param>
    /// <param name="port">The port number.</param>
    /// <param name="clientConfig">The client configuration.</param>
    /// <param name="reconnectConfig">The reconnect configuration.</param>
    /// <param name="keepAliveConfig">The keep-alive configuration.</param>
    public VncClient(
        ILogger logger,
        string hostname,
        int port,
        VncClientConfig? clientConfig = default,
        VncClientReconnectConfig? reconnectConfig = default,
        VncClientKeepAliveConfig? keepAliveConfig = default)
        : this(logger, clientConfig, reconnectConfig, keepAliveConfig)
    {
        if (string.IsNullOrEmpty(hostname))
        {
            throw new ArgumentNullException(nameof(hostname));
        }

        IPAddressOrHostname = hostname;
        Port = port;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VncClient"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="port">The port number.</param>
    /// <param name="clientConfig">The client configuration.</param>
    /// <param name="reconnectConfig">The reconnect configuration.</param>
    /// <param name="keepAliveConfig">The keep-alive configuration.</param>
    public VncClient(
        ILogger logger,
        IPAddress ipAddress,
        int port,
        VncClientConfig? clientConfig = default,
        VncClientReconnectConfig? reconnectConfig = default,
        VncClientKeepAliveConfig? keepAliveConfig = default)
        : this(logger, clientConfig, reconnectConfig, keepAliveConfig)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        IPAddressOrHostname = ipAddress.ToString();
        Port = port;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VncClient"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="ipEndpoint">The IP endpoint.</param>
    /// <param name="clientConfig">The client configuration.</param>
    /// <param name="reconnectConfig">The reconnect configuration.</param>
    /// <param name="keepAliveConfig">The keep-alive configuration.</param>
    public VncClient(
        ILogger logger,
        IPEndPoint ipEndpoint,
        VncClientConfig? clientConfig = default,
        VncClientReconnectConfig? reconnectConfig = default,
        VncClientKeepAliveConfig? keepAliveConfig = default)
        : this(logger, clientConfig, reconnectConfig, keepAliveConfig)
    {
        ArgumentNullException.ThrowIfNull(ipEndpoint);

        IPAddressOrHostname = ipEndpoint.Address.ToString();
        Port = ipEndpoint.Port;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VncClient"/> class.
    /// </summary>
    /// <param name="hostname">The hostname or IP address.</param>
    /// <param name="port">The port number.</param>
    /// <param name="clientConfig">The client configuration.</param>
    /// <param name="reconnectConfig">The reconnect configuration.</param>
    /// <param name="keepAliveConfig">The keep-alive configuration.</param>
    public VncClient(
        string hostname,
        int port,
        VncClientConfig? clientConfig = default,
        VncClientReconnectConfig? reconnectConfig = default,
        VncClientKeepAliveConfig? keepAliveConfig = default)
        : this(NullLogger.Instance, hostname, port, clientConfig, reconnectConfig, keepAliveConfig)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VncClient"/> class.
    /// </summary>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="port">The port number.</param>
    /// <param name="clientConfig">The client configuration.</param>
    /// <param name="reconnectConfig">The reconnect configuration.</param>
    /// <param name="keepAliveConfig">The keep-alive configuration.</param>
    public VncClient(
        IPAddress ipAddress,
        int port,
        VncClientConfig? clientConfig = default,
        VncClientReconnectConfig? reconnectConfig = default,
        VncClientKeepAliveConfig? keepAliveConfig = default)
        : this(NullLogger.Instance, ipAddress, port, clientConfig, reconnectConfig, keepAliveConfig)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VncClient"/> class.
    /// </summary>
    /// <param name="ipEndpoint">The IP endpoint.</param>
    /// <param name="clientConfig">The client configuration.</param>
    /// <param name="reconnectConfig">The reconnect configuration.</param>
    /// <param name="keepAliveConfig">The keep-alive configuration.</param>
    public VncClient(
        IPEndPoint ipEndpoint,
        VncClientConfig? clientConfig = default,
        VncClientReconnectConfig? reconnectConfig = default,
        VncClientKeepAliveConfig? keepAliveConfig = default)
        : this(NullLogger.Instance, ipEndpoint, clientConfig, reconnectConfig, keepAliveConfig)
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
    public bool IsConnected
    {
        get => isConnected;
        private set => isConnected = value;
    }

    /// <summary>
    /// Gets the framebuffer after initialization.
    /// </summary>
    public VncFramebuffer? Framebuffer { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the client is in view-only mode.
    /// </summary>
    public bool ViewOnly => clientConfig.ViewOnly;

    /// <summary>
    /// Connect to the VNC server.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public async Task<bool> Connect(
        CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            return false;
        }

        LogConnecting(IPAddressOrHostname, Port);
        InvokeConnectionStateChanged(ConnectionState.Connecting);

        try
        {
            tcpClient = new System.Net.Sockets.TcpClient();

            using var timeoutCts = new CancellationTokenSource(clientConfig.ConnectTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await tcpClient.ConnectAsync(IPAddressOrHostname, Port, linkedCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new InvalidOperationException("Connection timed out");
            }

            if (clientKeepAliveConfig.Enable)
            {
                tcpClient.SetKeepAlive(
                    clientKeepAliveConfig.Time,
                    clientKeepAliveConfig.Interval,
                    clientKeepAliveConfig.RetryCount);
            }

            tcpClient.SendTimeout = clientConfig.SendTimeout;
            tcpClient.ReceiveTimeout = clientConfig.ReceiveTimeout;

            var networkStream = tcpClient.GetStream();
            rfb.Initialize(networkStream);

            // Perform RFB protocol handshake
            var serverVersion = rfb.ReadProtocolVersion();
            if (!serverVersion.StartsWith("RFB", StringComparison.Ordinal))
            {
                LogProtocolVersionError(IPAddressOrHostname, Port, serverVersion.Trim());
                throw new InvalidOperationException($"Unsupported protocol version: {serverVersion.Trim()}");
            }

            rfb.WriteProtocolVersion();

            IsConnected = true;
            LogConnected(IPAddressOrHostname, Port);
            InvokeConnected();
            InvokeConnectionStateChanged(ConnectionState.Connected);

            return true;
        }
        catch (Exception ex)
        {
            LogConnectionError(IPAddressOrHostname, Port, ex.Message);
            InvokeConnectionStateChanged(ConnectionState.ConnectionFailed, ex.Message);

            CleanupTcpClient();
            return false;
        }
    }

    /// <summary>
    /// Authenticate with the VNC server using a password.
    /// </summary>
    /// <param name="password">The VNC password.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task<bool> Authenticate(
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
        {
            LogClientNotConnected(IPAddressOrHostname, Port);
            return Task.FromResult(false);
        }

        LogAuthenticating(IPAddressOrHostname, Port);

        try
        {
            var securityTypes = rfb.ReadSecurityTypes();
            var selectedType = SelectSecurityType(securityTypes);

            rfb.WriteSecurityType(selectedType);

            if (selectedType == (byte)Enums.VncSecurityType.VncAuthentication)
            {
                var challenge = rfb.ReadChallenge();
                var response = EncryptChallenge(password, challenge);
                rfb.WriteChallenge(response);
            }

            var result = rfb.ReadSecurityResult();
            if (result != 0)
            {
                var reason = string.Empty;
                try
                {
                    reason = rfb.ReadFailureReason();
                }
                catch
                {
                    // Some servers don't send a reason
                }

                var errorMsg = string.IsNullOrEmpty(reason) ? "Authentication failed" : reason;
                LogAuthenticationFailed(IPAddressOrHostname, Port, errorMsg);
                return Task.FromResult(false);
            }

            storedPassword = password;

            LogAuthenticated(IPAddressOrHostname, Port);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            LogAuthenticationFailed(IPAddressOrHostname, Port, ex.Message);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Initialize the VNC session after authentication.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task Initialize(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
        {
            LogClientNotConnected(IPAddressOrHostname, Port);
            throw new InvalidOperationException("Client is not connected.");
        }

        LogInitializing(IPAddressOrHostname, Port);

        rfb.WriteClientInit(clientConfig.SharedDesktop);
        var serverInit = rfb.ReadServerInit();

        var pixelFormat = VncPixelFormat.Create(clientConfig.BitsPerPixel, clientConfig.Depth);

        Framebuffer = new VncFramebuffer(
            serverInit.Width,
            serverInit.Height,
            serverInit.DesktopName,
            pixelFormat);

        // Tell the server our preferred pixel format
        rfb.WriteSetPixelFormat(pixelFormat);

        // Tell the server which encodings we support
        var encodings = new[]
        {
            (int)Enums.VncEncoding.Zrle,
            (int)Enums.VncEncoding.Hextile,
            (int)Enums.VncEncoding.CopyRect,
            (int)Enums.VncEncoding.Rre,
            (int)Enums.VncEncoding.CoRre,
            (int)Enums.VncEncoding.Raw,
        };
        rfb.WriteSetEncodings(encodings);

        LogInitialized(IPAddressOrHostname, Port, serverInit.DesktopName, serverInit.Width, serverInit.Height);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Start receiving framebuffer updates.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public Task StartUpdates(CancellationToken cancellationToken = default)
    {
        if (!IsConnected || Framebuffer is null)
        {
            throw new InvalidOperationException("Client must be connected and initialized before starting updates.");
        }

        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Request initial full screen update
        rfb.WriteFramebufferUpdateRequest(
            incremental: false,
            x: 0,
            y: 0,
            width: (ushort)Framebuffer.Width,
            height: (ushort)Framebuffer.Height);

        updateListenerTask = Task.Run(
            () => UpdateListener(cancellationTokenSource.Token),
            cancellationTokenSource.Token);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Request a full screen update.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task RequestFullScreenUpdate(
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected || Framebuffer is null)
        {
            return;
        }

        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(syncLockTimeoutInMs, cancellationToken);
            if (!lockAcquired)
            {
                return;
            }

            rfb.WriteFramebufferUpdateRequest(
                incremental: false,
                x: 0,
                y: 0,
                width: (ushort)Framebuffer.Width,
                height: (ushort)Framebuffer.Height);
        }
        finally
        {
            if (lockAcquired)
            {
                syncLock.Release();
            }
        }
    }

    /// <summary>
    /// Send a key event to the server.
    /// </summary>
    /// <param name="keysym">The X11 keysym value.</param>
    /// <param name="pressed">True if the key is pressed, false if released.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task SendKeyEvent(
        uint keysym,
        bool pressed,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected || !inputPolicy.AllowKeyboardInput)
        {
            return;
        }

        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(syncLockTimeoutInMs, cancellationToken);
            if (!lockAcquired)
            {
                return;
            }

            rfb.WriteKeyEvent(keysym, pressed);
        }
        finally
        {
            if (lockAcquired)
            {
                syncLock.Release();
            }
        }
    }

    /// <summary>
    /// Send a pointer (mouse) event to the server.
    /// </summary>
    /// <param name="buttonMask">The button state mask.</param>
    /// <param name="x">The X position.</param>
    /// <param name="y">The Y position.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task SendPointerEvent(
        byte buttonMask,
        int x,
        int y,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected || !inputPolicy.AllowPointerInput)
        {
            return;
        }

        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(syncLockTimeoutInMs, cancellationToken);
            if (!lockAcquired)
            {
                return;
            }

            rfb.WritePointerEvent(buttonMask, (ushort)x, (ushort)y);
        }
        finally
        {
            if (lockAcquired)
            {
                syncLock.Release();
            }
        }
    }

    /// <summary>
    /// Send clipboard text to the server.
    /// </summary>
    /// <param name="text">The clipboard text.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    public async Task SendClientCutText(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!IsConnected || !inputPolicy.AllowClipboardTransfer)
        {
            return;
        }

        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(syncLockTimeoutInMs, cancellationToken);
            if (!lockAcquired)
            {
                return;
            }

            rfb.WriteClientCutText(text);
        }
        finally
        {
            if (lockAcquired)
            {
                syncLock.Release();
            }
        }
    }

    /// <summary>
    /// Disconnect from the VNC server.
    /// </summary>
    public async Task Disconnect()
    {
        if (!IsConnected)
        {
            return;
        }

        storedPassword = null;

        LogDisconnecting(IPAddressOrHostname, Port);
        InvokeConnectionStateChanged(ConnectionState.Disconnecting);

        await Task.Delay(TimeToWaitForDisconnectionInMs);

        DisposeCancellationTokenAndTask();
        CleanupTcpClient();

        IsConnected = false;

        LogDisconnected(IPAddressOrHostname, Port);
        InvokeDisconnected();
        InvokeConnectionStateChanged(ConnectionState.Disconnected);
    }

    /// <summary>
    /// Called when connection is established.
    /// </summary>
    protected virtual void OnConnected()
    {
    }

    /// <summary>
    /// Called when connection is destroyed.
    /// </summary>
    protected virtual void OnDisconnected()
    {
    }

    /// <summary>
    /// Called when connection state is changed.
    /// </summary>
    /// <param name="connectionState">The connection state.</param>
    /// <param name="errorMessage">The error message.</param>
    protected virtual void OnConnectionStateChanged(
        ConnectionState connectionState,
        string? errorMessage = null)
    {
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
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        storedPassword = null;

        DisposeCancellationTokenAndTask();
        CleanupTcpClient();

        rfb.Dispose();
        zrleReader.Dispose();
        syncLock.Dispose();
    }

    private static byte SelectSecurityType(byte[] securityTypes)
    {
        // Prefer VNC authentication, fall back to None
        foreach (var t in securityTypes)
        {
            if (t == (byte)Enums.VncSecurityType.VncAuthentication)
            {
                return t;
            }
        }

        foreach (var t in securityTypes)
        {
            if (t == (byte)Enums.VncSecurityType.None)
            {
                return t;
            }
        }

        throw new InvalidOperationException("No supported security type found.");
    }

    [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "Required by VNC/RFB protocol specification.")]
    [SuppressMessage("Security", "CA5358:Review cipher mode usage", Justification = "ECB mode required by VNC/RFB protocol specification.")]
    [SuppressMessage("Security", "SCS0010:Weak cipher algorithm", Justification = "DES required by VNC/RFB protocol specification.")]
    [SuppressMessage("Security", "SCS0013:Potential usage of weak CipherMode", Justification = "ECB mode required by VNC/RFB protocol specification.")]
    [SuppressMessage("Security", "S5547:Use a strong cipher algorithm", Justification = "DES required by VNC/RFB protocol specification.")]
    [SuppressMessage("Security", "CA5401:Do not use CreateEncryptor with non-default IV", Justification = "ECB mode does not use IV; null IV is correct.")]
    [SuppressMessage("Security", "S3329:Use a dynamically-generated, random IV", Justification = "ECB mode does not use IV; required by VNC/RFB protocol.")]
    private static byte[] EncryptChallenge(
        string password,
        byte[] challenge)
    {
        // VNC uses a specific key derivation where bits in each byte are reversed
        var key = new byte[8];
        var passwordBytes = Encoding.ASCII.GetBytes(password);

        for (var i = 0; i < 8; i++)
        {
            if (i < passwordBytes.Length)
            {
                key[i] = ReverseBits(passwordBytes[i]);
            }
        }

        // Encrypt the challenge using DES with ECB mode (VNC protocol requirement).
        // Use CreateEncryptor(key, null) to bypass the weak key validation, as VNC
        // passwords may produce keys that .NET considers "weak" (e.g., empty password).
        using var des = System.Security.Cryptography.DES.Create();
        des.Mode = System.Security.Cryptography.CipherMode.ECB;
        des.Padding = System.Security.Cryptography.PaddingMode.None;

        var response = new byte[VncConstants.ChallengeLength];
        using var encryptor = des.CreateEncryptor(key, null);
        encryptor.TransformBlock(challenge, 0, 8, response, 0);
        encryptor.TransformBlock(challenge, 8, 8, response, 8);

        return response;
    }

    private static byte ReverseBits(byte value)
    {
        byte result = 0;
        for (var i = 0; i < 8; i++)
        {
            result = (byte)((result << 1) | (value & 1));
            value >>= 1;
        }

        return result;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private async Task UpdateListener(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && IsConnected)
        {
            try
            {
                var messageType = rfb.ReadServerMessageType();

                switch ((Enums.VncServerMessageType)messageType)
                {
                    case Enums.VncServerMessageType.FramebufferUpdate:
                        HandleFramebufferUpdate();
                        break;

                    case Enums.VncServerMessageType.SetColourMapEntries:
                        rfb.ReadSetColourMapEntries();
                        break;

                    case Enums.VncServerMessageType.Bell:
                        LogBellReceived(IPAddressOrHostname, Port);
                        BellReceived?.Invoke();
                        break;

                    case Enums.VncServerMessageType.ServerCutText:
                        var text = rfb.ReadServerCutText();
                        LogServerCutTextReceived(IPAddressOrHostname, Port);
                        ServerCutText?.Invoke(text);
                        break;

                    default:
                        LogServerMessageError(IPAddressOrHostname, Port, $"Unknown message type: {messageType}");
                        break;
                }

                // Request next incremental update (serialized with other writes)
                if (IsConnected && Framebuffer is not null)
                {
                    var updateLockAcquired = false;
                    try
                    {
                        updateLockAcquired = await syncLock.WaitAsync(syncLockTimeoutInMs, cancellationToken);
                        if (updateLockAcquired)
                        {
                            rfb.WriteFramebufferUpdateRequest(
                                incremental: true,
                                x: 0,
                                y: 0,
                                width: (ushort)Framebuffer.Width,
                                height: (ushort)Framebuffer.Height);
                        }
                    }
                    finally
                    {
                        if (updateLockAcquired)
                        {
                            syncLock.Release();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (EndOfStreamException)
            {
                await HandleConnectionLost();
                break;
            }
            catch (IOException)
            {
                await HandleConnectionLost();
                break;
            }
            catch (Exception ex)
            {
                LogServerMessageError(IPAddressOrHostname, Port, ex.Message);
                await HandleConnectionLost();
                break;
            }
        }
    }

    private void HandleFramebufferUpdate()
    {
        if (Framebuffer is null)
        {
            return;
        }

        try
        {
            var rectangleCount = rfb.ReadFramebufferUpdateHeader();
            LogFramebufferUpdateReceived(IPAddressOrHostname, Port, rectangleCount);

            for (var i = 0; i < rectangleCount; i++)
            {
                var (rectangle, encodingType) = rfb.ReadFramebufferUpdateRectangleHeader();

                var encodedRect = EncodedRectangleFactory.Create(
                    rfb,
                    Framebuffer,
                    rectangle,
                    encodingType,
                    zrleReader);

                encodedRect.Decode();

                FramebufferUpdated?.Invoke(this, new VncFramebufferUpdateEventArgs(rectangle, Framebuffer));
            }
        }
        catch (Exception ex)
        {
            LogFramebufferUpdateError(IPAddressOrHostname, Port, ex.Message);
            throw;
        }
    }

    private async Task HandleConnectionLost()
    {
        if (!IsConnected)
        {
            return;
        }

        IsConnected = false;

        if (clientReconnectConfig.Enable && storedPassword is not null)
        {
            try
            {
                await DoReconnect();
            }
            catch (Exception)
            {
                InvokeConnectionStateChanged(ConnectionState.Disconnected);
                InvokeDisconnected();
                ConnectionLost?.Invoke();
            }
        }
        else
        {
            InvokeConnectionStateChanged(ConnectionState.Disconnected);
            InvokeDisconnected();
            ConnectionLost?.Invoke();
        }
    }

    private async Task DoReconnect()
    {
        LogReconnecting(IPAddressOrHostname, Port);
        InvokeConnectionStateChanged(ConnectionState.Reconnecting);

        DisposeCancellationTokenAndTask();
        CleanupTcpClient();
        IsConnected = false;

        await Task.Delay(clientReconnectConfig.RetryInterval);

        if (await DoConnectAuthenticateAndInitialize())
        {
            reconnectRetryCounter = 0;
            LogReconnected(IPAddressOrHostname, Port);
            InvokeConnectionStateChanged(ConnectionState.Reconnected);
        }
        else
        {
            if (reconnectRetryCounter < clientReconnectConfig.RetryMaxAttempts)
            {
                LogReconnectionWarning(IPAddressOrHostname, Port, reconnectRetryCounter, clientReconnectConfig.RetryMaxAttempts);
                InvokeConnectionStateChanged(ConnectionState.ReconnectionFailed);

                reconnectRetryCounter++;

                // ReSharper disable once TailRecursiveCall
                await DoReconnect();
            }
            else
            {
                LogReconnectionMaxRetryExceededError(IPAddressOrHostname, Port);
                InvokeConnectionStateChanged(ConnectionState.ReconnectionFailed);
                InvokeDisconnected();
                ConnectionLost?.Invoke();
            }
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK - reconnect must not throw.")]
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private async Task<bool> DoConnectAuthenticateAndInitialize()
    {
        try
        {
            tcpClient = new System.Net.Sockets.TcpClient();

            var connectTimeoutTask = Task.Delay(clientConfig.ConnectTimeout);
            var connectTask = tcpClient.ConnectAsync(IPAddressOrHostname, Port);

            await await Task.WhenAny(connectTask, connectTimeoutTask);

            if (connectTimeoutTask.IsCompleted)
            {
                CleanupTcpClient();
                return false;
            }

            if (clientKeepAliveConfig.Enable)
            {
                tcpClient.SetKeepAlive(
                    clientKeepAliveConfig.Time,
                    clientKeepAliveConfig.Interval,
                    clientKeepAliveConfig.RetryCount);
            }

            tcpClient.SendTimeout = clientConfig.SendTimeout;
            tcpClient.ReceiveTimeout = clientConfig.ReceiveTimeout;

            var networkStream = tcpClient.GetStream();
            rfb.Initialize(networkStream);

            // RFB protocol handshake
            var serverVersion = rfb.ReadProtocolVersion();
            if (!serverVersion.StartsWith("RFB", StringComparison.Ordinal))
            {
                CleanupTcpClient();
                return false;
            }

            rfb.WriteProtocolVersion();

            IsConnected = true;

            // Re-authenticate
            if (storedPassword is not null)
            {
                var authResult = await Authenticate(storedPassword);
                if (!authResult)
                {
                    CleanupTcpClient();
                    IsConnected = false;
                    return false;
                }
            }

            // Re-initialize
            await Initialize();

            // Restart updates
            await StartUpdates();

            return true;
        }
        catch (Exception)
        {
            CleanupTcpClient();
            IsConnected = false;
            return false;
        }
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

        if (updateListenerTask is not null)
        {
            if (updateListenerTask.Status == TaskStatus.Running)
            {
                updateListenerTask.Wait(TimeSpan.FromMilliseconds(TimeToWaitForDisposeDisconnectionInMs));
            }

            updateListenerTask = null;
        }
    }

    private void CleanupTcpClient()
    {
        if (tcpClient is not null)
        {
            tcpClient.Close();
            tcpClient.Dispose();
            tcpClient = null;
        }
    }
}