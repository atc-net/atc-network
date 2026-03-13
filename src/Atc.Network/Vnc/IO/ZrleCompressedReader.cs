// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace Atc.Network.Vnc.IO;

/// <summary>
/// Reads ZRLE-compressed data using the built-in <see cref="ZLibStream"/>.
/// Replaces the embedded zlib.NET library from VncSharpCore.
/// </summary>
internal sealed class ZrleCompressedReader : IDisposable
{
    private readonly MemoryStream compressedStream;
    private ZLibStream? zlibStream;
    private BinaryReader? uncompressedReader;
    private bool disposed;

    public ZrleCompressedReader()
    {
        compressedStream = new MemoryStream();
    }

    /// <summary>
    /// Handles the compressed data received from the server by appending it
    /// to the compressed stream and resetting the decompressor.
    /// </summary>
    /// <param name="compressedBytes">The compressed byte data.</param>
    /// <param name="length">Number of bytes in the compressed data.</param>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Lifecycle managed by class.")]
    public void HandleCompressedData(
        byte[] compressedBytes,
        int length)
    {
        ArgumentNullException.ThrowIfNull(compressedBytes);

        // Reset the memory stream position and write the compressed data
        compressedStream.SetLength(0);
        compressedStream.Write(compressedBytes, 0, length);
        compressedStream.Position = 0;

        // Dispose existing readers before recreating
        uncompressedReader?.Dispose();
        zlibStream?.Dispose();
        zlibStream = new ZLibStream(compressedStream, CompressionMode.Decompress, leaveOpen: true);
        uncompressedReader = new BinaryReader(zlibStream);
    }

    /// <summary>
    /// Reads a single byte from the decompressed stream.
    /// </summary>
    public byte ReadByte()
        => uncompressedReader?.ReadByte() ?? throw new InvalidOperationException("No compressed data has been provided.");

    /// <summary>
    /// Reads the specified number of bytes from the decompressed stream.
    /// </summary>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>A byte array of the decompressed data.</returns>
    public byte[] ReadBytes(int count)
        => uncompressedReader is null ? throw new InvalidOperationException("No compressed data has been provided.") : uncompressedReader.ReadBytes(count);

    /// <summary>
    /// Reads a big-endian unsigned 32-bit integer from the decompressed stream.
    /// </summary>
    public int ReadPixel32()
    {
        if (uncompressedReader is null)
        {
            throw new InvalidOperationException("No compressed data has been provided.");
        }

        var bytes = uncompressedReader.ReadBytes(4);
        if (bytes.Length < 4)
        {
            throw new EndOfStreamException("Unexpected end of ZRLE compressed stream while reading pixel.");
        }

        return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
    }

    /// <summary>
    /// Reads a 3-byte compressed pixel value from the decompressed stream.
    /// </summary>
    public int ReadCompactPixel()
    {
        if (uncompressedReader is null)
        {
            throw new InvalidOperationException("No compressed data has been provided.");
        }

        var bytes = uncompressedReader.ReadBytes(3);
        if (bytes.Length < 3)
        {
            throw new EndOfStreamException("Unexpected end of ZRLE compressed stream while reading compact pixel.");
        }

        // Convert 3-byte cpixel to 32-bit ARGB with full alpha
        return unchecked((int)0xFF000000) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        uncompressedReader?.Dispose();
        zlibStream?.Dispose();
        compressedStream.Dispose();
        disposed = true;
    }
}