// ReSharper disable InvertIf
namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Decodes a Hextile encoded rectangle. Divides the rectangle into 16x16 tiles.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
internal sealed class HextileRectangle : EncodedRectangle
{
    public HextileRectangle(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        VncRectangle rectangle)
        : base(rfb, framebuffer, rectangle, Enums.VncEncoding.Hextile)
    {
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public override void Decode()
    {
        var bgPixel = 0;
        var fgPixel = 0;

        for (var tileY = 0; tileY < Rectangle.Height; tileY += 16)
        {
            for (var tileX = 0; tileX < Rectangle.Width; tileX += 16)
            {
                var tileWidth = System.Math.Min(16, Rectangle.Width - tileX);
                var tileHeight = System.Math.Min(16, Rectangle.Height - tileY);

                var subencoding = Rfb.ReadByte();

                if ((subencoding & VncConstants.HextileRaw) != 0)
                {
                    // Raw tile
                    for (var y = 0; y < tileHeight; y++)
                    {
                        for (var x = 0; x < tileWidth; x++)
                        {
                            FillPixel(
                                PixelReader.ReadPixel(),
                                Rectangle.X + tileX + x,
                                Rectangle.Y + tileY + y);
                        }
                    }

                    continue;
                }

                if ((subencoding & VncConstants.HextileBackgroundSpecified) != 0)
                {
                    bgPixel = PixelReader.ReadPixel();
                }

                // Fill tile with background
                var tileRect = new VncRectangle(
                    Rectangle.X + tileX,
                    Rectangle.Y + tileY,
                    tileWidth,
                    tileHeight);
                Framebuffer.FillRectangle(tileRect, bgPixel);

                if ((subencoding & VncConstants.HextileForegroundSpecified) != 0)
                {
                    fgPixel = PixelReader.ReadPixel();
                }

                if ((subencoding & VncConstants.HextileAnySubrects) != 0)
                {
                    var numSubrects = Rfb.ReadByte();
                    var coloured = (subencoding & VncConstants.HextileSubrectsColoured) != 0;

                    for (var i = 0; i < numSubrects; i++)
                    {
                        var pixel = coloured ? PixelReader.ReadPixel() : fgPixel;

                        var xy = Rfb.ReadByte();
                        var wh = Rfb.ReadByte();

                        var subX = (xy >> 4) & 0x0F;
                        var subY = xy & 0x0F;
                        var subW = ((wh >> 4) & 0x0F) + 1;
                        var subH = (wh & 0x0F) + 1;

                        var subRect = new VncRectangle(
                            Rectangle.X + tileX + subX,
                            Rectangle.Y + tileY + subY,
                            subW,
                            subH);

                        Framebuffer.FillRectangle(subRect, pixel);
                    }
                }
            }
        }
    }
}