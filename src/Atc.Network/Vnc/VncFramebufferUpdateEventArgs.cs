namespace Atc.Network.Vnc;

/// <summary>
/// Event arguments for a framebuffer update.
/// </summary>
public class VncFramebufferUpdateEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VncFramebufferUpdateEventArgs"/> class.
    /// </summary>
    /// <param name="rectangle">The updated rectangle region.</param>
    /// <param name="framebuffer">The framebuffer containing the updated pixel data.</param>
    public VncFramebufferUpdateEventArgs(
        VncRectangle rectangle,
        VncFramebuffer framebuffer)
    {
        Rectangle = rectangle;
        Framebuffer = framebuffer;
    }

    /// <summary>
    /// Gets the updated rectangle region.
    /// </summary>
    public VncRectangle Rectangle { get; }

    /// <summary>
    /// Gets the framebuffer containing the updated pixel data.
    /// </summary>
    public VncFramebuffer Framebuffer { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Rectangle)}: {Rectangle}";
}