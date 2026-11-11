namespace Atc.Network.Test.Tcp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class TcpClientMessageFramingTests : IDisposable
{
    private readonly System.Net.Sockets.TcpListener listener;

    public TcpClientMessageFramingTests()
    {
        listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
    }

    private int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    [Fact]
    public async Task NoFraming_TwoMessagesInOneWrite_AreDeliveredAsOneRead()
    {
        // Arrange
        using var tcpClient = CreateClient(new TcpClientConfig { TerminationType = TerminationType.LineFeed });
        var received = Collect(tcpClient);
        using var serverSide = await Connect(tcpClient);

        // Act
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes("a\nbc\n"));

        // Assert - the default is unchanged: one event per read.
        Assert.True(await WaitHelper.WaitUntil(() => !received.IsEmpty), "The data should be received.");
        await Task.Delay(200);
        Assert.Equal(["a\nbc\n"], received, StringComparer.Ordinal);
    }

    [Fact]
    public async Task TerminationTypeFraming_SplitsAndJoinsReadsIntoMessages()
    {
        // Arrange
        using var tcpClient = CreateClient(new TcpClientConfig
        {
            TerminationType = TerminationType.LineFeed,
            MessageFraming = TcpMessageFraming.TerminationType,
        });
        var received = Collect(tcpClient);
        using var serverSide = await Connect(tcpClient);
        var stream = serverSide.GetStream();

        // Act - two messages in one write, then one message in two writes.
        await stream.WriteAsync(Encoding.ASCII.GetBytes("a\nbc\n"));
        await Task.Delay(100);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("de"));
        await Task.Delay(100);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("f\n"));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Count == 3), $"Expected 3 messages, got {received.Count}.");
        Assert.Equal(["a\n", "bc\n", "def\n"], received, StringComparer.Ordinal);
    }

    [Fact]
    public async Task TerminationTypeFraming_TwoByteTerminatorSplitAcrossReads_EndsTheMessage()
    {
        // Arrange
        using var tcpClient = CreateClient(new TcpClientConfig
        {
            TerminationType = TerminationType.CarriageReturnLineFeed,
            MessageFraming = TcpMessageFraming.TerminationType,
        });
        var received = Collect(tcpClient);
        using var serverSide = await Connect(tcpClient);
        var stream = serverSide.GetStream();

        // Act
        await stream.WriteAsync(Encoding.ASCII.GetBytes("abc\r"));
        await Task.Delay(150);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("\nx"));

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => !received.IsEmpty), "The message should be received.");
        await Task.Delay(200);
        Assert.Equal(["abc\r\n"], received, StringComparer.Ordinal);
    }

    [Fact]
    public async Task LengthPrefixFraming_ReceivesMessagesWithoutTheLength()
    {
        // Arrange
        using var tcpClient = CreateClient(new TcpClientConfig { MessageFraming = TcpMessageFraming.LengthPrefix });
        var received = Collect(tcpClient);
        using var serverSide = await Connect(tcpClient);
        var stream = serverSide.GetStream();

        // Act - the second length prefix is split across two writes.
        await stream.WriteAsync((byte[])[0, 0, 0, 3, (byte)'a', (byte)'b', (byte)'c', 0, 0]);
        await Task.Delay(150);
        await stream.WriteAsync((byte[])[0, 2, (byte)'d', (byte)'e']);

        // Assert
        Assert.True(await WaitHelper.WaitUntil(() => received.Count == 2), $"Expected 2 messages, got {received.Count}.");
        Assert.Equal(["abc", "de"], received, StringComparer.Ordinal);
    }

    [Fact]
    public async Task LengthPrefixFraming_SendAddsTheLength()
    {
        // Arrange
        using var tcpClient = CreateClient(new TcpClientConfig
        {
            TerminationType = TerminationType.LineFeed,
            MessageFraming = TcpMessageFraming.LengthPrefix,
        });
        using var serverSide = await Connect(tcpClient);

        // Act
        await tcpClient.Send("hello");

        // Assert - the length, then the data, without a terminator.
        var buffer = new byte[64];
        using var cts = new CancellationTokenSource(3000);
        var readCount = await serverSide.GetStream().ReadAtLeastAsync(buffer, 9, throwOnEndOfStream: false, cts.Token);
        Assert.Equal((byte[])[0, 0, 0, 5, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o'], buffer[..readCount]);
    }

    [Fact]
    public async Task LengthPrefixFraming_MessageLongerThanMaxMessageSize_RaisesReceiveErrorAndDisconnects()
    {
        // Arrange
        using var tcpClient = CreateClient(new TcpClientConfig
        {
            MessageFraming = TcpMessageFraming.LengthPrefix,
            MaxMessageSize = 100,
        });
        var errors = new ConcurrentQueue<NetworkErrorEventArgs>();
        tcpClient.ErrorOccurred += (_, e) => errors.Enqueue(e);
        using var serverSide = await Connect(tcpClient);

        // Act - announce a 1000-byte message.
        await serverSide.GetStream().WriteAsync((byte[])[0, 0, 3, 232]);

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => errors.Any(x => x.Kind == NetworkErrorKind.Receive && x.Exception is InvalidDataException)),
            "The oversized message should be reported.");
        Assert.True(await WaitHelper.WaitUntil(() => !tcpClient.IsConnected), "The client should disconnect.");
    }

    [Fact]
    public async Task TerminationTypeFraming_MessageLongerThanMaxMessageSize_RaisesReceiveError()
    {
        // Arrange
        using var tcpClient = CreateClient(new TcpClientConfig
        {
            TerminationType = TerminationType.LineFeed,
            MessageFraming = TcpMessageFraming.TerminationType,
            MaxMessageSize = 100,
        });
        var errors = new ConcurrentQueue<NetworkErrorEventArgs>();
        tcpClient.ErrorOccurred += (_, e) => errors.Enqueue(e);
        using var serverSide = await Connect(tcpClient);

        // Act - 200 bytes without a terminator.
        await serverSide.GetStream().WriteAsync(Encoding.ASCII.GetBytes(new string('x', 200)));

        // Assert
        Assert.True(
            await WaitHelper.WaitUntil(() => errors.Any(x => x.Kind == NetworkErrorKind.Receive && x.Exception is InvalidDataException)),
            "The oversized message should be reported.");
    }

    [Fact]
    public void TerminationTypeFraming_WithoutTerminationType_Throws()
        => Assert.Throws<ArgumentException>(() => CreateClient(new TcpClientConfig { MessageFraming = TcpMessageFraming.TerminationType }));

    public void Dispose()
    {
        listener.Stop();
        listener.Dispose();
    }

    private static ConcurrentQueue<string> Collect(TcpClient tcpClient)
    {
        var received = new ConcurrentQueue<string>();
        tcpClient.DataReceived += data => received.Enqueue(Encoding.ASCII.GetString(data));
        return received;
    }

    private TcpClient CreateClient(TcpClientConfig config)
        => new(
            NullLogger.Instance,
            IPAddress.Loopback,
            Port,
            config,
            new TcpClientReconnectConfig { Enable = false });

    private async Task<System.Net.Sockets.TcpClient> Connect(
        TcpClient tcpClient)
    {
        var acceptTask = listener.AcceptTcpClientAsync();
        Assert.True(await tcpClient.Connect());
        return await acceptTask;
    }
}