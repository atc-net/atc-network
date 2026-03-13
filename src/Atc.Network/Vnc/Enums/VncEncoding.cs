// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace Atc.Network.Vnc.Enums;

/// <summary>
/// VNC encoding types for framebuffer updates.
/// </summary>
[SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute", Justification = "Not flags, these are protocol-defined encoding IDs.")]
public enum VncEncoding
{
    /// <summary>
    /// Raw pixel data encoding.
    /// </summary>
    Raw = 0,

    /// <summary>
    /// CopyRect encoding - copies a rectangle from another location.
    /// </summary>
    CopyRect = 1,

    /// <summary>
    /// Rise-and-Run-length Encoding.
    /// </summary>
    Rre = 2,

    /// <summary>
    /// Compact Rise-and-Run-length Encoding.
    /// </summary>
    CoRre = 4,

    /// <summary>
    /// Hextile encoding - divides rectangles into 16x16 tiles.
    /// </summary>
    Hextile = 5,

    /// <summary>
    /// Zlib Run-Length Encoding.
    /// </summary>
    Zrle = 16,
}