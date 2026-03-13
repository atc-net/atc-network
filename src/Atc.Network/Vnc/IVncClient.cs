// ReSharper disable InconsistentNaming
namespace Atc.Network.Vnc;

/// <summary>
/// This is a interface for <see cref="VncClient"/>.
/// </summary>
public interface IVncClient : IDisposable
{
    /// <summary>
    /// Event to raise when connection is established.
    /// </summary>
    event Action? Connected;

    /// <summary>
    /// Event to raise when connection is destroyed.
    /// </summary>
    event Action? Disconnected;

    /// <summary>
    /// Event to raise when connection state is changed.
    /// </summary>
    event EventHandler<ConnectionStateEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// Event to raise when a framebuffer update is received.
    /// </summary>
    event EventHandler<VncFramebufferUpdateEventArgs>? FramebufferUpdated;

    /// <summary>
    /// Event to raise when the server sends a bell notification.
    /// </summary>
    event Action? BellReceived;

    /// <summary>
    /// Event to raise when the server sends clipboard text.
    /// </summary>
    event Action<string>? ServerCutText;

    /// <summary>
    /// Event to raise when the connection is lost unexpectedly.
    /// </summary>
    event Action? ConnectionLost;

    /// <summary>
    /// IPAddress or hostname for server connection.
    /// </summary>
    string IPAddressOrHostname { get; }

    /// <summary>
    /// Port number for server connection.
    /// </summary>
    int Port { get; }

    /// <summary>
    /// Is client connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the framebuffer after initialization.
    /// </summary>
    VncFramebuffer? Framebuffer { get; }

    /// <summary>
    /// Gets a value indicating whether the client is in view-only mode.
    /// </summary>
    bool ViewOnly { get; }

    /// <summary>
    /// Connect to the VNC server.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task<bool> Connect(CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticate with the VNC server using a password.
    /// </summary>
    /// <param name="password">The VNC password.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task<bool> Authenticate(
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initialize the VNC session after authentication.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task Initialize(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start receiving framebuffer updates.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task StartUpdates(CancellationToken cancellationToken = default);

    /// <summary>
    /// Request a full screen update.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task RequestFullScreenUpdate(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a key event to the server.
    /// </summary>
    /// <param name="keysym">The X11 keysym value.</param>
    /// <param name="pressed">True if the key is pressed, false if released.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task SendKeyEvent(
        uint keysym,
        bool pressed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a pointer (mouse) event to the server.
    /// </summary>
    /// <param name="buttonMask">The button state mask.</param>
    /// <param name="x">The X position.</param>
    /// <param name="y">The Y position.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task SendPointerEvent(
        byte buttonMask,
        int x,
        int y,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send clipboard text to the server.
    /// </summary>
    /// <param name="text">The clipboard text.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    Task SendClientCutText(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the VNC server.
    /// </summary>
    Task Disconnect();
}