namespace Atc.Network.Test.Tcp;

public class TcpTerminationTypeHelperTests
{
    [Theory]
    [InlineData("", TerminationType.None)]
    [InlineData("\n", TerminationType.LineFeed)]
    [InlineData("\r", TerminationType.CarriageReturn)]
    [InlineData("\r\n", TerminationType.CarriageReturnLineFeed)]
    public void ConvertToString(string expected, TerminationType value)
        => Assert.Equal(expected, TerminationTypeHelper.ConvertToString(value));

    [Theory]
    [InlineData(new byte[] { }, TerminationType.None)]
    [InlineData(new byte[] { 0x0A }, TerminationType.LineFeed)]
    [InlineData(new byte[] { 0x0D }, TerminationType.CarriageReturn)]
    [InlineData(new byte[] { 0x0A, 0x0D }, TerminationType.CarriageReturnLineFeed)]
    public void ConvertToBytes(byte[] expected, TerminationType value)
        => Assert.Equal(expected, TerminationTypeHelper.ConvertToBytes(value));
}