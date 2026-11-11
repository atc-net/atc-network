namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// A minimal in-process RFB 3.8 server on loopback for end-to-end tests of <see cref="VncClient"/>.
/// </summary>
public sealed class FakeVncServer : IDisposable
{
    public const ushort Width = 4;
    public const ushort Height = 3;
    public const string DesktopName = "fake-desktop";

    private readonly System.Net.Sockets.TcpListener listener;
    private readonly string? password;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeVncServer"/> class.
    /// </summary>
    /// <param name="password">The VNC password, or null to offer security type None.</param>
    public FakeVncServer(string? password = null)
    {
        this.password = password;
        listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
    }

    public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    /// <summary>
    /// Accepts one client and runs the handshake up to and including the client's SetEncodings message.
    /// </summary>
    public async Task<FakeVncSession> AcceptSession()
    {
        var client = await listener.AcceptTcpClientAsync();
        var session = new FakeVncSession(client);
        await session.Handshake(password);
        return session;
    }

    public void Dispose()
    {
        listener.Stop();
        listener.Dispose();
    }
}