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
    public void AppendTerminationBytesIfNeeded(int expectedLength, string value, TerminationType terminationType)
    {
        // Arrange
        var bytes = Encoding.ASCII.GetBytes(value);

        // Act
        TerminationHelper.AppendTerminationBytesIfNeeded(ref bytes, terminationType);

        // Assert
        Assert.Equal(expectedLength, bytes.Length);
    }
}