// ReSharper disable InvertIf
namespace Atc.Network.Vnc.Encodings;

/// <summary>
/// Decodes a ZRLE (Zlib Run-Length Encoding) encoded rectangle.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
[SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
internal sealed class ZrleRectangle : EncodedRectangle
{
    private readonly IO.ZrleCompressedReader zrleReader;

    public ZrleRectangle(
        RfbProtocol rfb,
        VncFramebuffer framebuffer,
        VncRectangle rectangle,
        IO.ZrleCompressedReader zrleReader)
        : base(rfb, framebuffer, rectangle, Enums.VncEncoding.Zrle)
    {
        this.zrleReader = zrleReader;
    }

    public override void Decode()
    {
        // Read compressed data length and data
        var compressedLength = Rfb.ReadInt32();
        var compressedData = Rfb.ReadBytes(compressedLength);
        zrleReader.HandleCompressedData(compressedData, compressedLength);

        for (var tileY = 0; tileY < Rectangle.Height; tileY += 64)
        {
            for (var tileX = 0; tileX < Rectangle.Width; tileX += 64)
            {
                var tileWidth = System.Math.Min(64, Rectangle.Width - tileX);
                var tileHeight = System.Math.Min(64, Rectangle.Height - tileY);

                DecodeTile(tileX, tileY, tileWidth, tileHeight);
            }
        }
    }

    private void DecodeTile(
        int tileX,
        int tileY,
        int tileWidth,
        int tileHeight)
    {
        var subencoding = zrleReader.ReadByte();

        if (subencoding == 0)
        {
            // Raw pixel data
            DecodeRawTile(tileX, tileY, tileWidth, tileHeight);
        }
        else if (subencoding == 1)
        {
            // Solid tile
            var pixel = zrleReader.ReadCompactPixel();
            var tileRect = new VncRectangle(
                Rectangle.X + tileX,
                Rectangle.Y + tileY,
                tileWidth,
                tileHeight);
            Framebuffer.FillRectangle(tileRect, pixel);
        }
        else if (subencoding >= 2 && subencoding <= 16)
        {
            // Packed palette
            DecodePaletteTile(subencoding, tileX, tileY, tileWidth, tileHeight);
        }
        else if (subencoding == 128)
        {
            // Plain RLE
            DecodePlainRleTile(tileX, tileY, tileWidth, tileHeight);
        }
        else if (subencoding >= 130)
        {
            // Palette RLE
            DecodePaletteRleTile(subencoding, tileX, tileY, tileWidth, tileHeight);
        }
    }

    private void DecodeRawTile(
        int tileX,
        int tileY,
        int tileWidth,
        int tileHeight)
    {
        for (var y = 0; y < tileHeight; y++)
        {
            for (var x = 0; x < tileWidth; x++)
            {
                var pixel = zrleReader.ReadCompactPixel();
                FillPixel(pixel, Rectangle.X + tileX + x, Rectangle.Y + tileY + y);
            }
        }
    }

    private void DecodePaletteTile(
        int paletteSize,
        int tileX,
        int tileY,
        int tileWidth,
        int tileHeight)
    {
        var palette = new int[paletteSize];
        for (var i = 0; i < paletteSize; i++)
        {
            palette[i] = zrleReader.ReadCompactPixel();
        }

        var bitsPerIndex = paletteSize switch
        {
            2 => 1,
            3 or 4 => 2,
            _ => 4,
        };

        for (var y = 0; y < tileHeight; y++)
        {
            var bitsUsed = 0;
            byte currentByte = 0;

            for (var x = 0; x < tileWidth; x++)
            {
                if (bitsUsed == 0)
                {
                    currentByte = zrleReader.ReadByte();
                    bitsUsed = 8;
                }

                bitsUsed -= bitsPerIndex;
                var index = (currentByte >> bitsUsed) & ((1 << bitsPerIndex) - 1);

                if (index < palette.Length)
                {
                    FillPixel(palette[index], Rectangle.X + tileX + x, Rectangle.Y + tileY + y);
                }
            }
        }
    }

    private void DecodePlainRleTile(
        int tileX,
        int tileY,
        int tileWidth,
        int tileHeight)
    {
        var totalPixels = tileWidth * tileHeight;
        var pixelsDecoded = 0;

        while (pixelsDecoded < totalPixels)
        {
            var pixel = zrleReader.ReadCompactPixel();
            var runLength = 1;

            var b = zrleReader.ReadByte();
            runLength += b;
            while (b == 255)
            {
                b = zrleReader.ReadByte();
                runLength += b;
            }

            for (var i = 0; i < runLength && pixelsDecoded < totalPixels; i++)
            {
                var x = pixelsDecoded % tileWidth;
                var y = pixelsDecoded / tileWidth;
                FillPixel(pixel, Rectangle.X + tileX + x, Rectangle.Y + tileY + y);
                pixelsDecoded++;
            }
        }
    }

    private void DecodePaletteRleTile(
        int subencoding,
        int tileX,
        int tileY,
        int tileWidth,
        int tileHeight)
    {
        var paletteSize = subencoding - 128;
        var palette = new int[paletteSize];
        for (var i = 0; i < paletteSize; i++)
        {
            palette[i] = zrleReader.ReadCompactPixel();
        }

        var totalPixels = tileWidth * tileHeight;
        var pixelsDecoded = 0;

        while (pixelsDecoded < totalPixels)
        {
            var index = zrleReader.ReadByte();
            var runLength = 1;

            if ((index & 128) != 0)
            {
                index &= 127;

                var b = zrleReader.ReadByte();
                runLength += b;
                while (b == 255)
                {
                    b = zrleReader.ReadByte();
                    runLength += b;
                }
            }

            var pixel = index < palette.Length ? palette[index] : 0;
            for (var i = 0; i < runLength && pixelsDecoded < totalPixels; i++)
            {
                var x = pixelsDecoded % tileWidth;
                var y = pixelsDecoded / tileWidth;
                FillPixel(pixel, Rectangle.X + tileX + x, Rectangle.Y + tileY + y);
                pixelsDecoded++;
            }
        }
    }
}