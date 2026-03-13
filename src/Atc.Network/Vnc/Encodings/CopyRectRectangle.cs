namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Decodes a CopyRect encoded rectangle by copying pixels from another location.
/// </summary>
internal sealed class CopyRectRectangle : EncodedRectangle
{
    public CopyRectRectangle(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        VncRectangle rectangle)
        : base(rfb, framebuffer, rectangle, Enums.VncEncoding.CopyRect)
    {
    }

    public override void Decode()
    {
        var srcX = Rfb.ReadUInt16();
        var srcY = Rfb.ReadUInt16();

        // Copy to a temporary buffer first to handle overlapping regions correctly
        var width = Rectangle.Width;
        var height = Rectangle.Height;
        var tempBuffer = new int[width * height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var sy = srcY + y;
                var sx = srcX + x;
                if (sy >= 0 && sy < Framebuffer.Height && sx >= 0 && sx < Framebuffer.Width)
                {
                    tempBuffer[(y * width) + x] = Framebuffer.PixelData[(sy * Framebuffer.Width) + sx];
                }
            }
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                FillPixel(tempBuffer[(y * width) + x], Rectangle.X + x, Rectangle.Y + y);
            }
        }
    }
}