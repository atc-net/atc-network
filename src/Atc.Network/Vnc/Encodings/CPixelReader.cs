// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Reads compressed 3-byte pixel values used in ZRLE encoding.
/// </summary>
internal sealed class CPixelReader : PixelReader
{
    private readonly IO.ZrleCompressedReader zrleReader;

    public CPixelReader(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        IO.ZrleCompressedReader zrleReader)
        : base(rfb, framebuffer) =>
        this.zrleReader = zrleReader;

    // Read 3-byte cpixel (compressed pixel) in little-endian order
    public override int ReadPixel()
        => zrleReader.ReadCompactPixel();
}