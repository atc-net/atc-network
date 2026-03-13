// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace Atc.Network.Vnc;

/// <summary>
/// Constants for the VNC/RFB protocol.
/// </summary>
public static class VncConstants
{
    /// <summary>
    /// Default VNC server port.
    /// </summary>
    public const int DefaultPort = 5900;

    /// <summary>
    /// Default connect timeout in milliseconds (10 seconds).
    /// </summary>
    public const int DefaultConnectTimeout = 10_000;

    /// <summary>
    /// Default send timeout in milliseconds (30 seconds).
    /// </summary>
    public const int DefaultSendTimeout = 30_000;

    /// <summary>
    /// Default receive timeout in milliseconds (30 seconds).
    /// </summary>
    public const int DefaultReceiveTimeout = 30_000;

    /// <summary>
    /// Default buffer size in bytes (32 KB).
    /// </summary>
    public const int DefaultBufferSize = 32_768;

    /// <summary>
    /// Grace period timeout in milliseconds.
    /// </summary>
    public const int GracePeriodTimeout = 1_000;

    /// <summary>
    /// Default bits per pixel.
    /// </summary>
    public const int DefaultBitsPerPixel = 32;

    /// <summary>
    /// Default colour depth.
    /// </summary>
    public const int DefaultDepth = 24;

    /// <summary>
    /// VNC authentication challenge length in bytes.
    /// </summary>
    public const int ChallengeLength = 16;

    /// <summary>
    /// RFB protocol version string sent by client (3.8).
    /// </summary>
    public const string RfbVersion = "RFB 003.008\n";

    /// <summary>
    /// Framebuffer update request message type.
    /// </summary>
    internal const byte ClientMessageFramebufferUpdateRequest = 3;

    /// <summary>
    /// Key event message type.
    /// </summary>
    internal const byte ClientMessageKeyEvent = 4;

    /// <summary>
    /// Pointer event message type.
    /// </summary>
    internal const byte ClientMessagePointerEvent = 5;

    /// <summary>
    /// Client cut text message type.
    /// </summary>
    internal const byte ClientMessageClientCutText = 6;

    /// <summary>
    /// Set pixel format message type.
    /// </summary>
    internal const byte ClientMessageSetPixelFormat = 0;

    /// <summary>
    /// Set encodings message type.
    /// </summary>
    internal const byte ClientMessageSetEncodings = 2;

    /// <summary>
    /// Hextile sub-encoding: raw data.
    /// </summary>
    internal const int HextileRaw = 1;

    /// <summary>
    /// Hextile sub-encoding: background specified.
    /// </summary>
    internal const int HextileBackgroundSpecified = 2;

    /// <summary>
    /// Hextile sub-encoding: foreground specified.
    /// </summary>
    internal const int HextileForegroundSpecified = 4;

    /// <summary>
    /// Hextile sub-encoding: any subrects.
    /// </summary>
    internal const int HextileAnySubrects = 8;

    /// <summary>
    /// Hextile sub-encoding: subrects coloured.
    /// </summary>
    internal const int HextileSubrectsColoured = 16;

    /// <summary>
    /// Default reconnect retry interval in milliseconds (2 seconds).
    /// </summary>
    public const int DefaultReconnectRetryInterval = 2_000;

    /// <summary>
    /// Default reconnect retry max attempts (1800 × 2s = 1 hour).
    /// </summary>
    public const int DefaultReconnectRetryMaxAttempts = 1800;
}