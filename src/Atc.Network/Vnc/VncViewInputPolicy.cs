namespace Atc.Network.Vnc;

/// <summary>
/// View-only input policy that blocks all input.
/// </summary>
public sealed class VncViewInputPolicy : IVncInputPolicy
{
    /// <inheritdoc />
    public bool AllowKeyboardInput => false;

    /// <inheritdoc />
    public bool AllowPointerInput => false;

    /// <inheritdoc />
    public bool AllowClipboardTransfer => false;
}