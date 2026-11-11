namespace Atc.Network.Console.Scanner;

/// <summary>
/// What was read from an ONVIF camera.
/// </summary>
public sealed class OnvifCameraDetails
{
    public OnvifCameraDetails(
        CameraCandidate camera,
        Uri deviceServiceUrl)
    {
        Camera = camera;
        DeviceServiceUrl = deviceServiceUrl;
    }

    public CameraCandidate Camera { get; }

    public Uri DeviceServiceUrl { get; }

    public DateTime? Clock { get; set; }

    public TimeSpan ClockOffset { get; set; }

    public OnvifDeviceInformation? Information { get; set; }

    public IReadOnlyList<OnvifStreamInfo> Streams { get; set; } = [];

    public string? Error { get; set; }
}