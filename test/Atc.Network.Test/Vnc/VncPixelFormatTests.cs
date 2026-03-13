namespace Atc.Network.Test.Vnc;

public class VncPixelFormatTests
{
    [Fact]
    public void Default_Values_Are_32Bit()
    {
        // Arrange & Act
        var pf = new VncPixelFormat();

        // Assert
        Assert.Equal(32, pf.BitsPerPixel);
        Assert.Equal(24, pf.Depth);
        Assert.False(pf.BigEndian);
        Assert.True(pf.TrueColour);
        Assert.Equal(255, pf.RedMax);
        Assert.Equal(255, pf.GreenMax);
        Assert.Equal(255, pf.BlueMax);
        Assert.Equal(16, pf.RedShift);
        Assert.Equal(8, pf.GreenShift);
        Assert.Equal(0, pf.BlueShift);
    }

    [Theory]
    [InlineData(8, 8)]
    [InlineData(16, 16)]
    [InlineData(32, 24)]
    public void Create(
        int bitsPerPixel,
        int depth)
    {
        // Act
        var pf = VncPixelFormat.Create(bitsPerPixel, depth);

        // Assert
        Assert.NotNull(pf);
        Assert.Equal(bitsPerPixel, pf.BitsPerPixel);
        Assert.True(pf.TrueColour);
    }

    [Fact]
    public void Create_8Bit_Returns_Correct_Format()
    {
        // Arrange & Act
        var pf = VncPixelFormat.Create(8, 8);

        // Assert
        Assert.Equal(8, pf.BitsPerPixel);
        Assert.Equal(8, pf.Depth);
        Assert.True(pf.TrueColour);
        Assert.Equal(7, pf.RedMax);
        Assert.Equal(7, pf.GreenMax);
        Assert.Equal(3, pf.BlueMax);
    }

    [Fact]
    public void Create_16Bit_Returns_Correct_Format()
    {
        // Arrange & Act
        var pf = VncPixelFormat.Create(16, 16);

        // Assert
        Assert.Equal(16, pf.BitsPerPixel);
        Assert.Equal(16, pf.Depth);
        Assert.True(pf.TrueColour);
        Assert.Equal(31, pf.RedMax);
        Assert.Equal(63, pf.GreenMax);
        Assert.Equal(31, pf.BlueMax);
    }

    [Fact]
    public void Create_32Bit_Returns_Correct_Format()
    {
        // Arrange & Act
        var pf = VncPixelFormat.Create(32, 24);

        // Assert
        Assert.Equal(32, pf.BitsPerPixel);
        Assert.Equal(24, pf.Depth);
        Assert.True(pf.TrueColour);
        Assert.Equal(255, pf.RedMax);
        Assert.Equal(255, pf.GreenMax);
        Assert.Equal(255, pf.BlueMax);
    }

    [Fact]
    public void ToString_Contains_Key_Properties()
    {
        // Arrange
        var pf = new VncPixelFormat();

        // Act
        var result = pf.ToString();

        // Assert
        Assert.Contains("BitsPerPixel", result, StringComparison.Ordinal);
        Assert.Contains("Depth", result, StringComparison.Ordinal);
        Assert.Contains("TrueColour", result, StringComparison.Ordinal);
    }
}