namespace Atc.Network.Test.Tcp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class TcpTlsTests : IDisposable
{
    private readonly X509Certificate2 certificate = CertificateHelper.CreateSelfSignedCertificate();

    [Fact]
    public async Task TcpClient_WithSslOptions_ExchangesDataWithTlsServer()
    {
        // Arrange
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var done = new TaskCompletionSource();

        var serverTask = Task.Run(async () =>
        {
            using var serverClient = await listener.AcceptTcpClientAsync();
            await using var sslStream = new SslStream(serverClient.GetStream());
            await sslStream.AuthenticateAsServerAsync(certificate).WaitAsync(TimeSpan.FromSeconds(5));
            var request = await ReadText(sslStream);
            await sslStream.WriteAsync(Encoding.ASCII.GetBytes($"echo:{request}"));
            await done.Task.WaitAsync(TimeSpan.FromSeconds(5));
        });

        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpClientConfig { SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = CertificateHelper.TrustOnly(certificate) } },
            new TcpClientReconnectConfig { Enable = false });

        var received = new ConcurrentQueue<string>();
        tcpClient.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        // Act
        var connected = await tcpClient.Connect();
        await tcpClient.Send("hello");

        // Assert
        Assert.True(connected);
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("echo:hello", StringComparer.Ordinal)), "The TLS reply should be received and decrypted.");

        done.SetResult();
        await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        await tcpClient.Disconnect();
    }

    [Fact]
    public async Task TcpClient_WithSslOptions_UntrustedCertificate_FailsConnectWithoutRaisingConnected()
    {
        // Arrange - the server's certificate is self-signed, so default validation rejects it.
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var serverTask = Task.Run(async () =>
        {
            using var serverClient = await listener.AcceptTcpClientAsync();
            await using var sslStream = new SslStream(serverClient.GetStream());
            try
            {
                await sslStream.AuthenticateAsServerAsync(certificate).WaitAsync(TimeSpan.FromSeconds(5));
                await ReadText(sslStream);
            }
            catch (Exception ex) when (ex is IOException or AuthenticationException)
            {
                // The client rejects the certificate.
            }
        });

        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpClientConfig { SslOptions = new SslClientAuthenticationOptions() },
            new TcpClientReconnectConfig { Enable = false });

        var raisedConnected = false;
        var states = new ConcurrentQueue<ConnectionState>();
        tcpClient.Connected += () => raisedConnected = true;
        tcpClient.ConnectionStateChanged += (_, e) => states.Enqueue(e.State);

        // Act
        var connected = await tcpClient.Connect();

        // Assert
        Assert.False(connected);
        Assert.False(tcpClient.IsConnected);
        Assert.False(raisedConnected);
        Assert.Contains(ConnectionState.ConnectionFailed, states);
        Assert.DoesNotContain(ConnectionState.Connected, states);

        await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task TcpServer_WithSslOptions_ReceivesAndSendsOverTls()
    {
        // Arrange
        var port = GetFreePort();
        using var tcpServer = new TcpServer(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpServerConfig { SslOptions = new SslServerAuthenticationOptions { ServerCertificate = certificate } });

        // Send logs and returns false on connection errors, so this async handler does not throw.
        tcpServer.ClientDataReceived += async (_, e)
            => await tcpServer.Send(e.ClientId, $"reply to {Encoding.ASCII.GetString(e.Data.Span)}", CancellationToken.None);

        await tcpServer.StartAsync(CancellationToken.None);

        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        await using var sslStream = await AuthenticateAsClient(client);

        // Act
        await sslStream.WriteAsync(Encoding.ASCII.GetBytes("A"));

        // Assert
        Assert.Equal("reply to A", await ReadText(sslStream));

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_WithSslOptions_ClientThatNeverHandshakes_DoesNotBlockOthersAndIsNotConnected()
    {
        // Arrange
        var port = GetFreePort();
        using var tcpServer = new TcpServer(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpServerConfig { SslOptions = new SslServerAuthenticationOptions { ServerCertificate = certificate } });

        var connected = new ConcurrentQueue<TcpServerClientEventArgs>();
        var received = new ConcurrentQueue<string>();
        tcpServer.ClientConnected += (_, e) => connected.Enqueue(e);
        tcpServer.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        await tcpServer.StartAsync(CancellationToken.None);

        // Act - the first client connects but never starts the TLS handshake.
        using var silentClient = new System.Net.Sockets.TcpClient();
        await silentClient.ConnectAsync(IPAddress.Loopback, port);

        using var tlsClient = new System.Net.Sockets.TcpClient();
        await tlsClient.ConnectAsync(IPAddress.Loopback, port);
        await using var sslStream = await AuthenticateAsClient(tlsClient);
        await sslStream.WriteAsync(Encoding.ASCII.GetBytes("B"));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("B", StringComparer.Ordinal)), "The TLS client should be served.");
        var tlsClientPort = ((IPEndPoint)tlsClient.Client.LocalEndPoint).Port;
        Assert.Equal(tlsClientPort, Assert.Single(connected).RemoteEndPoint.Port);

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpServer_WithClientCertificateRequired_DoesNotConnectClientWithoutCertificate()
    {
        // Arrange
        var port = GetFreePort();
        using var tcpServer = new TcpServer(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpServerConfig
            {
                SslOptions = new SslServerAuthenticationOptions
                {
                    ServerCertificate = certificate,
                    ClientCertificateRequired = true,
                },
            });

        var connected = 0;
        tcpServer.ClientConnected += (_, _) => Interlocked.Increment(ref connected);
        await tcpServer.StartAsync(CancellationToken.None);

        // Act
        using var client = new System.Net.Sockets.TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);
        try
        {
            await using var sslStream = await AuthenticateAsClient(client);
            await sslStream.WriteAsync(Encoding.ASCII.GetBytes("A"));
            await ReadText(sslStream, timeoutInMs: 500);
        }
        catch (Exception ex) when (ex is IOException or AuthenticationException)
        {
            // The server rejects the client.
        }

        // Assert
        await Task.Delay(300);
        Assert.Equal(0, Volatile.Read(ref connected));
        Assert.True(tcpServer.IsRunning);

        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpClientAndTcpServer_WithSslOptions_ExchangeDataEndToEnd()
    {
        // Arrange
        var port = GetFreePort();
        using var tcpServer = new TcpServer(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpServerConfig
            {
                TerminationType = TerminationType.LineFeed,
                SslOptions = new SslServerAuthenticationOptions { ServerCertificate = certificate },
            });

        // Send logs and returns false on connection errors, so this async handler does not throw.
        tcpServer.ClientDataReceived += async (_, e)
            => await tcpServer.Send(e.ClientId, $"pong:{Encoding.ASCII.GetString(e.Data.Span).TrimEnd('\n')}", CancellationToken.None);

        await tcpServer.StartAsync(CancellationToken.None);

        var validations = 0;
        var trustOnly = CertificateHelper.TrustOnly(certificate);
        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            "localhost",
            port,
            new TcpClientConfig
            {
                TerminationType = TerminationType.LineFeed,
                SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = (sender, serverCertificate, chain, errors) =>
                    {
                        Interlocked.Increment(ref validations);
                        return trustOnly(sender, serverCertificate, chain, errors);
                    },
                },
            },
            new TcpClientReconnectConfig { Enable = false });

        var received = new ConcurrentQueue<string>();
        tcpClient.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        // Act
        Assert.True(await tcpClient.Connect());
        await tcpClient.Send("ping");

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("pong:ping\n", StringComparer.Ordinal)), "The server's TLS reply should reach the client.");
        Assert.Equal(1, Volatile.Read(ref validations));

        await tcpClient.Disconnect();
        await tcpServer.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TcpClient_WithSslOptions_ServerRestarts_ReconnectsWithANewHandshake()
    {
        // Arrange
        var port = GetFreePort();
        var listener = StartReusableListener(port);
        var states = new ConcurrentQueue<ConnectionState>();
        var received = new ConcurrentQueue<string>();
        var validations = 0;
        var trustOnly = CertificateHelper.TrustOnly(certificate);

        using var tcpClient = new TcpClient(
            NullLogger.Instance,
            IPAddress.Loopback,
            port,
            new TcpClientConfig
            {
                ConnectTimeout = 2000,
                SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = (sender, serverCertificate, chain, errors) =>
                    {
                        Interlocked.Increment(ref validations);
                        return trustOnly(sender, serverCertificate, chain, errors);
                    },
                },
            },
            new TcpClientReconnectConfig { Enable = true, RetryInterval = 50, RetryMaxAttempts = 50 });

        tcpClient.ConnectionStateChanged += (_, e) => states.Enqueue(e.State);
        tcpClient.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));

        var firstServerSide = AcceptTls(listener);
        Assert.True(await tcpClient.Connect());
        var (firstClient, firstStream) = await firstServerSide;

        // Act - the server goes away, then comes back on the same port.
        await firstStream.DisposeAsync();
        firstClient.Dispose();
        listener.Stop();
        listener.Dispose();
        Assert.True(await WaitHelper.WaitUntil(() => states.Contains(ConnectionState.Reconnecting)), "The client should start reconnecting.");

        using var restartedListener = StartReusableListener(port);
        var (secondClient, secondStream) = await AcceptTls(restartedListener);
        using var secondClientToDispose = secondClient;
        await using var secondStreamToDispose = secondStream;

        Assert.True(await WaitHelper.WaitUntil(() => states.Contains(ConnectionState.Reconnected), timeoutInMs: 10_000), "The client should reconnect.");
        await tcpClient.Send("again");

        // Assert
        Assert.Equal("again", await ReadText(secondStream));
        await secondStream.WriteAsync(Encoding.ASCII.GetBytes("echo:again"));
        Assert.True(await WaitHelper.WaitUntil(() => received.Contains("echo:again", StringComparer.Ordinal)), "The reply after reconnect should be received.");
        Assert.Equal(2, Volatile.Read(ref validations));

        await tcpClient.Disconnect();
    }

    [Fact]
    public void SslClientAuthenticationOptions_EverySettablePropertyIsCopiedByTcpClient()
    {
        // TcpClient copies the options by hand when TargetHost is empty.
        // A new settable property in .NET fails this test, so the copy can be updated.
        string[] copiedByTcpClient =
        [
            nameof(SslClientAuthenticationOptions.AllowRenegotiation),
            nameof(SslClientAuthenticationOptions.AllowRsaPkcs1Padding),
            nameof(SslClientAuthenticationOptions.AllowRsaPssPadding),
            nameof(SslClientAuthenticationOptions.AllowTlsResume),
            nameof(SslClientAuthenticationOptions.ApplicationProtocols),
            nameof(SslClientAuthenticationOptions.CertificateChainPolicy),
            nameof(SslClientAuthenticationOptions.CertificateRevocationCheckMode),
            nameof(SslClientAuthenticationOptions.CipherSuitesPolicy),
            nameof(SslClientAuthenticationOptions.ClientCertificateContext),
            nameof(SslClientAuthenticationOptions.ClientCertificates),
            nameof(SslClientAuthenticationOptions.EnabledSslProtocols),
            nameof(SslClientAuthenticationOptions.EncryptionPolicy),
            nameof(SslClientAuthenticationOptions.LocalCertificateSelectionCallback),
            nameof(SslClientAuthenticationOptions.RemoteCertificateValidationCallback),
            nameof(SslClientAuthenticationOptions.TargetHost),
        ];

        var settableProperties = typeof(SslClientAuthenticationOptions)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanWrite)
            .Select(x => x.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(copiedByTcpClient.Order(StringComparer.Ordinal), settableProperties, StringComparer.Ordinal);
    }

    public void Dispose()
        => certificate.Dispose();

    private static System.Net.Sockets.TcpListener StartReusableListener(
        int port)
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, port);

        // Allow restarting on the same port while the old connection is in TIME_WAIT.
        listener.Server.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, optionValue: true);
        listener.Start();
        return listener;
    }

    private async Task<(System.Net.Sockets.TcpClient Client, SslStream Stream)> AcceptTls(
        System.Net.Sockets.TcpListener listener)
    {
        var client = await listener.AcceptTcpClientAsync().WaitAsync(TimeSpan.FromSeconds(10));
        var sslStream = new SslStream(client.GetStream());
        await sslStream.AuthenticateAsServerAsync(certificate).WaitAsync(TimeSpan.FromSeconds(5));
        return (client, sslStream);
    }

    private async Task<SslStream> AuthenticateAsClient(
        System.Net.Sockets.TcpClient client)
    {
        var sslStream = new SslStream(client.GetStream());
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await sslStream.AuthenticateAsClientAsync(
            new SslClientAuthenticationOptions
            {
                TargetHost = "localhost",
                RemoteCertificateValidationCallback = CertificateHelper.TrustOnly(certificate),
            },
            cts.Token);

        return sslStream;
    }

    private static async Task<string?> ReadText(
        Stream stream,
        int timeoutInMs = 3000)
    {
        using var cts = new CancellationTokenSource(timeoutInMs);
        var buffer = new byte[1024];
        try
        {
            var readCount = await stream.ReadAsync(buffer, cts.Token);
            return readCount == 0 ? null : Encoding.ASCII.GetString(buffer, 0, readCount);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static int GetFreePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}