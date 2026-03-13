// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace Atc.Network.Vnc;

/// <summary>
/// Factory for creating the appropriate <see cref="Encodings.EncodedRectangle"/> based on encoding type.
/// </summary>
internal static class EncodedRectangleFactory
{
    /// <summary>
    /// Creates an <see cref="Encodings.EncodedRectangle"/> for the given encoding type.
    /// </summary>
    /// <param name="rfb">The RFB protocol instance.</param>
    /// <param name="framebuffer">The framebuffer to decode into.</param>
    /// <param name="rectangle">The rectangle region.</param>
    /// <param name="encodingType">The encoding type identifier.</param>
    /// <param name="zrleReader">The ZRLE compressed reader (for ZRLE encoding).</param>
    /// <returns>An <see cref="Encodings.EncodedRectangle"/> instance.</returns>
    public static Encodings.EncodedRectangle Create(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        VncRectangle rectangle,
        int encodingType,
        IO.ZrleCompressedReader zrleReader)
        => encodingType switch
        {
            (int)Enums.VncEncoding.Raw => new Encodings.RawRectangle(rfb, framebuffer, rectangle),
            (int)Enums.VncEncoding.CopyRect => new Encodings.CopyRectRectangle(rfb, framebuffer, rectangle),
            (int)Enums.VncEncoding.Rre => new Encodings.RreRectangle(rfb, framebuffer, rectangle),
            (int)Enums.VncEncoding.CoRre => new Encodings.CoRreRectangle(rfb, framebuffer, rectangle),
            (int)Enums.VncEncoding.Hextile => new Encodings.HextileRectangle(rfb, framebuffer, rectangle),
            (int)Enums.VncEncoding.Zrle => new Encodings.ZrleRectangle(rfb, framebuffer, rectangle, zrleReader),
            _ => new Encodings.RawRectangle(rfb, framebuffer, rectangle),
        };
}