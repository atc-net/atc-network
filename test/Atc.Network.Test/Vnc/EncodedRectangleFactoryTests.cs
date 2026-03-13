namespace Atc.Network.Test.Vnc;

public class EncodedRectangleFactoryTests
{
    [Theory]
    [InlineData((int)VncEncoding.Raw, typeof(RawRectangle))]
    [InlineData((int)VncEncoding.CopyRect, typeof(CopyRectRectangle))]
    [InlineData((int)VncEncoding.Rre, typeof(RreRectangle))]
    [InlineData((int)VncEncoding.CoRre, typeof(CoRreRectangle))]
    [InlineData((int)VncEncoding.Hextile, typeof(HextileRectangle))]
    [InlineData((int)VncEncoding.Zrle, typeof(ZrleRectangle))]
    public void Create_Returns_Correct_Type(
        int encodingType,
        Type expectedType)
    {
        // Arrange
        using var rfb = new RfbProtocol();
        var framebuffer = new VncFramebuffer(100, 100, "Test", new VncPixelFormat());
        var rectangle = new VncRectangle(0, 0, 10, 10);
        using var zrleReader = new ZrleCompressedReader();

        // Act
        var result = EncodedRectangleFactory.Create(rfb, framebuffer, rectangle, encodingType, zrleReader);

        // Assert
        Assert.IsType(expectedType, result);
    }

    [Fact]
    public void Create_Unknown_Encoding_Falls_Back_To_Raw()
    {
        // Arrange
        using var rfb = new RfbProtocol();
        var framebuffer = new VncFramebuffer(100, 100, "Test", new VncPixelFormat());
        var rectangle = new VncRectangle(0, 0, 10, 10);
        using var zrleReader = new ZrleCompressedReader();

        // Act
        var result = EncodedRectangleFactory.Create(rfb, framebuffer, rectangle, 999, zrleReader);

        // Assert
        Assert.IsType<RawRectangle>(result);
    }
}