namespace Atc.Network.Onvif;

/// <summary>
/// What an ONVIF device says about itself (GetDeviceInformation).
/// </summary>
/// <param name="Manufacturer">The manufacturer, such as "TP-Link" or "HIKVISION".</param>
/// <param name="Model">The model, such as "C210" or "DS-2CD2183G0-IU".</param>
/// <param name="FirmwareVersion">The firmware version.</param>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="HardwareId">The hardware id.</param>
public sealed record OnvifDeviceInformation(
    string? Manufacturer,
    string? Model,
    string? FirmwareVersion,
    string? SerialNumber,
    string? HardwareId);