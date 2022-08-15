// ReSharper disable once CheckNamespace
namespace Atc.Network;

/// <summary>
/// Extension methods for <see langword="ushort" />
/// </summary>
public static class UshortExtensions
{
    public static bool IsPortForIPService(
        this ushort portNumber,
        ServiceProtocolType serviceProtocolType,
        IPServicePortExaminationLevel matchLevel)
        => matchLevel switch
        {
            IPServicePortExaminationLevel.All => true,
            IPServicePortExaminationLevel.WellKnown => portNumber.IsWellKnownIPServicePort(serviceProtocolType),
            IPServicePortExaminationLevel.WellKnownAndCommon => portNumber.IsWellKnownOrCommonIPServicePort(serviceProtocolType),
            _ => false,
        };

    /// <summary>
    /// Returns <see langword="true"/> if the value is a well known port for a IP service.
    /// </summary>
    /// <param name="portNumber">The port number.</param>
    /// <param name="serviceProtocolType">Type of the service protocol.</param>
    /// <remarks>
    /// See <see href="https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers" /> for
    /// a complete list of well known port numbers.
    /// </remarks>
    public static bool IsWellKnownIPServicePort(
        this ushort portNumber,
        ServiceProtocolType serviceProtocolType)
        => serviceProtocolType switch
        {
            ServiceProtocolType.Https => IPServicePortLists.WellKnownForHttps.Contains(portNumber),
            ServiceProtocolType.Http => IPServicePortLists.WellKnownForHttp.Contains(portNumber),
            ServiceProtocolType.Ftps => IPServicePortLists.WellKnownForFtps.Contains(portNumber),
            ServiceProtocolType.Ftp => IPServicePortLists.WellKnownForFtp.Contains(portNumber),
            ServiceProtocolType.Telnet => IPServicePortLists.WellKnownForTelnet.Contains(portNumber),
            ServiceProtocolType.Ssh => IPServicePortLists.WellKnownForSsh.Contains(portNumber),
            _ => false,
        };

    /// <summary>
    /// Returns <see langword="true"/> if the value is a well known port or
    /// a common substitute for a IP service.
    /// </summary>
    /// <param name="portNumber">The port number.</param>
    /// <param name="serviceProtocolType">Type of the service protocol.</param>
    /// <remarks>
    /// See <see href="https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers" /> for
    /// a complete list of well known port numbers.
    /// </remarks>
    public static bool IsWellKnownOrCommonIPServicePort(
        this ushort portNumber,
        ServiceProtocolType serviceProtocolType)
        => serviceProtocolType switch
        {
            ServiceProtocolType.Https => IPServicePortLists.WellKnownOrCommonForHttps.Contains(portNumber),
            ServiceProtocolType.Http => IPServicePortLists.WellKnownOrCommonForHttp.Contains(portNumber),
            ServiceProtocolType.Ftps => IPServicePortLists.WellKnownOrCommonForFtps.Contains(portNumber),
            ServiceProtocolType.Ftp => IPServicePortLists.WellKnownOrCommonForFtp.Contains(portNumber),
            ServiceProtocolType.Telnet => IPServicePortLists.WellKnownOrCommonForTelnet.Contains(portNumber),
            ServiceProtocolType.Ssh => IPServicePortLists.WellKnownOrCommonForSsh.Contains(portNumber),
            _ => false,
        };
}