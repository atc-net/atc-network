namespace Atc.Network.Onvif;

/// <summary>
/// A media profile with its RTSP stream URI.
/// </summary>
/// <param name="Profile">The media profile.</param>
/// <param name="StreamUri">The RTSP URI, without credentials; add the user name and password to play it.</param>
public sealed record OnvifStreamInfo(
    OnvifMediaProfile Profile,
    Uri StreamUri);