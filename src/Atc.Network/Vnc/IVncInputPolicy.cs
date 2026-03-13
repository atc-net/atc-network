namespace Atc.Network.Vnc;

/// <summary>
/// Defines input handling policy for a VNC client.
/// Controls whether keyboard and pointer events are sent to the server.
/// </summary>
public interface IVncInputPolicy
{
    /// <summary>
    /// Determines whether keyboard events should be forwarded to the server.
    /// </summary>
    bool AllowKeyboardInput { get; }

    /// <summary>
    /// Determines whether pointer (mouse) events should be forwarded to the server.
    /// </summary>
    bool AllowPointerInput { get; }

    /// <summary>
    /// Determines whether clipboard text should be sent to the server.
    /// </summary>
    bool AllowClipboardTransfer { get; }
}