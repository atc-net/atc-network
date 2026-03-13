// ReSharper disable InvertIf
namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Base class for encoded rectangle types in the RFB protocol.
/// Handles decoding pixel data into the framebuffer.
/// </summary>
internal abstract class EncodedRectangle
{
    protected EncodedRectangle(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        VncRectangle rectangle,
        Enums.VncEncoding encoding)
    {
        Rfb = rfb;
        Framebuffer = framebuffer;
        Rectangle = rectangle;
        Encoding = encoding;

        PixelReader = framebuffer.PixelFormat.BitsPerPixel switch
        {
            8 => new PixelReader8(rfb, framebuffer),
            16 => new PixelReader16(rfb, framebuffer),
            _ => new PixelReader32(rfb, framebuffer),
        };
    }

    protected RfbProtocol Rfb { get; }

    protected VncFramebuffer Framebuffer { get; }

    protected PixelReader PixelReader { get; }

    public VncRectangle Rectangle { get; }

    public Enums.VncEncoding Encoding { get; }

    /// <summary>
    /// Decodes the rectangle data from the protocol stream into the framebuffer.
    /// </summary>
    public abstract void Decode();

    /// <summary>
    /// Fills a pixel row in the framebuffer from an array.
    /// </summary>
    /// <param name="pixels">The pixel values to fill.</param>
    /// <param name="x">Starting X position.</param>
    /// <param name="y">Y position.</param>
    /// <param name="width">Number of pixels to fill.</param>
    protected void FillPixels(
        int[] pixels,
        int x,
        int y,
        int width)
    {
        var offset = (y * Framebuffer.Width) + x;
        var maxPixels = System.Math.Min(width, Framebuffer.PixelData.Length - offset);
        if (maxPixels > 0)
        {
            Array.Copy(pixels, 0, Framebuffer.PixelData, offset, maxPixels);
        }
    }

    /// <summary>
    /// Fills a single pixel in the framebuffer.
    /// </summary>
    /// <param name="pixel">The pixel value.</param>
    /// <param name="x">X position.</param>
    /// <param name="y">Y position.</param>
    protected void FillPixel(
        int pixel,
        int x,
        int y)
    {
        var offset = (y * Framebuffer.Width) + x;
        if (offset >= 0 && offset < Framebuffer.PixelData.Length)
        {
            Framebuffer.PixelData[offset] = pixel;
        }
    }
}