namespace Atc.Network.Test.Extensions;

[SuppressMessage("Blocker Bug", "S2930:\"IDisposables\" should be disposed", Justification = "OK.")]
[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "OK.")]
[Trait(Traits.Category, Traits.Categories.Integration)]
[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public class TcpClientExtensionsTests
{
    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(100, 0, 0, 0)]
    [InlineData(0, 100, 0, 0)]
    [InlineData(0, 0, 100, 0)]
    [InlineData(0, 0, 0, 100)]
    public void SetBufferSizeAndTimeouts(int sendTimeout, int sendBufferSize, int receiveTimeout, int receiveBufferSize)
    {
        // Arrange
        var tcpClient = new System.Net.Sockets.TcpClient();

        // Atc
        tcpClient.SetBufferSizeAndTimeouts(sendTimeout, sendBufferSize, receiveTimeout, receiveBufferSize);

        // Assert
        Assert.Equal(sendTimeout, tcpClient.SendTimeout);
        Assert.Equal(sendBufferSize, tcpClient.SendBufferSize);
        Assert.Equal(receiveTimeout, tcpClient.ReceiveTimeout);
        Assert.Equal(receiveBufferSize, tcpClient.ReceiveBufferSize);
    }
}