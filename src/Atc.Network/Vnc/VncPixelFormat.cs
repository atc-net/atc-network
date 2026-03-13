namespace Atc.Network.Vnc;

/// <summary>
/// Describes the pixel format used by the VNC framebuffer.
/// </summary>
public sealed class VncPixelFormat
{
    /// <summary>
    /// Gets or sets the number of bits per pixel (8, 16, or 32).
    /// </summary>
    public int BitsPerPixel { get; set; } = VncConstants.DefaultBitsPerPixel;

    /// <summary>
    /// Gets or sets the colour depth.
    /// </summary>
    public int Depth { get; set; } = VncConstants.DefaultDepth;

    /// <summary>
    /// Gets or sets a value indicating whether the pixel values are in big-endian byte order.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether true colour is used (as opposed to colour map).
    /// </summary>
    public bool TrueColour { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum red value.
    /// </summary>
    public ushort RedMax { get; set; } = 255;

    /// <summary>
    /// Gets or sets the maximum green value.
    /// </summary>
    public ushort GreenMax { get; set; } = 255;

    /// <summary>
    /// Gets or sets the maximum blue value.
    /// </summary>
    public ushort BlueMax { get; set; } = 255;

    /// <summary>
    /// Gets or sets the red colour shift.
    /// </summary>
    public byte RedShift { get; set; } = 16;

    /// <summary>
    /// Gets or sets the green colour shift.
    /// </summary>
    public byte GreenShift { get; set; } = 8;

    /// <summary>
    /// Gets or sets the blue colour shift.
    /// </summary>
    public byte BlueShift { get; set; }

    /// <summary>
    /// Creates a pixel format for the specified bits per pixel and depth.
    /// </summary>
    /// <param name="bitsPerPixel">Bits per pixel (8, 16, or 32).</param>
    /// <param name="depth">Colour depth.</param>
    /// <returns>A configured <see cref="VncPixelFormat"/>.</returns>
    public static VncPixelFormat Create(
        int bitsPerPixel,
        int depth)
        => bitsPerPixel switch
        {
            8 => new VncPixelFormat
            {
                BitsPerPixel = 8,
                Depth = 8,
                BigEndian = false,
                TrueColour = true,
                RedMax = 7,
                GreenMax = 7,
                BlueMax = 3,
                RedShift = 0,
                GreenShift = 3,
                BlueShift = 6,
            },
            16 => new VncPixelFormat
            {
                BitsPerPixel = 16,
                Depth = depth,
                BigEndian = false,
                TrueColour = true,
                RedMax = 31,
                GreenMax = 63,
                BlueMax = 31,
                RedShift = 11,
                GreenShift = 5,
                BlueShift = 0,
            },
            _ => new VncPixelFormat
            {
                BitsPerPixel = 32,
                Depth = depth,
                BigEndian = false,
                TrueColour = true,
                RedMax = 255,
                GreenMax = 255,
                BlueMax = 255,
                RedShift = 16,
                GreenShift = 8,
                BlueShift = 0,
            },
        };

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(BitsPerPixel)}: {BitsPerPixel}, {nameof(Depth)}: {Depth}, {nameof(BigEndian)}: {BigEndian}, {nameof(TrueColour)}: {TrueColour}";
}