namespace Atc.Network.Models;

/// <summary>
/// The answer of a host to a NetBIOS node status query: its names and MAC address.
/// </summary>
public sealed class NetBiosNodeStatus
{
    public NetBiosNodeStatus(
        IPAddress ipAddress,
        IReadOnlyList<NetBiosName> names,
        string? macAddress)
    {
        ArgumentNullException.ThrowIfNull(names);

        IPAddress = ipAddress;
        Names = names;
        MacAddress = macAddress;
        ComputerName = names.FirstOrDefault(x => x is { IsGroup: false, Suffix: 0x00 })?.Name;
        Workgroup = names.FirstOrDefault(x => x is { IsGroup: true, Suffix: 0x00 })?.Name;
    }

    /// <summary>
    /// Gets the address that answered.
    /// </summary>
    public IPAddress IPAddress { get; }

    /// <summary>
    /// Gets the computer name: the unique name with suffix 0x00 (the workstation service), or <see langword="null"/>.
    /// </summary>
    public string? ComputerName { get; }

    /// <summary>
    /// Gets the workgroup or domain: the group name with suffix 0x00, or <see langword="null"/>.
    /// </summary>
    public string? Workgroup { get; }

    /// <summary>
    /// Gets the MAC address the host reports, such as "00-15-5D-01-02-03", or <see langword="null"/> when it reports
    /// none (Samba reports zeros).
    /// </summary>
    public string? MacAddress { get; }

    /// <summary>
    /// Gets every name the host has registered.
    /// </summary>
    public IReadOnlyList<NetBiosName> Names { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(IPAddress)}: {IPAddress}, {nameof(ComputerName)}: {ComputerName}, {nameof(Workgroup)}: {Workgroup}, {nameof(MacAddress)}: {MacAddress}, {nameof(Names)}: {string.Join(", ", Names)}";
}