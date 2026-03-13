// ReSharper disable CommentTypo
// ReSharper disable InvertIf
// ReSharper disable InconsistentNaming
// ReSharper disable MergeIntoPattern
namespace Atc.Network.Vnc;

/// <summary>
/// Core RFB protocol read/write methods for VNC communication.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
internal sealed class RfbProtocol : IDisposable
{
    private const int MaxStringLength = 10_000;

    private IO.BigEndianBinaryReader? reader;
    private IO.BigEndianBinaryWriter? writer;
    private bool disposed;

    /// <summary>
    /// Gets the underlying network stream.
    /// </summary>
    public NetworkStream? Stream { get; private set; }

    /// <summary>
    /// Initializes the protocol with a connected network stream.
    /// </summary>
    /// <param name="stream">The network stream to use.</param>
    public void Initialize(NetworkStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Stream = stream;
        reader = new IO.BigEndianBinaryReader(stream, Encoding.ASCII, leaveOpen: true);
        writer = new IO.BigEndianBinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
    }

    /// <summary>
    /// Reads the RFB protocol version from the server.
    /// </summary>
    /// <returns>The server version string.</returns>
    public string ReadProtocolVersion()
    {
        EnsureInitialized();
        var versionBytes = reader!.ReadBytes(12);
        return Encoding.ASCII.GetString(versionBytes);
    }

    /// <summary>
    /// Writes the RFB protocol version to the server.
    /// </summary>
    public void WriteProtocolVersion()
    {
        EnsureInitialized();
        var versionBytes = Encoding.ASCII.GetBytes(VncConstants.RfbVersion);
        writer!.Write(versionBytes);
        writer.Flush();
    }

    /// <summary>
    /// Reads the security types offered by the server (RFB 3.8).
    /// </summary>
    /// <returns>Array of supported security type bytes.</returns>
    public byte[] ReadSecurityTypes()
    {
        EnsureInitialized();
        var count = reader!.ReadByte();
        if (count == 0)
        {
            var reasonLength = reader.ReadInt32();
            if (reasonLength < 0 || reasonLength > MaxStringLength)
            {
                throw new InvalidOperationException($"VNC server sent invalid reason length: {reasonLength}");
            }

            var reasonBytes = reader.ReadBytes(reasonLength);
            var reason = Encoding.ASCII.GetString(reasonBytes);
            throw new InvalidOperationException($"VNC server rejected connection: {reason}");
        }

        return reader.ReadBytes(count);
    }

    /// <summary>
    /// Writes the selected security type to the server.
    /// </summary>
    /// <param name="securityType">The security type to use.</param>
    public void WriteSecurityType(byte securityType)
    {
        EnsureInitialized();
        writer!.Write(securityType);
        writer.Flush();
    }

    /// <summary>
    /// Reads the VNC authentication challenge from the server.
    /// </summary>
    /// <returns>The 16-byte challenge.</returns>
    public byte[] ReadChallenge()
    {
        EnsureInitialized();
        return reader!.ReadBytes(VncConstants.ChallengeLength);
    }

    /// <summary>
    /// Writes the DES-encrypted challenge response to the server.
    /// </summary>
    /// <param name="response">The 16-byte encrypted response.</param>
    public void WriteChallenge(byte[] response)
    {
        ArgumentNullException.ThrowIfNull(response);
        EnsureInitialized();

        writer!.Write(response);
        writer.Flush();
    }

    /// <summary>
    /// Reads the security result from the server.
    /// </summary>
    /// <returns>0 for success, non-zero for failure.</returns>
    public uint ReadSecurityResult()
    {
        EnsureInitialized();
        return (uint)reader!.ReadInt32();
    }

    /// <summary>
    /// Reads the failure reason string from the server.
    /// </summary>
    /// <returns>The reason string.</returns>
    public string ReadFailureReason()
    {
        EnsureInitialized();
        var length = reader!.ReadInt32();
        if (length < 0 || length > MaxStringLength)
        {
            throw new InvalidOperationException($"VNC server sent invalid failure reason length: {length}");
        }

        var bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Sends the ClientInit message.
    /// </summary>
    /// <param name="sharedDesktop">True to share the desktop with other clients.</param>
    public void WriteClientInit(bool sharedDesktop)
    {
        EnsureInitialized();
        writer!.Write((byte)(sharedDesktop ? 1 : 0));
        writer.Flush();
    }

    /// <summary>
    /// Reads the ServerInit message, returning framebuffer dimensions, pixel format, and desktop name.
    /// </summary>
    /// <returns>A tuple of (width, height, pixelFormat, desktopName).</returns>
    public (int Width, int Height, VncPixelFormat PixelFormat, string DesktopName) ReadServerInit()
    {
        EnsureInitialized();

        var width = reader!.ReadUInt16();
        var height = reader.ReadUInt16();

        var pixelFormat = ReadPixelFormat();

        var nameLength = reader.ReadInt32();
        if (nameLength < 0 || nameLength > MaxStringLength)
        {
            throw new InvalidOperationException($"VNC server sent invalid desktop name length: {nameLength}");
        }

        var nameBytes = reader.ReadBytes(nameLength);
        var desktopName = Encoding.UTF8.GetString(nameBytes);

        return (width, height, pixelFormat, desktopName);
    }

    /// <summary>
    /// Sends the SetPixelFormat message.
    /// </summary>
    /// <param name="pixelFormat">The pixel format to request.</param>
    public void WriteSetPixelFormat(VncPixelFormat pixelFormat)
    {
        ArgumentNullException.ThrowIfNull(pixelFormat);
        EnsureInitialized();

        writer!.Write(VncConstants.ClientMessageSetPixelFormat);

        // 3 bytes padding
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);

        WritePixelFormat(pixelFormat);
        writer.Flush();
    }

    /// <summary>
    /// Sends the SetEncodings message.
    /// </summary>
    /// <param name="encodings">Array of encoding types to support.</param>
    public void WriteSetEncodings(int[] encodings)
    {
        ArgumentNullException.ThrowIfNull(encodings);
        EnsureInitialized();

        writer!.Write(VncConstants.ClientMessageSetEncodings);
        writer.Write((byte)0); // padding
        writer.Write((ushort)encodings.Length);

        foreach (var encoding in encodings)
        {
            writer.Write(encoding);
        }

        writer.Flush();
    }

    /// <summary>
    /// Sends a FramebufferUpdateRequest message.
    /// </summary>
    /// <param name="incremental">True for incremental update, false for full.</param>
    /// <param name="x">X position.</param>
    /// <param name="y">Y position.</param>
    /// <param name="width">Width of the requested region.</param>
    /// <param name="height">Height of the requested region.</param>
    public void WriteFramebufferUpdateRequest(
        bool incremental,
        ushort x,
        ushort y,
        ushort width,
        ushort height)
    {
        EnsureInitialized();

        writer!.Write(VncConstants.ClientMessageFramebufferUpdateRequest);
        writer.Write((byte)(incremental ? 1 : 0));
        writer.Write(x);
        writer.Write(y);
        writer.Write(width);
        writer.Write(height);
        writer.Flush();
    }

    /// <summary>
    /// Sends a KeyEvent message.
    /// </summary>
    /// <param name="keysym">The X11 keysym value.</param>
    /// <param name="pressed">True if the key is pressed, false if released.</param>
    public void WriteKeyEvent(
        uint keysym,
        bool pressed)
    {
        EnsureInitialized();

        writer!.Write(VncConstants.ClientMessageKeyEvent);
        writer.Write((byte)(pressed ? 1 : 0));
        writer.Write((ushort)0); // padding
        writer.Write(keysym);
        writer.Flush();
    }

    /// <summary>
    /// Sends a PointerEvent message.
    /// </summary>
    /// <param name="buttonMask">Button state mask.</param>
    /// <param name="x">X position.</param>
    /// <param name="y">Y position.</param>
    public void WritePointerEvent(
        byte buttonMask,
        ushort x,
        ushort y)
    {
        EnsureInitialized();

        writer!.Write(VncConstants.ClientMessagePointerEvent);
        writer.Write(buttonMask);
        writer.Write(x);
        writer.Write(y);
        writer.Flush();
    }

    /// <summary>
    /// Sends a ClientCutText message.
    /// </summary>
    /// <param name="text">The clipboard text to send.</param>
    public void WriteClientCutText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        EnsureInitialized();

        var textBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(text);

        writer!.Write(VncConstants.ClientMessageClientCutText);

        // 3 bytes padding
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);

        writer.Write(textBytes.Length);
        writer.Write(textBytes);
        writer.Flush();
    }

    /// <summary>
    /// Reads a server message type byte.
    /// </summary>
    /// <returns>The message type byte.</returns>
    public byte ReadServerMessageType()
        => ReadByte();

    /// <summary>
    /// Reads a framebuffer update header and returns the number of rectangles.
    /// </summary>
    /// <returns>The number of rectangles in the update.</returns>
    public int ReadFramebufferUpdateHeader()
    {
        EnsureInitialized();
        reader!.ReadByte(); // padding
        return reader.ReadUInt16();
    }

    /// <summary>
    /// Reads a framebuffer update rectangle header.
    /// </summary>
    /// <returns>A tuple of (rectangle, encodingType).</returns>
    public (VncRectangle Rectangle, int EncodingType) ReadFramebufferUpdateRectangleHeader()
    {
        EnsureInitialized();

        var x = reader!.ReadUInt16();
        var y = reader.ReadUInt16();
        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        var encodingType = reader.ReadInt32();

        return (new VncRectangle(x, y, width, height), encodingType);
    }

    /// <summary>
    /// Reads SetColourMapEntries message data.
    /// </summary>
    /// <returns>The first colour index and the colour entries.</returns>
    public (int FirstColour, ushort[][] Colours) ReadSetColourMapEntries()
    {
        EnsureInitialized();

        reader!.ReadByte(); // padding
        var firstColour = reader.ReadUInt16();
        var numColours = reader.ReadUInt16();

        var colours = new ushort[numColours][];
        for (var i = 0; i < numColours; i++)
        {
            colours[i] = new ushort[3];
            colours[i][0] = reader.ReadUInt16(); // red
            colours[i][1] = reader.ReadUInt16(); // green
            colours[i][2] = reader.ReadUInt16(); // blue
        }

        return (firstColour, colours);
    }

    /// <summary>
    /// Reads a ServerCutText message.
    /// </summary>
    /// <returns>The clipboard text from the server.</returns>
    public string ReadServerCutText()
    {
        EnsureInitialized();

        // 3 bytes padding
        reader!.ReadByte();
        reader.ReadByte();
        reader.ReadByte();

        var length = reader.ReadInt32();
        if (length < 0 || length > MaxStringLength)
        {
            throw new InvalidOperationException($"VNC server sent invalid cut text length: {length}");
        }

        var textBytes = reader.ReadBytes(length);
        return Encoding.GetEncoding("iso-8859-1").GetString(textBytes);
    }

    /// <summary>
    /// Reads the specified number of bytes from the stream.
    /// </summary>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>The byte array read.</returns>
    public byte[] ReadBytes(int count)
    {
        EnsureInitialized();
        return reader!.ReadBytes(count);
    }

    /// <summary>
    /// Reads a single byte from the stream.
    /// </summary>
    /// <returns>The byte read.</returns>
    public byte ReadByte()
    {
        EnsureInitialized();
        return reader!.ReadByte();
    }

    /// <summary>
    /// Reads a big-endian unsigned 16-bit integer from the stream.
    /// </summary>
    /// <returns>The unsigned 16-bit value.</returns>
    public ushort ReadUInt16()
    {
        EnsureInitialized();
        return reader!.ReadUInt16();
    }

    /// <summary>
    /// Reads a big-endian signed 32-bit integer from the stream.
    /// </summary>
    /// <returns>The signed 32-bit value.</returns>
    public int ReadInt32()
    {
        EnsureInitialized();
        return reader!.ReadInt32();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        reader?.Dispose();
        writer?.Dispose();

        disposed = true;
    }

    private VncPixelFormat ReadPixelFormat()
    {
        var bitsPerPixel = reader!.ReadByte();
        var depth = reader.ReadByte();
        var bigEndian = reader.ReadByte() != 0;
        var trueColour = reader.ReadByte() != 0;
        var redMax = reader.ReadUInt16();
        var greenMax = reader.ReadUInt16();
        var blueMax = reader.ReadUInt16();
        var redShift = reader.ReadByte();
        var greenShift = reader.ReadByte();
        var blueShift = reader.ReadByte();

        // 3 bytes padding
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();

        return new VncPixelFormat
        {
            BitsPerPixel = bitsPerPixel,
            Depth = depth,
            BigEndian = bigEndian,
            TrueColour = trueColour,
            RedMax = redMax,
            GreenMax = greenMax,
            BlueMax = blueMax,
            RedShift = redShift,
            GreenShift = greenShift,
            BlueShift = blueShift,
        };
    }

    private void WritePixelFormat(VncPixelFormat pixelFormat)
    {
        writer!.Write((byte)pixelFormat.BitsPerPixel);
        writer.Write((byte)pixelFormat.Depth);
        writer.Write((byte)(pixelFormat.BigEndian ? 1 : 0));
        writer.Write((byte)(pixelFormat.TrueColour ? 1 : 0));
        writer.Write(pixelFormat.RedMax);
        writer.Write(pixelFormat.GreenMax);
        writer.Write(pixelFormat.BlueMax);
        writer.Write(pixelFormat.RedShift);
        writer.Write(pixelFormat.GreenShift);
        writer.Write(pixelFormat.BlueShift);

        // 3 bytes padding
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
    }

    private void EnsureInitialized()
    {
        if (reader is null || writer is null)
        {
            throw new InvalidOperationException("RfbProtocol has not been initialized. Call Initialize first.");
        }
    }
}