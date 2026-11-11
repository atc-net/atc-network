namespace Atc.Network.Discovery;

/// <summary>
/// A minimal DNS message reader and writer, with only what mDNS discovery needs:
/// queries with PTR, SRV, TXT, A and AAAA questions, and the records of a response.
/// </summary>
/// <remarks>
/// Packets come from the network, so the reader treats every malformed packet as invalid
/// (<see cref="TryParse"/> returns <see langword="false"/>): a truncated packet, a compression pointer
/// that does not point backwards, a label longer than 63 bytes or a name longer than 255 bytes.
/// </remarks>
internal sealed class DnsMessage
{
    private const int HeaderLength = 12;
    private const int MaxLabelLength = 63;
    private const int MaxNameLength = 255;
    private const ushort ResponseFlag = 0x8000;
    private const ushort ClassInternet = 1;
    private const ushort UnicastResponseBit = 0x8000;
    private const byte PointerMask = 0xC0;

    public DnsMessage(
        bool isResponse,
        IReadOnlyList<DnsRecord> records)
    {
        IsResponse = isResponse;
        Records = records;
    }

    /// <summary>
    /// Gets a value indicating whether the message is a response.
    /// </summary>
    public bool IsResponse { get; }

    /// <summary>
    /// Gets the records of the answer, authority and additional sections, of the types in <see cref="DnsRecordType"/>.
    /// </summary>
    public IReadOnlyList<DnsRecord> Records { get; }

    /// <summary>
    /// Creates a query with the questions, each asking for a unicast response (the QU bit).
    /// </summary>
    /// <param name="questions">The questions.</param>
    /// <returns>The query.</returns>
    public static byte[] CreateQuery(IReadOnlyList<DnsQuestion> questions)
    {
        ArgumentNullException.ThrowIfNull(questions);

        var buffer = new List<byte>(HeaderLength + (questions.Count * 32));
        buffer.AddRange(new byte[HeaderLength]);
        buffer[4] = (byte)(questions.Count >> 8);
        buffer[5] = (byte)questions.Count;

        foreach (var question in questions)
        {
            WriteName(buffer, question.Name);
            WriteUInt16(buffer, (ushort)question.Type);
            WriteUInt16(buffer, UnicastResponseBit | ClassInternet);
        }

        return buffer.ToArray();
    }

    /// <summary>
    /// Reads a DNS message.
    /// </summary>
    /// <param name="data">The datagram.</param>
    /// <param name="message">The message, when it could be read.</param>
    /// <returns><see langword="true"/> when the message could be read; <see langword="false"/> when it is malformed.</returns>
    public static bool TryParse(
        ReadOnlySpan<byte> data,
        [NotNullWhen(true)] out DnsMessage? message)
    {
        message = null;
        if (data.Length < HeaderLength)
        {
            return false;
        }

        var flags = BinaryPrimitives.ReadUInt16BigEndian(data[2..]);
        var questionCount = BinaryPrimitives.ReadUInt16BigEndian(data[4..]);
        var recordCount = BinaryPrimitives.ReadUInt16BigEndian(data[6..]) +
                          BinaryPrimitives.ReadUInt16BigEndian(data[8..]) +
                          BinaryPrimitives.ReadUInt16BigEndian(data[10..]);

        var offset = HeaderLength;
        for (var i = 0; i < questionCount; i++)
        {
            if (!TryReadName(data, ref offset, out _) ||
                !TrySkip(data, ref offset, 4))
            {
                return false;
            }
        }

        var records = new List<DnsRecord>(recordCount);
        for (var i = 0; i < recordCount; i++)
        {
            if (!TryReadRecord(data, ref offset, out var record))
            {
                return false;
            }

            if (record is not null)
            {
                records.Add(record);
            }
        }

        message = new DnsMessage((flags & ResponseFlag) != 0, records);
        return true;
    }

    /// <summary>
    /// Reads one resource record. <paramref name="record"/> is <see langword="null"/> for a type that is skipped.
    /// </summary>
    private static bool TryReadRecord(
        ReadOnlySpan<byte> data,
        ref int offset,
        out DnsRecord? record)
    {
        record = null;
        if (!TryReadName(data, ref offset, out var name) ||
            offset + 10 > data.Length)
        {
            return false;
        }

        var type = BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
        var ttl = BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..]);
        var dataLength = BinaryPrimitives.ReadUInt16BigEndian(data[(offset + 8)..]);
        offset += 10;

        var dataStart = offset;
        if (dataStart + dataLength > data.Length)
        {
            return false;
        }

        offset += dataLength;
        return TryReadRecordData(data, name, (DnsRecordType)type, ttl, dataStart, dataLength, out record);
    }

    private static bool TryReadRecordData(
        ReadOnlySpan<byte> data,
        string name,
        DnsRecordType type,
        uint ttl,
        int dataStart,
        int dataLength,
        out DnsRecord? record)
    {
        record = null;
        var recordData = data.Slice(dataStart, dataLength);

        switch (type)
        {
            case DnsRecordType.A when dataLength == 4:
            case DnsRecordType.Aaaa when dataLength == 16:
                record = new DnsRecord(name, type, ttl, Address: new IPAddress(recordData));
                return true;
            case DnsRecordType.A:
            case DnsRecordType.Aaaa:
                return false;
            case DnsRecordType.Ptr:
            {
                var nameOffset = dataStart;
                if (!TryReadName(data, ref nameOffset, out var target))
                {
                    return false;
                }

                record = new DnsRecord(name, DnsRecordType.Ptr, ttl, DomainName: target);
                return true;
            }

            case DnsRecordType.Srv:
            {
                var nameOffset = dataStart + 6;
                if (dataLength < 7 ||
                    !TryReadName(data, ref nameOffset, out var target))
                {
                    return false;
                }

                var port = BinaryPrimitives.ReadUInt16BigEndian(recordData[4..]);
                record = new DnsRecord(name, DnsRecordType.Srv, ttl, DomainName: target, Port: port);
                return true;
            }

            case DnsRecordType.Txt:
                if (!TryReadText(recordData, out var text))
                {
                    return false;
                }

                record = new DnsRecord(name, DnsRecordType.Txt, ttl, Text: text);
                return true;
            default:
                return true;
        }
    }

    /// <summary>
    /// Reads a name, following compression pointers. A pointer must point before itself,
    /// so following pointers always ends.
    /// </summary>
    private static bool TryReadName(
        ReadOnlySpan<byte> data,
        ref int offset,
        [NotNullWhen(true)] out string? name)
    {
        name = null;
        var labels = new List<string>();
        var position = offset;
        var wireLength = 1;
        var jumped = false;

        while (true)
        {
            if (position >= data.Length)
            {
                return false;
            }

            var length = data[position];
            if (length == 0)
            {
                position++;
                break;
            }

            if ((length & PointerMask) == PointerMask)
            {
                if (!TryFollowPointer(data, ref position, out var pointerEnd))
                {
                    return false;
                }

                if (!jumped)
                {
                    offset = pointerEnd;
                    jumped = true;
                }

                continue;
            }

            if (length > MaxLabelLength ||
                position + 1 + length > data.Length)
            {
                return false;
            }

            wireLength += length + 1;
            if (wireLength > MaxNameLength)
            {
                return false;
            }

            labels.Add(Encoding.UTF8.GetString(data.Slice(position + 1, length)));
            position += length + 1;
        }

        if (!jumped)
        {
            offset = position;
        }

        name = string.Join('.', labels);
        return true;
    }

    /// <summary>
    /// Moves <paramref name="position"/> to the target of the compression pointer there.
    /// The target must be before the pointer, so following pointers always ends.
    /// </summary>
    private static bool TryFollowPointer(
        ReadOnlySpan<byte> data,
        ref int position,
        out int pointerEnd)
    {
        pointerEnd = position + 2;
        if (position + 1 >= data.Length)
        {
            return false;
        }

        var target = ((data[position] & ~PointerMask) << 8) | data[position + 1];
        if (target >= position)
        {
            return false;
        }

        position = target;
        return true;
    }

    private static bool TryReadText(
        ReadOnlySpan<byte> recordData,
        out IReadOnlyList<string> text)
    {
        var strings = new List<string>();
        text = strings;

        var position = 0;
        while (position < recordData.Length)
        {
            var length = recordData[position];
            if (position + 1 + length > recordData.Length)
            {
                return false;
            }

            if (length > 0)
            {
                strings.Add(Encoding.UTF8.GetString(recordData.Slice(position + 1, length)));
            }

            position += length + 1;
        }

        return true;
    }

    private static bool TrySkip(
        ReadOnlySpan<byte> data,
        ref int offset,
        int count)
    {
        if (offset + count > data.Length)
        {
            return false;
        }

        offset += count;
        return true;
    }

    private static void WriteName(
        List<byte> buffer,
        string name)
    {
        foreach (var label in name.TrimEnd('.').Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            var bytes = Encoding.UTF8.GetBytes(label);
            if (bytes.Length > MaxLabelLength)
            {
                throw new ArgumentException($"The label '{label}' is longer than {MaxLabelLength} bytes.", nameof(name));
            }

            buffer.Add((byte)bytes.Length);
            buffer.AddRange(bytes);
        }

        buffer.Add(0);
    }

    private static void WriteUInt16(
        List<byte> buffer,
        ushort value)
    {
        buffer.Add((byte)(value >> 8));
        buffer.Add((byte)value);
    }
}