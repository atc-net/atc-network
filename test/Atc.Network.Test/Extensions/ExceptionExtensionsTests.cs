using System.Net.Sockets;

namespace Atc.Network.Test.Extensions;

[SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "OK.")]
public class ExceptionExtensionsTests
{
    [Theory]
    [InlineData(false, SocketError.Success)]
    [InlineData(true, SocketError.TimedOut)]
    [InlineData(true, SocketError.ConnectionReset)]
    [InlineData(false, SocketError.OperationAborted)]
    public void IsKnownExceptionForNetworkCableUnplugged(bool expected, SocketError socketError)
    {
        // Arrange
        var ex = GenerateExceptionForTest(socketError);

        // Act
        var (actualIsKnownException, actualSocketError) = ex.IsKnownExceptionForNetworkCableUnplugged();

        // Assert
        if (expected)
        {
            Assert.True(actualIsKnownException);
            Assert.NotNull(actualSocketError);
        }
        else
        {
            Assert.False(actualIsKnownException);
            Assert.Null(actualSocketError);
        }
    }

    [Theory]
    [InlineData(false, SocketError.Success)]
    [InlineData(false, SocketError.TimedOut)]
    [InlineData(false, SocketError.ConnectionReset)]
    [InlineData(true, SocketError.OperationAborted)]
    public void IsKnownExceptionForConsumerDisposed(bool expected, SocketError socketError)
    {
        // Arrange
        var ex = GenerateExceptionForTest(socketError);

        // Act
        var (actualIsKnownException, actualSocketError) = ex.IsKnownExceptionForConsumerDisposed();

        // Assert
        if (expected)
        {
            Assert.True(actualIsKnownException);
            Assert.NotNull(actualSocketError);
        }
        else
        {
            Assert.False(actualIsKnownException);
            Assert.Null(actualSocketError);
        }
    }

    private static Exception GenerateExceptionForTest(
        SocketError socketError)
    {
        var ex = new Exception("Test-Exception");
        if (socketError == SocketError.Success)
        {
            return ex;
        }

        ex = new SocketException((int)socketError);

        return ex;
    }
}