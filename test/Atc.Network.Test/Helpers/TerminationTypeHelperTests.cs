namespace Atc.Network.Test.Helpers;

public class TerminationTypeHelperTests
{
    [Theory]
    [InlineData("", TerminationType.None)]
    [InlineData("\n", TerminationType.LineFeed)]
    [InlineData("\r", TerminationType.CarriageReturn)]
    [InlineData("\r\n", TerminationType.CarriageReturnLineFeed)]
    [InlineData("", TerminationType.EndOfText)]
    [InlineData("", TerminationType.EndOfTransmission)]
    public void ConvertToString(string expected, TerminationType value)
        => Assert.Equal(expected, TerminationTypeHelper.ConvertToString(value));

    [Theory]
    [InlineData(new byte[] { }, TerminationType.None)]
    [InlineData(new byte[] { 0x0A }, TerminationType.LineFeed)]
    [InlineData(new byte[] { 0x0D }, TerminationType.CarriageReturn)]
    [InlineData(new byte[] { 0x0A, 0x0D }, TerminationType.CarriageReturnLineFeed)]
    [InlineData(new byte[] { 0x03 }, TerminationType.EndOfText)]
    [InlineData(new byte[] { 0x04 }, TerminationType.EndOfTransmission)]
    public void ConvertToBytes(byte[] expected, TerminationType value)
        => Assert.Equal(expected, TerminationTypeHelper.ConvertToBytes(value));

    [Theory]
    [InlineData(true, new byte[] { }, TerminationType.None)]
    [InlineData(true, new byte[] { 0x0A }, TerminationType.LineFeed)]
    [InlineData(true, new byte[] { 0x0D }, TerminationType.CarriageReturn)]
    [InlineData(true, new byte[] { 0x0A, 0x0D }, TerminationType.CarriageReturnLineFeed)]
    [InlineData(true, new byte[] { 0x03 }, TerminationType.EndOfText)]
    [InlineData(true, new byte[] { 0x04 }, TerminationType.EndOfTransmission)]
    [InlineData(true, new byte[] { 0xAA }, TerminationType.None)]
    [InlineData(true, new byte[] { 0xAA, 0x0A }, TerminationType.LineFeed)]
    [InlineData(true, new byte[] { 0xAA, 0x0D }, TerminationType.CarriageReturn)]
    [InlineData(true, new byte[] { 0xAA, 0x0A, 0x0D }, TerminationType.CarriageReturnLineFeed)]
    [InlineData(true, new byte[] { 0xAA, 0x03 }, TerminationType.EndOfText)]
    [InlineData(true, new byte[] { 0xAA, 0x04 }, TerminationType.EndOfTransmission)]
    [InlineData(false, new byte[] { 0xAA }, TerminationType.LineFeed)]
    [InlineData(false, new byte[] { 0xAA }, TerminationType.CarriageReturn)]
    [InlineData(false, new byte[] { 0xAA }, TerminationType.CarriageReturnLineFeed)]
    [InlineData(false, new byte[] { 0x0A }, TerminationType.EndOfText)]
    [InlineData(false, new byte[] { 0x0A }, TerminationType.EndOfTransmission)]
    public void HasTerminationType(bool expected, byte[] data, TerminationType terminationType)
        => Assert.Equal(expected, TerminationTypeHelper.HasTerminationType(terminationType, data));
}