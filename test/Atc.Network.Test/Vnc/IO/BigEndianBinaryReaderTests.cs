namespace Atc.Network.Test.Vnc.IO;

public class BigEndianBinaryReaderTests
{
    [Fact]
    public void ReadUInt16_Returns_BigEndian_Value()
    {
        // Arrange - 0x0102 in big-endian
        var data = new byte[] { 0x01, 0x02 };
        using var stream = new MemoryStream(data);
        using var reader = new BigEndianBinaryReader(stream);

        // Act
        var result = reader.ReadUInt16();

        // Assert
        Assert.Equal((ushort)0x0102, result);
    }

    [Fact]
    public void ReadInt16_Returns_BigEndian_Value()
    {
        // Arrange - -1 in big-endian (0xFF, 0xFF)
        var data = new byte[] { 0xFF, 0xFF };
        using var stream = new MemoryStream(data);
        using var reader = new BigEndianBinaryReader(stream);

        // Act
        var result = reader.ReadInt16();

        // Assert
        Assert.Equal((short)-1, result);
    }

    [Fact]
    public void ReadUInt32_Returns_BigEndian_Value()
    {
        // Arrange - 0x01020304 in big-endian
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        using var stream = new MemoryStream(data);
        using var reader = new BigEndianBinaryReader(stream);

        // Act
        var result = reader.ReadUInt32();

        // Assert
        Assert.Equal(0x01020304u, result);
    }

    [Fact]
    public void ReadInt32_Returns_BigEndian_Value()
    {
        // Arrange - 0x01020304 in big-endian
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        using var stream = new MemoryStream(data);
        using var reader = new BigEndianBinaryReader(stream);

        // Act
        var result = reader.ReadInt32();

        // Assert
        Assert.Equal(0x01020304, result);
    }

    [Fact]
    public void ReadInt32_Negative_Value()
    {
        // Arrange - -1 in big-endian (0xFF, 0xFF, 0xFF, 0xFF)
        var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        using var stream = new MemoryStream(data);
        using var reader = new BigEndianBinaryReader(stream);

        // Act
        var result = reader.ReadInt32();

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void ReadUInt16_Multiple_Reads()
    {
        // Arrange
        var data = new byte[] { 0x00, 0x01, 0x00, 0x02 };
        using var stream = new MemoryStream(data);
        using var reader = new BigEndianBinaryReader(stream);

        // Act
        var first = reader.ReadUInt16();
        var second = reader.ReadUInt16();

        // Assert
        Assert.Equal((ushort)1, first);
        Assert.Equal((ushort)2, second);
    }
}