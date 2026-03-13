namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Decodes a CoRRE (Compact RRE) encoded rectangle.
/// </summary>
internal sealed class CoRreRectangle : EncodedRectangle
{
    public CoRreRectangle(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        VncRectangle rectangle)
        : base(rfb, framebuffer, rectangle, Enums.VncEncoding.CoRre)
    {
    }

    public override void Decode()
    {
        var numSubrects = Rfb.ReadInt32();
        var bgPixel = PixelReader.ReadPixel();

        // Fill entire rectangle with background
        Framebuffer.FillRectangle(Rectangle, bgPixel);

        // Draw subrectangles (CoRRE uses 8-bit coordinates instead of 16-bit)
        for (var i = 0; i < numSubrects; i++)
        {
            var pixel = PixelReader.ReadPixel();
            var subX = Rfb.ReadByte();
            var subY = Rfb.ReadByte();
            var subW = Rfb.ReadByte();
            var subH = Rfb.ReadByte();

            var subRect = new VncRectangle(
                Rectangle.X + subX,
                Rectangle.Y + subY,
                subW,
                subH);

            Framebuffer.FillRectangle(subRect, pixel);
        }
    }
}