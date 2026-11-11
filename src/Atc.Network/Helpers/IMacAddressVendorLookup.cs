namespace Atc.Network.Helpers;

/// <summary>
/// Finds the manufacturer (vendor) of a network interface from its MAC address.
/// </summary>
/// <remarks>
/// Use <see cref="IeeeOuiMacAddressVendorLookup"/> to look up offline from the IEEE registry files,
/// or <see cref="OnlineMacAddressVendorLookup"/> to ask the macvendors.com web service.
/// </remarks>
public interface IMacAddressVendorLookup
{
    /// <summary>
    /// Returns the vendor name for the MAC address.
    /// </summary>
    /// <param name="macAddress">The MAC address, for example "00:1B:C5:00:90:01", "00-1B-C5-00-90-01" or "001BC5009001".</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The vendor name, or <see langword="null"/> when the vendor is unknown.</returns>
    Task<string?> LookupVendorName(
        string macAddress,
        CancellationToken cancellationToken = default);
}