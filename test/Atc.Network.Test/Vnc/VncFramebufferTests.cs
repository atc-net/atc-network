namespace Atc.Network.Test.Vnc;

public class VncFramebufferTests
{
    [Fact]
    public void Constructor_Sets_Properties_Correctly()
    {
        // Arrange
        var pixelFormat = VncPixelFormat.Create(32, 24);

        // Act
        var fb = new VncFramebuffer(800, 600, "Test Desktop", pixelFormat);

        // Assert
        Assert.Equal(800, fb.Width);
        Assert.Equal(600, fb.Height);
        Assert.Equal("Test Desktop", fb.DesktopName);
        Assert.Same(pixelFormat, fb.PixelFormat);
        Assert.Equal(800 * 600, fb.PixelData.Length);
    }

    [Fact]
    public void Indexer_Gets_And_Sets_Pixels()
    {
        // Arrange
        var fb = new VncFramebuffer(100, 100, "Test", new VncPixelFormat());

        // Act
        fb[10, 20] = unchecked((int)0xFFAABBCC);

        // Assert
        Assert.Equal(unchecked((int)0xFFAABBCC), fb[10, 20]);
    }

    [Fact]
    public void FillRectangle_Fills_Correct_Region()
    {
        // Arrange
        var fb = new VncFramebuffer(100, 100, "Test", new VncPixelFormat());
        var rect = new VncRectangle(10, 10, 5, 5);
        var pixel = unchecked((int)0xFF112233);

        // Act
        fb.FillRectangle(rect, pixel);

        // Assert
        Assert.Equal(pixel, fb[10, 10]);
        Assert.Equal(pixel, fb[14, 14]);
        Assert.Equal(0, fb[9, 9]);
        Assert.Equal(0, fb[15, 15]);
    }

    [Fact]
    public void FillRectangle_Clamps_To_Framebuffer_Bounds()
    {
        // Arrange
        var fb = new VncFramebuffer(50, 50, "Test", new VncPixelFormat());
        var rect = new VncRectangle(45, 45, 20, 20); // extends beyond bounds

        // Act (should not throw)
        fb.FillRectangle(rect, 0xFF);

        // Assert
        Assert.Equal(0xFF, fb[45, 45]);
        Assert.Equal(0xFF, fb[49, 49]);
    }

    [Fact]
    public void ToString_Contains_Dimensions()
    {
        // Arrange
        var fb = new VncFramebuffer(1920, 1080, "My Desktop", new VncPixelFormat());

        // Act
        var result = fb.ToString();

        // Assert
        Assert.Contains("1920", result, StringComparison.Ordinal);
        Assert.Contains("1080", result, StringComparison.Ordinal);
        Assert.Contains("My Desktop", result, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void FromPixelFormat_Creates_Correct_Format(int bitsPerPixel)
    {
        // Arrange & Act
        var pf = VncPixelFormat.Create(bitsPerPixel, bitsPerPixel == 32 ? 24 : bitsPerPixel);

        // Assert
        Assert.Equal(bitsPerPixel, pf.BitsPerPixel);
    }
}