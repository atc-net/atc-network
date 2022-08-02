using Atc.Network.Tcp;

namespace Atc.Network.Test.Tcp;

public class TcpClientTests
{
    private const string ExternalTcpServer = "tcpbin.com";
    private const int ExternalTcpServerPort = 4242;
    private const string TextToSend = "ping";
    private const TcpTerminationType TerminationType = TcpTerminationType.LineFeed;

    private bool isConnectedRaised;
    private bool isDisconnectedRaised;
    private byte[] receivedData = Array.Empty<byte>();

    [Fact]
    public async Task Can_Ping_External_Server()
    {
        using var tcpClient = new TcpClient(
            NullLogger<TcpClient>.Instance,
            ExternalTcpServer,
            ExternalTcpServerPort,
            new TcpClientConfig
            {
                TerminationType = TerminationType,
            });

        tcpClient.Connected += OnConnected;
        tcpClient.Disconnected += OnDisconnected;

        tcpClient.DataReceived += OnDataReceived;
        await tcpClient.Connect();

        await tcpClient.Send(TextToSend);

        await Task.Delay(400);

        await tcpClient.Disconnect();

        tcpClient.DataReceived -= OnDataReceived;
        tcpClient.Disconnected -= OnDisconnected;
        tcpClient.Connected -= OnConnected;

        Assert.True(isConnectedRaised);
        Assert.True(isDisconnectedRaised);

        Assert.NotEmpty(receivedData);
        var receivedText = Encoding.ASCII.GetString(receivedData);
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
        receivedData = data;
    }
}