namespace Atc.Network.Discovery;

/// <summary>
/// An ONVIF device that answered a WS-Discovery probe: a camera (network video transmitter), an NVR or another device.
/// </summary>
public sealed class OnvifDevice
{
    private const string ScopePrefix = "onvif://www.onvif.org/";

    public OnvifDevice(
        IPAddress ipAddress,
        string endpointReference,
        IReadOnlyList<string> types,
        IReadOnlyList<string> scopes,
        IReadOnlyList<Uri> xAddrs)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
        ArgumentNullException.ThrowIfNull(types);
        ArgumentNullException.ThrowIfNull(scopes);
        ArgumentNullException.ThrowIfNull(xAddrs);

        IPAddress = ipAddress;
        EndpointReference = endpointReference;
        Types = types;
        Scopes = scopes;
        XAddrs = xAddrs;
        Name = GetScopeValue("name");
        Hardware = GetScopeValue("hardware");
        Location = GetScopeValue("location");
        DeviceServiceUrl = xAddrs.FirstOrDefault(x => IPAddress.TryParse(x.Host.Trim('[', ']'), out var host) && host.Equals(ipAddress)) ??
                           xAddrs.FirstOrDefault(x => x.HostNameType == UriHostNameType.IPv4) ??
                           (xAddrs.Count > 0 ? xAddrs[0] : null);
    }

    /// <summary>
    /// Gets the address the answer came from.
    /// </summary>
    public IPAddress IPAddress { get; }

    /// <summary>
    /// Gets the device's endpoint reference, such as "urn:uuid:…": a stable id that stays the same when the device's address changes.
    /// </summary>
    public string EndpointReference { get; }

    /// <summary>
    /// Gets the device types, such as "tdn:NetworkVideoTransmitter" for a camera, as the device wrote them.
    /// </summary>
    public IReadOnlyList<string> Types { get; }

    /// <summary>
    /// Gets the scopes, percent-decoded, such as "onvif://www.onvif.org/name/C210" or "onvif://www.onvif.org/Profile/Streaming".
    /// </summary>
    public IReadOnlyList<string> Scopes { get; }

    /// <summary>
    /// Gets the device service URLs. A device can list several, such as one per address.
    /// </summary>
    public IReadOnlyList<Uri> XAddrs { get; }

    /// <summary>
    /// Gets the device service URL to use: the one on the address that answered, else the first IPv4 one,
    /// or <see langword="null"/> when the device listed none.
    /// </summary>
    public Uri? DeviceServiceUrl { get; }

    /// <summary>
    /// Gets the name from the "name" scope, such as "HIKVISION DS-2CD2183G0-IU" or "C210".
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the hardware (model) from the "hardware" scope.
    /// </summary>
    public string? Hardware { get; }

    /// <summary>
    /// Gets the location from the first "location" scope.
    /// </summary>
    public string? Location { get; }

    /// <summary>
    /// Gets a value indicating whether the device is a network video transmitter: a camera or video encoder.
    /// </summary>
    public bool IsNetworkVideoTransmitter
        => Types.Any(x => x.EndsWith("NetworkVideoTransmitter", StringComparison.Ordinal)) ||
           Scopes.Any(x => x.StartsWith(ScopePrefix + "type/", StringComparison.OrdinalIgnoreCase) &&
                           (x.EndsWith("NetworkVideoTransmitter", StringComparison.OrdinalIgnoreCase) ||
                            x.EndsWith("video_encoder", StringComparison.OrdinalIgnoreCase)));

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(IPAddress)}: {IPAddress}, {nameof(Name)}: {Name}, {nameof(Hardware)}: {Hardware}, {nameof(DeviceServiceUrl)}: {DeviceServiceUrl}, {nameof(IsNetworkVideoTransmitter)}: {IsNetworkVideoTransmitter}";

    private string? GetScopeValue(string category)
    {
        var prefix = ScopePrefix + category + "/";
        var scope = Scopes.FirstOrDefault(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return scope?[prefix.Length..];
    }
}