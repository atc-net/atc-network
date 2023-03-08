// ReSharper disable AccessToDisposedClosure
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable PossibleMultipleEnumeration
namespace Atc.Network.Test.Tcp;

[Trait(Traits.Category, Traits.Categories.Integration)]
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public class TcpClientTests
{
    private const string ExternalTcpServer = "tcpbin.com";
    private const int ExternalTcpServerPort = 4242;
    private const string NonRoutableTcpServer = "192.100.255.100";
    private const int ConnectionTimeoutInMs = 2000;
    private const int ReceiveDataDelayInMs = 400;
    private const string TextToSend = "ping";
    private const TerminationType Termination = TerminationType.LineFeed;

    private readonly List<byte> receivedData = new();

    private bool isConnectedRaised;
    private bool isDisconnectedRaised;

    [Fact]
    public async Task Can_Connect_Successfully()
    {
        // Arrange
        using var tcpClient = new TcpClient(
            NullLogger<TcpClient>.Instance,
            ExternalTcpServer,
            ExternalTcpServerPort,
            new TcpClientConfig
            {
                TerminationType = Termination,
            });

        // Act & Assert
        var connectionSucceeded = await tcpClient.Connect();
        Assert.True(connectionSucceeded, "Could not connect to TCP server.");
        Assert.True(tcpClient.IsConnected, "IsConnected has wrong state.");

        await tcpClient.Disconnect();
        Assert.False(tcpClient.IsConnected, "IsConnected has wrong state.");
    }

    [Fact]
    public async Task Connect_To_NonRoutableIpAddress_Should_Indicate_Failure()
    {
        // Arrange
        using var tcpClient = new TcpClient(
            NullLogger<TcpClient>.Instance,
            NonRoutableTcpServer,
            ExternalTcpServerPort,
            new TcpClientConfig
            {
                TerminationType = Termination,
                ConnectTimeout = ConnectionTimeoutInMs,
            });

        // Act
        var connectionSucceeded = await tcpClient.Connect();

        // Assert
        Assert.False(connectionSucceeded, "Connection should not be possible to non routable ip-address.");
        Assert.False(tcpClient.IsConnected, "TcpClient should not be connected.");
    }

    [Fact]
    public async Task Connect_To_NonRoutableIpAddress_With_CancellationTokenTimeout_Should_Indicate_Failure()
    {
        // Arrange
        using var tcpClient = new TcpClient(
            NullLogger<TcpClient>.Instance,
            NonRoutableTcpServer,
            ExternalTcpServerPort,
            new TcpClientConfig
            {
                TerminationType = Termination,
            });

        using var cancellationTokenSource = new CancellationTokenSource(millisecondsDelay: 1000);
        var connectSuccessful = await tcpClient.Connect(cancellationTokenSource.Token);

        Assert.False(connectSuccessful, "Connection should not be possible.");
        Assert.False(tcpClient.IsConnected, "TcpClient should not be connected.");
    }

    [Fact]
    public async Task Can_Ping_External_Server()
    {
        // Arrange
        using var tcpClient = new TcpClient(
            NullLogger<TcpClient>.Instance,
            ExternalTcpServer,
            ExternalTcpServerPort,
            new TcpClientConfig
            {
                TerminationType = Termination,
            });

        tcpClient.Connected += OnConnected;
        tcpClient.Disconnected += OnDisconnected;
        tcpClient.DataReceived += OnDataReceived;

        // Act
        await tcpClient.Connect();

        await tcpClient.Send(TextToSend);

        await Task.Delay(ReceiveDataDelayInMs);

        await tcpClient.Disconnect();

        tcpClient.DataReceived -= OnDataReceived;
        tcpClient.Disconnected -= OnDisconnected;
        tcpClient.Connected -= OnConnected;

        // Assert
        Assert.True(isConnectedRaised);
        Assert.True(isDisconnectedRaised);

        Assert.NotEmpty(receivedData);
        var receivedText = Encoding.ASCII.GetString(receivedData.ToArray());
        Assert.Equal(TextToSend + "\n", receivedText);
    }

    [Fact]
    public async Task Can_Ping_External_Server_With_Small_ReceiveBuffer()
    {
        // Arrange
        using var tcpClient = new TcpClient(
            NullLogger<TcpClient>.Instance,
            ExternalTcpServer,
            ExternalTcpServerPort,
            new TcpClientConfig
            {
                ReceiveBufferSize = 5,
                TerminationType = Termination,
            });

        tcpClient.Connected += OnConnected;
        tcpClient.Disconnected += OnDisconnected;
        tcpClient.DataReceived += OnDataReceived;

        // Act
        await tcpClient.Connect();

        await tcpClient.Send(TextToSend);

        await Task.Delay(ReceiveDataDelayInMs);

        await tcpClient.Disconnect();

        tcpClient.DataReceived -= OnDataReceived;
        tcpClient.Disconnected -= OnDisconnected;
        tcpClient.Connected -= OnConnected;

        // Assert
        Assert.True(isConnectedRaised);
        Assert.True(isDisconnectedRaised);

        Assert.NotEmpty(receivedData);
        var receivedText = Encoding.ASCII.GetString(receivedData.ToArray());
        Assert.Equal(TextToSend + TerminationTypeHelper.ConvertToString(TerminationType.LineFeed), receivedText);
    }

    [Fact]
    public async Task Can_Send_And_Receive_Data_Successfully()
    {
        // Arrange
        var testChars = new[] { 'A', 'b', 'C', 'D', 'E', '0', '1', 'X', 'Y', 'z' };
        const int testDataLength = 20;

        using var tcpClient = new TcpClient(
            NullLogger<TcpClient>.Instance,
            ExternalTcpServer,
            ExternalTcpServerPort,
            new TcpClientConfig
            {
                TerminationType = Termination,
                ConnectTimeout = 1000,
            });

        // Act
        tcpClient.DataReceived += OnDataReceived;
        await tcpClient.Connect();

        var tasks = new List<Task>();
        using var barrier = new Barrier(testChars.Length);

        foreach (var testChar in testChars)
        {
            tasks.Add(Task.Run(async () =>
            {
                var data = Encoding.UTF8.GetBytes(new string(testChar, testDataLength));

                barrier.SignalAndWait();

                await tcpClient.Send(data);
            }));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(ReceiveDataDelayInMs);

        await tcpClient.Disconnect();
        tcpClient.DataReceived -= OnDataReceived;
        tcpClient.Disconnected -= OnDisconnected;
        tcpClient.Connected -= OnConnected;

        // Assert
        var byteArrayParts = receivedData
            .Split(TerminationTypeHelper.LineFeed)
            .ToList();

        Assert.Equal(
            byteArrayParts.TryGetNonEnumeratedCount(out var partsCount)
                ? partsCount
                : byteArrayParts.Count,
            testChars.Length);

        foreach (var part in byteArrayParts)
        {
            Assert.Equal(testDataLength, part.Length);
        }
    }

    [Fact]
    public async Task Can_Not_Ping_External_Server_With_Too_Small_ReceiveBuffer()
    {
        // Arrange
        using var tcpClient = new TcpClient(
            NullLogger<TcpClient>.Instance,
            ExternalTcpServer,
            ExternalTcpServerPort,
            new TcpClientConfig
            {
                ReceiveBufferSize = 2,
                TerminationType = Termination,
            });

        tcpClient.Connected += OnConnected;
        tcpClient.Disconnected += OnDisconnected;
        tcpClient.DataReceived += OnDataReceived;

        // Act
        await tcpClient.Connect();

        await tcpClient.Send(TextToSend);

        await Task.Delay(400);

        await tcpClient.Disconnect();

        tcpClient.DataReceived -= OnDataReceived;
        tcpClient.Disconnected -= OnDisconnected;
        tcpClient.Connected -= OnConnected;

        // Assert
        Assert.True(isConnectedRaised);
        Assert.True(isDisconnectedRaised);
        Assert.Empty(receivedData);
    }

    private void OnConnected()
    {
        isConnectedRaised = true;
    }

    private void OnDisconnected()
    {
        isDisconnectedRaised = true;
    }

    private void OnDataReceived(
        byte[] data)
    {
        receivedData.AddRange(data);
    }
}