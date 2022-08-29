namespace Atc.Network.Test.Udp;

[Trait(Traits.Category, Traits.Categories.Integration)]
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public class UdpClientTests
{
    private const string ExternalUdpServer = "tcpbin.org";
    private const int ExternalUdpServerPort = 40000;
    private const int ReceiveDataDelayInMs = 400;
    private const string TextToSend = "ping";
    private readonly List<byte> receivedData = new();

    private bool isConnectedRaised;
    private bool isDisconnectedRaised;

    [Fact]
    public async Task Can_Connect_Successfully()
    {
        // Arrange
        using var udpClient = new UdpClient(
            NullLogger<UdpClient>.Instance,
            ExternalUdpServer,
            ExternalUdpServerPort);

        // Act & Assert
        var connectionSucceeded = await udpClient.Connect();
        Assert.True(connectionSucceeded, "Could not connect to TCP server.");
        Assert.True(udpClient.IsConnected, "IsConnected has wrong state.");

        await udpClient.Disconnect();
        Assert.False(udpClient.IsConnected, "IsConnected has wrong state.");
    }

    [Fact]
    public async Task Can_Ping_External_Server()
    {
        // Arrange
        using var udpClient = new UdpClient(
            NullLogger<TcpClient>.Instance,
            ExternalUdpServer,
            ExternalUdpServerPort);

        udpClient.Connected += OnConnected;
        udpClient.Disconnected += OnDisconnected;
        udpClient.DataReceived += OnDataReceived;

        // Act
        await udpClient.Connect();

        await udpClient.Send(TextToSend);

        await Task.Delay(ReceiveDataDelayInMs);

        await udpClient.Disconnect();

        udpClient.DataReceived -= OnDataReceived;
        udpClient.Disconnected -= OnDisconnected;
        udpClient.Connected -= OnConnected;

        // Assert
        Assert.True(isConnectedRaised);
        Assert.True(isDisconnectedRaised);

        Assert.NotEmpty(receivedData);
        var receivedText = Encoding.ASCII.GetString(receivedData.ToArray());
        Assert.Equal(TextToSend + "\n", receivedText);
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