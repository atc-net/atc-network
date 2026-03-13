namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Abstract base class for reading pixel values from the RFB protocol stream.
/// </summary>
internal abstract class PixelReader
{
    protected PixelReader(
        RfbProtocol rfb,
        VncFramebuffer framebuffer)
    {
        Rfb = rfb;
        Framebuffer = framebuffer;
    }

    protected RfbProtocol Rfb { get; }

    protected VncFramebuffer Framebuffer { get; }

    /// <summary>
    /// Reads a single pixel value from the stream.
    /// </summary>
    /// <returns>A 32-bit ARGB pixel value.</returns>
    public abstract int ReadPixel();
}