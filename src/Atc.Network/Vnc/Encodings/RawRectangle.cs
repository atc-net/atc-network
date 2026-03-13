namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Decodes a Raw encoded rectangle.
/// </summary>
internal sealed class RawRectangle : EncodedRectangle
{
    public RawRectangle(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        VncRectangle rectangle)
        : base(rfb, framebuffer, rectangle, Enums.VncEncoding.Raw)
    {
    }

    public override void Decode()
    {
        for (var y = 0; y < Rectangle.Height; y++)
        {
            for (var x = 0; x < Rectangle.Width; x++)
            {
                FillPixel(PixelReader.ReadPixel(), Rectangle.X + x, Rectangle.Y + y);
            }
        }
    }
}