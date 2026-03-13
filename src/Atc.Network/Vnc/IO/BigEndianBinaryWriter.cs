namespace Atc.Network.Vnc.IO;

/// <summary>
/// A BinaryWriter that writes multi-byte values in big-endian byte order,
/// as required by the RFB protocol.
/// </summary>
internal sealed class BigEndianBinaryWriter : BinaryWriter
{
    public BigEndianBinaryWriter(Stream output)
        : base(output)
    {
    }

    public BigEndianBinaryWriter(
        Stream output,
        Encoding encoding)
        : base(output, encoding)
    {
    }

    public BigEndianBinaryWriter(
        Stream output,
        Encoding encoding,
        bool leaveOpen)
        : base(output, encoding, leaveOpen)
    {
    }

    public override void Write(ushort value)
    {
        var bytes = new byte[2];
        bytes[0] = (byte)((value >> 8) & 0xFF);
        bytes[1] = (byte)(value & 0xFF);
        BaseStream.Write(bytes, 0, 2);
    }

    public override void Write(short value)
    {
        Write((ushort)value);
    }

    public override void Write(uint value)
    {
        var bytes = new byte[4];
        bytes[0] = (byte)((value >> 24) & 0xFF);
        bytes[1] = (byte)((value >> 16) & 0xFF);
        bytes[2] = (byte)((value >> 8) & 0xFF);
        bytes[3] = (byte)(value & 0xFF);
        BaseStream.Write(bytes, 0, 4);
    }

    public override void Write(int value)
    {
        Write((uint)value);
    }
}