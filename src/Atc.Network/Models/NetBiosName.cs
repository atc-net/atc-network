namespace Atc.Network.Models;

/// <summary>
/// A name a host has registered with NetBIOS.
/// </summary>
/// <param name="Name">The name, such as the computer or workgroup name.</param>
/// <param name="Suffix">
/// The 16th byte, which says what the name is for: 0x00 workstation (or workgroup, for a group name),
/// 0x20 file server, 0x1B domain master browser, 0x1C domain controllers, 0x1D master browser.
/// </param>
/// <param name="IsGroup">Whether it is a group name (a workgroup or domain) rather than a unique name.</param>
public sealed record NetBiosName(
    string Name,
    byte Suffix,
    bool IsGroup)
{
    /// <summary>
    /// Returns the name with its suffix in hexadecimal, as nbtstat shows it: "DESKTOP-ABC&lt;20&gt;".
    /// </summary>
    /// <returns>The name and suffix.</returns>
    public override string ToString()
        => $"{Name}<{Suffix:X2}>";
}