namespace Atc.Network.Test.Tcp;

public class TcpTerminationTypeHelperTests
{
    [Theory]
    [InlineData("", TcpTerminationType.None)]
    [InlineData("\n", TcpTerminationType.LineFeed)]
    [InlineData("\r", TcpTerminationType.CarriageReturn)]
    [InlineData("\r\n", TcpTerminationType.CarriageReturnLineFeed)]
    public void ConvertToString(string expected, TcpTerminationType value)
        => Assert.Equal(expected, TcpTerminationTypeHelper.ConvertToString(value));

    [Theory]
    [InlineData(new byte[] { }, TcpTerminationType.None)]
    [InlineData(new byte[] { 0x0A }, TcpTerminationType.LineFeed)]
    [InlineData(new byte[] { 0x0D }, TcpTerminationType.CarriageReturn)]
    [InlineData(new byte[] { 0x0A, 0x0D }, TcpTerminationType.CarriageReturnLineFeed)]
    public void ConvertToBytes(byte[] expected, TcpTerminationType value)
        => Assert.Equal(expected, TcpTerminationTypeHelper.ConvertToBytes(value));
}