namespace Atc.Network.Discovery;

/// <summary>
/// A device or service that answered an SSDP (UPnP) search.
/// </summary>
public sealed class SsdpDevice
{
    public SsdpDevice(
        IPAddress ipAddress,
        IReadOnlyDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        IPAddress = ipAddress;
        Headers = headers;
        UniqueServiceName = headers.GetValueOrDefault("USN") ?? string.Empty;
        SearchTarget = headers.GetValueOrDefault("ST");
        Server = headers.GetValueOrDefault("SERVER");
        Location = Uri.TryCreate(headers.GetValueOrDefault("LOCATION"), UriKind.Absolute, out var location)
            ? location
            : null;
    }

    /// <summary>
    /// Gets the address the answer came from.
    /// </summary>
    public IPAddress IPAddress { get; }

    /// <summary>
    /// Gets the unique service name (USN header), which identifies the device or service.
    /// </summary>
    public string UniqueServiceName { get; }

    /// <summary>
    /// Gets the search target that matched (ST header), such as "upnp:rootdevice" or a device type URN.
    /// </summary>
    public string? SearchTarget { get; }

    /// <summary>
    /// Gets the operating system and product (SERVER header), such as "Linux/5.4 UPnP/1.1 MiniUPnPd/2.2".
    /// </summary>
    public string? Server { get; }

    /// <summary>
    /// Gets the URL of the device description (LOCATION header), an XML document with the model and manufacturer.
    /// </summary>
    public Uri? Location { get; }

    /// <summary>
    /// Gets all headers of the answer. Names are case-insensitive.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(IPAddress)}: {IPAddress}, {nameof(Server)}: {Server}, {nameof(SearchTarget)}: {SearchTarget}, {nameof(UniqueServiceName)}: {UniqueServiceName}, {nameof(Location)}: {Location}";
}