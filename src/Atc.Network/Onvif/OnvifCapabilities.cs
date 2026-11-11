namespace Atc.Network.Onvif;

/// <summary>
/// The service URLs of an ONVIF device (GetCapabilities). A device may serve several services on one URL.
/// </summary>
/// <param name="MediaServiceUrl">The media service: profiles and stream URIs.</param>
/// <param name="PtzServiceUrl">The pan/tilt/zoom service, when the device has one.</param>
/// <param name="ImagingServiceUrl">The imaging service.</param>
/// <param name="EventsServiceUrl">The events service.</param>
public sealed record OnvifCapabilities(
    Uri? MediaServiceUrl,
    Uri? PtzServiceUrl,
    Uri? ImagingServiceUrl,
    Uri? EventsServiceUrl);