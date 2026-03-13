namespace Atc.Network.Vnc;

/// <summary>
/// Base configurations for <see cref="VncClient"/>.
/// </summary>
public class VncClientConfig
{
    /// <summary>
    /// Gets or sets the connect timeout value of the connection in milliseconds.
    /// </summary>
    /// <returns>
    /// The connect time-out value, in milliseconds. The default is 10000 (10 sec).
    /// </returns>
    public int ConnectTimeout { get; set; } = VncConstants.DefaultConnectTimeout;

    /// <summary>
    /// Gets or sets the send timeout value in milliseconds.
    /// </summary>
    /// <returns>
    /// The send time-out value, in milliseconds. The default is 30000 (30 sec).
    /// </returns>
    public int SendTimeout { get; set; } = VncConstants.DefaultSendTimeout;

    /// <summary>
    /// Gets or sets the receive timeout value in milliseconds.
    /// </summary>
    /// <returns>
    /// The receive time-out value, in milliseconds. The default is 30000 (30 sec).
    /// </returns>
    public int ReceiveTimeout { get; set; } = VncConstants.DefaultReceiveTimeout;

    /// <summary>
    /// Gets or sets the bits per pixel for the requested pixel format.
    /// </summary>
    /// <returns>
    /// The bits per pixel. The default is 32.
    /// </returns>
    public int BitsPerPixel { get; set; } = VncConstants.DefaultBitsPerPixel;

    /// <summary>
    /// Gets or sets the colour depth.
    /// </summary>
    /// <returns>
    /// The colour depth. The default is 24.
    /// </returns>
    public int Depth { get; set; } = VncConstants.DefaultDepth;

    /// <summary>
    /// Gets or sets a value indicating whether the client is view-only (no input events sent).
    /// </summary>
    public bool ViewOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the desktop should be shared with other clients.
    /// </summary>
    public bool SharedDesktop { get; set; } = true;

    /// <summary>
    /// Gets or sets the VNC server port.
    /// </summary>
    /// <returns>
    /// The port number. The default is 5900.
    /// </returns>
    public int Port { get; set; } = VncConstants.DefaultPort;

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(ConnectTimeout)}: {ConnectTimeout}, {nameof(SendTimeout)}: {SendTimeout}, {nameof(ReceiveTimeout)}: {ReceiveTimeout}, {nameof(BitsPerPixel)}: {BitsPerPixel}, {nameof(Depth)}: {Depth}, {nameof(ViewOnly)}: {ViewOnly}, {nameof(SharedDesktop)}: {SharedDesktop}, {nameof(Port)}: {Port}";
}