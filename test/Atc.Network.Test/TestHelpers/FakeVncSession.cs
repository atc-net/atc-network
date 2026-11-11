namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// One client connection on a <see cref="FakeVncServer"/>. All values are written big-endian, as RFB requires.
/// </summary>
public sealed class FakeVncSession : IDisposable
{
    private const byte SecurityTypeNone = 1;
    private const byte SecurityTypeVncAuthentication = 2;

    private readonly System.Net.Sockets.TcpClient client;
    private readonly System.Net.Sockets.NetworkStream stream;

    internal FakeVncSession(System.Net.Sockets.TcpClient client)
    {
        this.client = client;
        stream = client.GetStream();
    }

    public bool AuthenticationSucceeded { get; private set; }

    // Sends a framebuffer update with one Raw-encoded rectangle filled with one colour,
    // in the client's 32-bit little-endian pixel format (red shift 16, green 8, blue 0).
    public Task SendRawRectangle(
        ushort x,
        ushort y,
        ushort width,
        ushort height,
        byte red,
        byte green,
        byte blue)
    {
        var message = new List<byte> { 0, 0 }; // FramebufferUpdate, padding
        AddUInt16(message, 1); // number of rectangles
        AddUInt16(message, x);
        AddUInt16(message, y);
        AddUInt16(message, width);
        AddUInt16(message, height);
        AddInt32(message, 0); // Raw encoding

        for (var i = 0; i < width * height; i++)
        {
            message.AddRange([blue, green, red, 0]);
        }

        return Write([.. message]);
    }

    public Task SendBell()
        => Write([2]);

    public Task SendServerCutText(string text)
    {
        var textBytes = Encoding.Latin1.GetBytes(text);
        var message = new List<byte> { 3, 0, 0, 0 };
        AddInt32(message, textBytes.Length);
        message.AddRange(textBytes);
        return Write([.. message]);
    }

    public void Dispose()
    {
        stream.Dispose();
        client.Dispose();
    }

    internal async Task Handshake(string? password)
    {
        await Write(Encoding.ASCII.GetBytes("RFB 003.008\n"));
        await ReadExactly(12); // client protocol version

        var securityType = password is null ? SecurityTypeNone : SecurityTypeVncAuthentication;
        await Write([1, securityType]);
        await ReadExactly(1); // selected security type

        AuthenticationSucceeded = true;
        if (password is not null)
        {
            var challenge = RandomNumberGenerator.GetBytes(16);
            await Write(challenge);
            var response = await ReadExactly(16);
            AuthenticationSucceeded = response.AsSpan().SequenceEqual(ExpectedResponse(password, challenge));
        }

        if (!AuthenticationSucceeded)
        {
            var reason = Encoding.UTF8.GetBytes("Authentication failed");
            var failure = new List<byte>();
            AddInt32(failure, 1);
            AddInt32(failure, reason.Length);
            failure.AddRange(reason);
            await Write([.. failure]);
            return;
        }

        var ok = new List<byte>();
        AddInt32(ok, 0);
        await Write([.. ok]);

        await ReadExactly(1); // ClientInit
        await Write(ServerInit());

        await ReadExactly(20); // SetPixelFormat
        var setEncodingsHeader = await ReadExactly(4);
        var encodingCount = (setEncodingsHeader[2] << 8) | setEncodingsHeader[3];
        await ReadExactly(4 * encodingCount);
    }

    private static byte[] ServerInit()
    {
        var message = new List<byte>();
        AddUInt16(message, FakeVncServer.Width);
        AddUInt16(message, FakeVncServer.Height);

        // Pixel format: 32 bpp, depth 24, little-endian, true colour, max 255, shifts 16/8/0, 3 padding.
        message.AddRange([32, 24, 0, 1]);
        AddUInt16(message, 255);
        AddUInt16(message, 255);
        AddUInt16(message, 255);
        message.AddRange([16, 8, 0, 0, 0, 0]);

        var name = Encoding.UTF8.GetBytes(FakeVncServer.DesktopName);
        AddInt32(message, name.Length);
        message.AddRange(name);
        return [.. message];
    }

    /// <summary>
    /// Computes the expected VNC authentication response with the client's own (private) algorithm.
    /// </summary>
    private static byte[] ExpectedResponse(
        string password,
        byte[] challenge)
    {
        var method = typeof(VncClient).GetMethod("EncryptChallenge", BindingFlags.NonPublic | BindingFlags.Static);
        return (byte[])method.Invoke(null, [password, challenge]);
    }

    private static void AddUInt16(
        List<byte> buffer,
        ushort value)
    {
        buffer.Add((byte)(value >> 8));
        buffer.Add((byte)value);
    }

    private static void AddInt32(
        List<byte> buffer,
        int value)
    {
        buffer.Add((byte)(value >> 24));
        buffer.Add((byte)(value >> 16));
        buffer.Add((byte)(value >> 8));
        buffer.Add((byte)value);
    }

    private async Task Write(byte[] data)
    {
        await stream.WriteAsync(data);
        await stream.FlushAsync();
    }

    private async Task<byte[]> ReadExactly(int count)
    {
        var buffer = new byte[count];
        await stream.ReadExactlyAsync(buffer);
        return buffer;
    }
}