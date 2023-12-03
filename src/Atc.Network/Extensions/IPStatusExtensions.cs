// ReSharper disable once CheckNamespace
// ReSharper disable LocalizableElement
namespace Atc.Network;

/// <summary>
/// Provides extension methods for the <see cref="IPStatus"/> enumeration.
/// </summary>
/// <remarks>
/// This static class extends the <see cref="IPStatus"/> enumeration,
/// allowing for easy retrieval of localized string descriptions for each IP status value.
/// This can be particularly useful for displaying user-friendly status messages in applications
/// that perform network operations and diagnostics.
/// </remarks>
public static class IPStatusExtensions
{
    /// <summary>
    /// Retrieves a localized description for a specified <see cref="IPStatus"/> value.
    /// </summary>
    /// <param name="ipStatus">The <see cref="IPStatus"/> value for which a localized description is needed.</param>
    /// <returns>
    /// A localized string representation of the specified <see cref="IPStatus"/> value.
    /// </returns>
    /// <exception cref="SwitchCaseDefaultException">
    /// Thrown when the provided <paramref name="ipStatus"/> does not match any of the known cases.
    /// </exception>
    /// <example>
    /// This example shows how to call the <see cref="GetLocalizedDescription"/> method on an instance of the <see cref="IPStatus"/> enumeration:
    /// <code>
    /// var status = IPStatus.TimedOut;
    /// var description = status.GetLocalizedDescription();
    /// Console.WriteLine(description); // Outputs the localized description for the IPStatus.TimedOut
    /// </code>
    /// </example>
    public static string GetLocalizedDescription(
        this IPStatus ipStatus)
            => ipStatus switch
            {
                IPStatus.Unknown => EnumResources.IPStatusUnknown,
                IPStatus.Success => EnumResources.IPStatusSuccess,
                IPStatus.DestinationNetworkUnreachable => EnumResources.IPStatusDestinationNetworkUnreachable,
                IPStatus.DestinationHostUnreachable => EnumResources.IPStatusDestinationHostUnreachable,
                IPStatus.DestinationProhibited => EnumResources.IPStatusDestinationProhibited,
                IPStatus.DestinationPortUnreachable => EnumResources.IPStatusDestinationPortUnreachable,
                IPStatus.NoResources => EnumResources.IPStatusNoResources,
                IPStatus.BadOption => EnumResources.IPStatusBadOption,
                IPStatus.HardwareError => EnumResources.IPStatusHardwareError,
                IPStatus.PacketTooBig => EnumResources.IPStatusPacketTooBig,
                IPStatus.TimedOut => EnumResources.IPStatusTimedOut,
                IPStatus.BadRoute => EnumResources.IPStatusBadRoute,
                IPStatus.TtlExpired => EnumResources.IPStatusTtlExpired,
                IPStatus.TtlReassemblyTimeExceeded => EnumResources.IPStatusTtlReassemblyTimeExceeded,
                IPStatus.ParameterProblem => EnumResources.IPStatusParameterProblem,
                IPStatus.SourceQuench => EnumResources.IPStatusSourceQuench,
                IPStatus.BadDestination => EnumResources.IPStatusBadDestination,
                IPStatus.DestinationUnreachable => EnumResources.IPStatusDestinationUnreachable,
                IPStatus.TimeExceeded => EnumResources.IPStatusTimeExceeded,
                IPStatus.BadHeader => EnumResources.IPStatusBadHeader,
                IPStatus.UnrecognizedNextHeader => EnumResources.IPStatusUnrecognizedNextHeader,
                IPStatus.IcmpError => EnumResources.IPStatusIcmpError,
                IPStatus.DestinationScopeMismatch => EnumResources.IPStatusDestinationScopeMismatch,
                _ => throw new SwitchCaseDefaultException(ipStatus),
            };
}