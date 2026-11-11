namespace Atc.Network.Tcp;

/// <summary>
/// Splits a TCP byte stream into messages, for <see cref="TcpClientConfig.MessageFraming"/>.
/// Bytes of an incomplete message are kept until the rest arrives.
/// </summary>
internal sealed class TcpMessageFramer
{
    private const int LengthPrefixSize = 4;

    private readonly TcpMessageFraming framing;
    private readonly byte[] terminator;
    private readonly int maxMessageSize;
    private byte[] buffer = new byte[1024];
    private int count;

    public TcpMessageFramer(
        TcpMessageFraming framing,
        TerminationType terminationType,
        int maxMessageSize)
    {
        this.framing = framing;
        this.maxMessageSize = maxMessageSize;
        terminator = framing == TcpMessageFraming.TerminationType
            ? TerminationTypeHelper.ConvertToBytes(terminationType)
            : [];
    }

    /// <summary>
    /// Adds a 4-byte big-endian length in front of the data.
    /// </summary>
    /// <param name="data">The message.</param>
    /// <returns>The length followed by the message.</returns>
    public static byte[] AddLengthPrefix(byte[] data)
    {
        var framed = new byte[LengthPrefixSize + data.Length];
        BinaryPrimitives.WriteInt32BigEndian(framed, data.Length);
        data.CopyTo(framed, LengthPrefixSize);
        return framed;
    }

    /// <summary>
    /// Drops the bytes of an incomplete message, for example after a reconnect.
    /// </summary>
    public void Reset()
        => count = 0;

    /// <summary>
    /// Adds received bytes and returns the messages they complete, in order.
    /// </summary>
    /// <param name="data">The bytes of one read.</param>
    /// <returns>The complete messages; empty when the read ended in the middle of a message.</returns>
    /// <exception cref="InvalidDataException">A message is longer than the maximum message size.</exception>
    public IReadOnlyList<byte[]> Append(ReadOnlySpan<byte> data)
    {
        EnsureCapacity(count + data.Length);
        data.CopyTo(buffer.AsSpan(count));

        // A terminator can start in an earlier read, so look back a few bytes.
        var searchFrom = System.Math.Max(0, count - (terminator.Length - 1));
        count += data.Length;

        var messages = new List<byte[]>();
        var consumed = framing == TcpMessageFraming.LengthPrefix
            ? ReadLengthPrefixedMessages(messages)
            : ReadTerminatedMessages(messages, searchFrom);

        if (consumed > 0)
        {
            Buffer.BlockCopy(buffer, consumed, buffer, 0, count - consumed);
            count -= consumed;
        }

        if (framing == TcpMessageFraming.TerminationType && count > maxMessageSize)
        {
            count = 0;
            throw new InvalidDataException($"No terminator within the maximum message size of {maxMessageSize} bytes.");
        }

        return messages;
    }

    private int ReadTerminatedMessages(
        List<byte[]> messages,
        int searchFrom)
    {
        var start = 0;
        var position = searchFrom;
        while (true)
        {
            var index = buffer.AsSpan(position, count - position).IndexOf(terminator);
            if (index < 0)
            {
                return start;
            }

            var end = position + index + terminator.Length;
            if (end - start - terminator.Length > maxMessageSize)
            {
                count = 0;
                throw new InvalidDataException($"A message is longer than the maximum message size of {maxMessageSize} bytes.");
            }

            messages.Add(buffer.AsSpan(start, end - start).ToArray());
            start = end;
            position = end;
        }
    }

    private int ReadLengthPrefixedMessages(List<byte[]> messages)
    {
        var start = 0;
        while (count - start >= LengthPrefixSize)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(start, LengthPrefixSize));
            if (length < 0 || length > maxMessageSize)
            {
                count = 0;
                throw new InvalidDataException($"A message length of {length} bytes is outside 0 to the maximum message size of {maxMessageSize} bytes.");
            }

            if (count - start - LengthPrefixSize < length)
            {
                break;
            }

            messages.Add(buffer.AsSpan(start + LengthPrefixSize, length).ToArray());
            start += LengthPrefixSize + length;
        }

        return start;
    }

    private void EnsureCapacity(int needed)
    {
        if (needed <= buffer.Length)
        {
            return;
        }

        var newBuffer = new byte[System.Math.Max(needed, buffer.Length * 2)];
        Buffer.BlockCopy(buffer, 0, newBuffer, 0, count);
        buffer = newBuffer;
    }
}