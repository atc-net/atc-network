namespace Atc.Network.Vnc;

/// <summary>
/// Default input policy that allows all input types.
/// </summary>
public sealed class VncDefaultInputPolicy : IVncInputPolicy
{
    /// <inheritdoc />
    public bool AllowKeyboardInput => true;

    /// <inheritdoc />
    public bool AllowPointerInput => true;

    /// <inheritdoc />
    public bool AllowClipboardTransfer => true;
}