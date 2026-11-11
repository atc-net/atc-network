namespace Atc.Network.Console.Scanner;

/// <summary>
/// A scanned address with its camera score.
/// </summary>
/// <param name="Result">The scan result.</param>
/// <param name="Score">The score; see <see cref="CameraDetector.CameraScore"/>.</param>
/// <param name="Reasons">What was seen, such as "ONVIF", "mDNS _psia" or "RTSP :554".</param>
/// <param name="Name">The best name found for the address, or <see langword="null"/>.</param>
public sealed record CameraCandidate(
    IPScanResult Result,
    int Score,
    IReadOnlyList<string> Reasons,
    string? Name)
{
    /// <summary>
    /// Gets a value indicating whether the address is a camera.
    /// </summary>
    public bool IsCamera
        => Score >= CameraDetector.CameraScore;

    /// <summary>
    /// Gets a value indicating whether the address may be a camera.
    /// </summary>
    public bool IsMaybeCamera
        => Score >= CameraDetector.MaybeCameraScore && !IsCamera;

    /// <summary>
    /// Gets the ONVIF device service URL, when the address answered ONVIF discovery.
    /// </summary>
    public Uri? OnvifDeviceServiceUrl
        => Result.OnvifDevices.Select(x => x.DeviceServiceUrl).FirstOrDefault(x => x is not null);
}