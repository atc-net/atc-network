namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Reads 32-bit ARGB pixel values.
/// </summary>
internal sealed class PixelReader32 : PixelReader
{
    public PixelReader32(
        RfbProtocol rfb,
        VncFramebuffer framebuffer)
        : base(rfb, framebuffer)
    {
    }

    public override int ReadPixel()
    {
        var bytes = Rfb.ReadBytes(4);

        // The pixel data is always in the byte order specified by the pixel format.
        // For typical 32-bit true colour, we read 4 bytes and assemble the pixel.
        var pf = Framebuffer.PixelFormat;

        int pixelValue;
        if (pf.BigEndian)
        {
            pixelValue = (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }
        else
        {
            pixelValue = (bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];
        }

        var red = (pixelValue >> pf.RedShift) & pf.RedMax;
        var green = (pixelValue >> pf.GreenShift) & pf.GreenMax;
        var blue = (pixelValue >> pf.BlueShift) & pf.BlueMax;

        // Scale channel values to 0-255 range when max values aren't 255
        if (pf.RedMax > 0 && pf.RedMax != 255)
        {
            red = red * 255 / pf.RedMax;
        }

        if (pf.GreenMax > 0 && pf.GreenMax != 255)
        {
            green = green * 255 / pf.GreenMax;
        }

        if (pf.BlueMax > 0 && pf.BlueMax != 255)
        {
            blue = blue * 255 / pf.BlueMax;
        }

        return unchecked((int)0xFF000000) | (red << 16) | (green << 8) | blue;
    }
}