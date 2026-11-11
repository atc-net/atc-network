namespace Atc.Network.Onvif;

/// <summary>
/// A media profile of an ONVIF device: one video stream setup, such as the main or the sub stream.
/// </summary>
/// <param name="Token">The profile token, which <c>GetStreamUri</c> takes.</param>
/// <param name="Name">The profile name, such as "mainStream".</param>
/// <param name="Encoding">The video encoding, such as "H264" or "H265".</param>
/// <param name="Width">The video width in pixels, or 0 when unknown.</param>
/// <param name="Height">The video height in pixels, or 0 when unknown.</param>
/// <param name="FrameRate">The frame rate limit, or 0 when unknown.</param>
public sealed record OnvifMediaProfile(
    string Token,
    string? Name,
    string? Encoding,
    int Width,
    int Height,
    int FrameRate);