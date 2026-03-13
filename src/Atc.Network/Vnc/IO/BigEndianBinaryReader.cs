namespace Atc.Network.Vnc.IO;

/// <summary>
/// A BinaryReader that reads multi-byte values in big-endian byte order,
/// as required by the RFB protocol.
/// </summary>
internal sealed class BigEndianBinaryReader : BinaryReader
{
    private readonly byte[] buffer = new byte[4];

    public BigEndianBinaryReader(Stream input)
        : base(input)
    {
    }

    public BigEndianBinaryReader(
        Stream input,
        Encoding encoding)
        : base(input, encoding)
    {
    }

    public BigEndianBinaryReader(
        Stream input,
        Encoding encoding,
        bool leaveOpen)
        : base(input, encoding, leaveOpen)
    {
    }

    public override ushort ReadUInt16()
    {
        FillBuffer(2);
        return (ushort)((buffer[0] << 8) | buffer[1]);
    }

    public override short ReadInt16()
    {
        FillBuffer(2);
        return (short)((buffer[0] << 8) | buffer[1]);
    }

    public override uint ReadUInt32()
    {
        FillBuffer(4);
        return (uint)((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3]);
    }

    public override int ReadInt32()
    {
        FillBuffer(4);
        return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
    }

    private new void FillBuffer(int count)
    {
        var totalRead = 0;
        while (totalRead < count)
        {
            var bytesRead = BaseStream.Read(buffer, totalRead, count - totalRead);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException();
            }

            totalRead += bytesRead;
        }
    }
}