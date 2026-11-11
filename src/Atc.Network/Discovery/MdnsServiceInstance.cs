namespace Atc.Network.Discovery;

/// <summary>
/// A service found with mDNS / DNS-SD, such as a web interface, a printer or a Chromecast.
/// </summary>
public sealed class MdnsServiceInstance
{
    public MdnsServiceInstance(
        string fullName,
        string serviceType,
        string? hostName,
        int port,
        IReadOnlyList<IPAddress> ipAddresses,
        IReadOnlyDictionary<string, string?> properties)
    {
        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentNullException.ThrowIfNull(serviceType);

        FullName = fullName;
        ServiceType = serviceType;
        InstanceName = fullName.EndsWith("." + serviceType, StringComparison.OrdinalIgnoreCase)
            ? fullName[..^(serviceType.Length + 1)]
            : fullName;
        HostName = hostName;
        Port = port;
        IPAddresses = ipAddresses;
        Properties = properties;
    }

    /// <summary>
    /// Gets the instance's own name, such as "Office Printer".
    /// </summary>
    public string InstanceName { get; }

    /// <summary>
    /// Gets the service type, such as "_ipp._tcp.local".
    /// </summary>
    public string ServiceType { get; }

    /// <summary>
    /// Gets the full name: the instance name followed by the service type.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// Gets the host the service runs on, such as "printer.local", or <see langword="null"/> when it did not answer in time.
    /// </summary>
    public string? HostName { get; }

    /// <summary>
    /// Gets the service's port, or 0 when it did not answer in time.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Gets the host's IPv4 and IPv6 addresses. Empty when they did not arrive in time.
    /// </summary>
    public IReadOnlyList<IPAddress> IPAddresses { get; }

    /// <summary>
    /// Gets the TXT record's key/value pairs, such as "model" or "path". A key without '=' has a <see langword="null"/> value.
    /// Keys are case-insensitive.
    /// </summary>
    public IReadOnlyDictionary<string, string?> Properties { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(InstanceName)}: {InstanceName}, {nameof(ServiceType)}: {ServiceType}, {nameof(HostName)}: {HostName}, {nameof(Port)}: {Port}, {nameof(IPAddresses)}: {string.Join(", ", IPAddresses)}";
}