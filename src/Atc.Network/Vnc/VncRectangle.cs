namespace Atc.Network.Vnc;

/// <summary>
/// Represents a rectangle region in the VNC framebuffer.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct VncRectangle(int X, int Y, int Width, int Height);