namespace Atc.Network.Vnc.Enums;

/// <summary>
/// Server-to-client message types in the RFB protocol.
/// </summary>
public enum VncServerMessageType
{
    /// <summary>
    /// Framebuffer update message containing updated pixel data.
    /// </summary>
    FramebufferUpdate = 0,

    /// <summary>
    /// Set colour map entries for indexed colour modes.
    /// </summary>
    SetColourMapEntries = 1,

    /// <summary>
    /// Bell notification from the server.
    /// </summary>
    Bell = 2,

    /// <summary>
    /// Server cut text (clipboard) message.
    /// </summary>
    ServerCutText = 3,
}