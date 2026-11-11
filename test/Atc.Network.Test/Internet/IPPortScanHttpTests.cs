namespace Atc.Network.Test.Internet;

/// <summary>
/// Tests for the HTTP/HTTPS reachability check against small in-process servers on loopback.
/// </summary>
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class IPPortScanHttpTests : IDisposable
{
    private const string UnauthorizedResponse = "HTTP/1.1 401 Unauthorized\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";

    private readonly System.Net.Sockets.TcpListener listener;

    public IPPortScanHttpTests()
    {
        listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
    }

    private int ListenerPort => ((IPEndPoint)listener.LocalEndpoint).Port;

    [Fact]
    public async Task CanConnectWithHttp_ServerAnswersWithError_ReturnsTrue()
    {
        // Arrange
        var serverTask = AnswerFirstRequest(useTls: false);
        using var ipPortScan = new IPPortScan(IPAddress.Loopback, timeoutInMs: 2000);

        // Act
        var actual = await ipPortScan.CanConnectWithHttp(ListenerPort, CancellationToken.None);
        await serverTask;

        // Assert
        Assert.True(actual, "Any HTTP response, also 401, means an HTTP server answered.");
    }

    [Fact]
    public async Task CanConnectWithHttps_SelfSignedCertificate_ReturnsTrue()
    {
        // Arrange
        var serverTask = AnswerFirstRequest(useTls: true);
        using var ipPortScan = new IPPortScan(IPAddress.Loopback, timeoutInMs: 5000);

        // Act
        var actual = await ipPortScan.CanConnectWithHttps(ListenerPort, CancellationToken.None);
        await serverTask;

        // Assert
        Assert.True(actual, "A device with a self-signed certificate is still reachable over HTTPS.");
    }

    [Fact]
    public async Task CanConnectWithHttp_ServerNeverAnswers_ReturnsFalseAndDropsTheRequest()
    {
        // Arrange - the server accepts and reads the request, but never answers.
        var connectionClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Run(async () =>
        {
            using var serverSide = await listener.AcceptTcpClientAsync();
            var buffer = new byte[4096];
            try
            {
                while (await serverSide.GetStream().ReadAsync(buffer) > 0)
                {
                    // Keep reading until the client closes the connection.
                }
            }
            catch (IOException)
            {
                // A reset also means the client dropped the request.
            }

            connectionClosed.TrySetResult(true);
        });

        using var ipPortScan = new IPPortScan(IPAddress.Loopback, timeoutInMs: 300);

        // Act
        var actual = await ipPortScan.CanConnectWithHttp(ListenerPort, CancellationToken.None);

        // Assert
        Assert.False(actual);
        var closedInTime = await WaitHelper.WaitUntil(() => connectionClosed.Task.IsCompleted);
        Assert.True(closedInTime, "The request should be dropped once the timeout has passed.");
    }

    public void Dispose()
    {
        listener.Stop();
        listener.Dispose();
    }

    /// <summary>
    /// Accepts one connection, reads the request headers and answers with 401.
    /// A client that rejects the connection (for example the TLS certificate) is not an error here;
    /// the test asserts on the client's result instead.
    /// </summary>
    private async Task AnswerFirstRequest(bool useTls)
    {
        try
        {
            await AnswerFirstRequestCore(useTls);
        }
        catch (Exception ex) when (ex is IOException or AuthenticationException)
        {
            // The client hung up.
        }
    }

    private async Task AnswerFirstRequestCore(bool useTls)
    {
        using var serverSide = await listener.AcceptTcpClientAsync();
        Stream stream = serverSide.GetStream();

        using var certificate = useTls ? CertificateHelper.CreateSelfSignedCertificate() : null;
        if (certificate is not null)
        {
            var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
            await sslStream.AuthenticateAsServerAsync(certificate);
            stream = sslStream;
        }

        await using (stream)
        {
            var buffer = new byte[4096];
            var request = new StringBuilder();
            while (!request.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                var read = await stream.ReadAsync(buffer);
                if (read == 0)
                {
                    return;
                }

                request.Append(Encoding.ASCII.GetString(buffer, 0, read));
            }

            await stream.WriteAsync(Encoding.ASCII.GetBytes(UnauthorizedResponse));
            await stream.FlushAsync();
        }
    }
}