// ReSharper disable InconsistentNaming
namespace Atc.Network.Vnc;

/// <summary>
/// Represents the VNC server's framebuffer, holding pixel data and format information.
/// </summary>
public sealed class VncFramebuffer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VncFramebuffer"/> class.
    /// </summary>
    /// <param name="width">The width of the framebuffer in pixels.</param>
    /// <param name="height">The height of the framebuffer in pixels.</param>
    /// <param name="desktopName">The name of the remote desktop.</param>
    /// <param name="pixelFormat">The pixel format used by the framebuffer.</param>
    public VncFramebuffer(
        int width,
        int height,
        string desktopName,
        VncPixelFormat pixelFormat)
    {
        ArgumentNullException.ThrowIfNull(pixelFormat);

        Width = width;
        Height = height;
        DesktopName = desktopName ?? string.Empty;
        PixelFormat = pixelFormat;
        PixelData = new int[width * height];
    }

    /// <summary>
    /// Gets the width of the framebuffer in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the framebuffer in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the name of the remote desktop.
    /// </summary>
    public string DesktopName { get; }

    /// <summary>
    /// Gets the pixel format of the framebuffer.
    /// </summary>
    public VncPixelFormat PixelFormat { get; }

    /// <summary>
    /// Gets the pixel data as an array of 32-bit ARGB values.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Performance-critical pixel data buffer.")]
    public int[] PixelData { get; }

    /// <summary>
    /// Gets or sets the pixel value at the specified coordinates.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The 32-bit ARGB pixel value.</returns>
    public int this[int x, int y]
    {
        get => PixelData[(y * Width) + x];
        set => PixelData[(y * Width) + x] = value;
    }

    /// <summary>
    /// Fills a rectangle region with the specified pixel value.
    /// </summary>
    /// <param name="rectangle">The rectangle region to fill.</param>
    /// <param name="pixel">The pixel value to fill with.</param>
    public void FillRectangle(
        VncRectangle rectangle,
        int pixel)
    {
        var startY = System.Math.Max(rectangle.Y, 0);
        var startX = System.Math.Max(rectangle.X, 0);
        var maxY = System.Math.Min(rectangle.Y + rectangle.Height, Height);
        var maxX = System.Math.Min(rectangle.X + rectangle.Width, Width);

        for (var y = startY; y < maxY; y++)
        {
            var rowOffset = y * Width;
            for (var x = startX; x < maxX; x++)
            {
                PixelData[rowOffset + x] = pixel;
            }
        }
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(DesktopName)}: {DesktopName}, {nameof(PixelFormat)}: {PixelFormat}";
}