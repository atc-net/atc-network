namespace Atc.Network.Helpers;

/// <summary>
/// Looks up MAC address vendors with the macvendors.com web service, through <see cref="MacAddressVendorLookupHelper"/>.
/// </summary>
/// <remarks>
/// Each MAC address that is not cached yet is sent to macvendors.com, at most once every 1.2 seconds.
/// Use <see cref="IeeeOuiMacAddressVendorLookup"/> to look up offline instead.
/// </remarks>
public sealed class OnlineMacAddressVendorLookup : IMacAddressVendorLookup
{
    /// <inheritdoc />
    public Task<string?> LookupVendorName(
        string macAddress,
        CancellationToken cancellationToken = default)
        => MacAddressVendorLookupHelper.LookupVendorNameFromMacAddress(macAddress, cancellationToken);
}