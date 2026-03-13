namespace Atc.Network.Test.Vnc.IO;

public class BigEndianBinaryWriterTests
{
    [Fact]
    public void WriteUInt16_Writes_BigEndian()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new BigEndianBinaryWriter(stream);

        // Act
        writer.Write((ushort)0x0102);
        writer.Flush();

        // Assert
        var bytes = stream.ToArray();
        Assert.Equal(new byte[] { 0x01, 0x02 }, bytes);
    }

    [Fact]
    public void WriteInt16_Writes_BigEndian()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new BigEndianBinaryWriter(stream);

        // Act
        writer.Write((short)0x0102);
        writer.Flush();

        // Assert
        var bytes = stream.ToArray();
        Assert.Equal(new byte[] { 0x01, 0x02 }, bytes);
    }

    [Fact]
    public void WriteUInt32_Writes_BigEndian()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new BigEndianBinaryWriter(stream);

        // Act
        writer.Write(0x01020304u);
        writer.Flush();

        // Assert
        var bytes = stream.ToArray();
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, bytes);
    }

    [Fact]
    public void WriteInt32_Writes_BigEndian()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new BigEndianBinaryWriter(stream);

        // Act
        writer.Write(0x01020304);
        writer.Flush();

        // Assert
        var bytes = stream.ToArray();
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, bytes);
    }

    [Fact]
    public void WriteUInt32_MaxValue()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new BigEndianBinaryWriter(stream);

        // Act
        writer.Write(uint.MaxValue);
        writer.Flush();

        // Assert
        var bytes = stream.ToArray();
        Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, bytes);
    }
}