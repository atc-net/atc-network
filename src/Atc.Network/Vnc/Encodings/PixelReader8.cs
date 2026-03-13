namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Reads 8-bit indexed colour pixel values.
/// </summary>
internal sealed class PixelReader8 : PixelReader
{
    public PixelReader8(
        RfbProtocol rfb,
        VncFramebuffer framebuffer)
        : base(rfb, framebuffer)
    {
    }

    public override int ReadPixel()
    {
        var pixelValue = Rfb.ReadByte();
        var pf = Framebuffer.PixelFormat;

        var red = (pixelValue >> pf.RedShift) & pf.RedMax;
        var green = (pixelValue >> pf.GreenShift) & pf.GreenMax;
        var blue = (pixelValue >> pf.BlueShift) & pf.BlueMax;

        // Scale to 8-bit range
        red = red * 255 / pf.RedMax;
        green = green * 255 / pf.GreenMax;
        blue = blue * 255 / pf.BlueMax;

        return unchecked((int)0xFF000000) | (red << 16) | (green << 8) | blue;
    }
}