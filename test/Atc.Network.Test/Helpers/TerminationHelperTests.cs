namespace Atc.Network.Test.Helpers;

public class TerminationHelperTests
{
    [Theory]
    [InlineData(5, "Hallo", TerminationType.None)]
    [InlineData(6, "Hallo", TerminationType.LineFeed)]
    [InlineData(6, "Hallo", TerminationType.CarriageReturn)]
    [InlineData(7, "Hallo", TerminationType.CarriageReturnLineFeed)]
    [InlineData(6, "Hallo", TerminationType.EndOfText)]
    [InlineData(6, "Hallo", TerminationType.EndOfTransmission)]
    public void AppendTerminationBytesIfNeeded(
        int expectedLength,
        string value,
        TerminationType terminationType)
    {
        // Arrange
        var bytes = Encoding.ASCII.GetBytes(value);

        // Act
        TerminationHelper.AppendTerminationBytesIfNeeded(ref bytes, terminationType);

        // Assert
        Assert.Equal(expectedLength, bytes.Length);
    }

    [Fact]
    public void AppendTerminationBytesIfNeeded_CarriageReturnLineFeed_AppendsCarriageReturnThenLineFeed()
    {
        // Arrange
        var bytes = Encoding.ASCII.GetBytes("Hallo");

        // Act
        TerminationHelper.AppendTerminationBytesIfNeeded(ref bytes, TerminationType.CarriageReturnLineFeed);

        // Assert
        Assert.Equal("Hallo\r\n", Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public void AppendTerminationBytesIfNeeded_AlreadyEndsWithCarriageReturnLineFeed_AppendsNothing()
    {
        // Arrange
        var bytes = Encoding.ASCII.GetBytes("Hallo\r\n");

        // Act
        TerminationHelper.AppendTerminationBytesIfNeeded(ref bytes, TerminationType.CarriageReturnLineFeed);

        // Assert
        Assert.Equal("Hallo\r\n", Encoding.ASCII.GetString(bytes));
    }
}