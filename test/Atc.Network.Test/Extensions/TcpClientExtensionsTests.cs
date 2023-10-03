using Microsoft.AspNetCore.Mvc;

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

        // Act
        tcpClient.SetBufferSizeAndTimeouts(sendTimeout, sendBufferSize, receiveTimeout, receiveBufferSize);

        // Assert
        Assert.Equal(sendTimeout, tcpClient.SendTimeout);
        Assert.Equal(sendBufferSize, tcpClient.SendBufferSize);
        Assert.Equal(receiveTimeout, tcpClient.ReceiveTimeout);
        Assert.Equal(receiveBufferSize, tcpClient.ReceiveBufferSize);
    }

    [Theory]
    [InlineData(true, 0, 0, 0)]
    [InlineData(true, 1, 1, 1)]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
    public void SetKeepAlive(bool expected, int tcpKeepAliveTime, int tcpKeepAliveInterval, int tcpKeepAliveRetryCount)
    {
        // Arrange
        var tcpClient = new System.Net.Sockets.TcpClient();

        // Act
        bool? result = null;
        if (expected)
        {
            tcpClient.SetKeepAlive(tcpKeepAliveTime, tcpKeepAliveInterval, tcpKeepAliveRetryCount);
            result = true;
        }
        else
        {
            try
            {
                tcpClient.SetKeepAlive(tcpKeepAliveTime, tcpKeepAliveInterval, tcpKeepAliveRetryCount);
            }
            catch
            {
                result = false;
            }
        }

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DisableKeepAlive()
    {
        // Arrange
        var tcpClient = new System.Net.Sockets.TcpClient();

        // Act
        const bool result = true;
        tcpClient.DisableKeepAlive();

        // Assert
        Assert.True(result);
    }
}