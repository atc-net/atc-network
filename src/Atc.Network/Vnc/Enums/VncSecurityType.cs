namespace Atc.Network.Vnc.Enums;

/// <summary>
/// VNC security types as defined in the RFB protocol specification.
/// </summary>
[SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "Values are protocol-defined. RFB protocol starts at 1.")]
public enum VncSecurityType
{
    /// <summary>
    /// No authentication required.
    /// </summary>
    None = 1,

    /// <summary>
    /// VNC authentication using DES-encrypted challenge-response.
    /// </summary>
    VncAuthentication = 2,
}