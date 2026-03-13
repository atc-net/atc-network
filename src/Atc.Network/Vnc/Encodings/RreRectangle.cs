namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Decodes an RRE (Rise-and-Run-length Encoding) encoded rectangle.
/// </summary>
internal sealed class RreRectangle : EncodedRectangle
{
    public RreRectangle(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        VncRectangle rectangle)
        : base(rfb, framebuffer, rectangle, Enums.VncEncoding.Rre)
    {
    }

    public override void Decode()
    {
        var numSubrects = Rfb.ReadInt32();
        var bgPixel = PixelReader.ReadPixel();

        // Fill entire rectangle with background
        Framebuffer.FillRectangle(Rectangle, bgPixel);

        // Draw subrectangles
        for (var i = 0; i < numSubrects; i++)
        {
            var pixel = PixelReader.ReadPixel();
            var subX = Rfb.ReadUInt16();
            var subY = Rfb.ReadUInt16();
            var subW = Rfb.ReadUInt16();
            var subH = Rfb.ReadUInt16();

            var subRect = new VncRectangle(
                Rectangle.X + subX,
                Rectangle.Y + subY,
                subW,
                subH);

            Framebuffer.FillRectangle(subRect, pixel);
        }
    }
}