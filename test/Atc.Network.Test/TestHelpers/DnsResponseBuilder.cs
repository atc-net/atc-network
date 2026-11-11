namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// Builds mDNS responses for fake responders (names are written without compression),
/// and reads the questions of a query.
/// </summary>
internal sealed class DnsResponseBuilder
{
    private readonly List<byte> records = [];
    private int recordCount;

    public DnsResponseBuilder PointerRecord(
        string name,
        string target)
        => Add(name, 12, Name(target));

    public DnsResponseBuilder ServiceRecord(
        string name,
        string host,
        ushort port)
        => Add(name, 33, [0, 0, 0, 0, (byte)(port >> 8), (byte)port, .. Name(host)]);

    public DnsResponseBuilder TextRecord(
        string name,
        params string[] strings)
        => Add(name, 16, strings.SelectMany(x => new[] { (byte)Encoding.UTF8.GetByteCount(x) }.Concat(Encoding.UTF8.GetBytes(x))).ToArray());

    public DnsResponseBuilder AddressRecord(
        string name,
        string address)
        => Add(name, 1, IPAddress.Parse(address).GetAddressBytes());

    public byte[] Build()
    {
        // id 0, flags 0x8400 (response, authoritative), 0 questions, all records as answers.
        byte[] header = [0x00, 0x00, 0x84, 0x00, 0x00, 0x00, (byte)(recordCount >> 8), (byte)recordCount, 0x00, 0x00, 0x00, 0x00];
        return [.. header, .. records];
    }

    /// <summary>
    /// Reads the questions of a query, as (name, type) pairs. Names must not be compressed.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>The questions.</returns>
    public static IReadOnlyList<(string Name, int Type)> ReadQuestions(
        byte[] query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var questions = new List<(string Name, int Type)>();
        var count = (query[4] << 8) | query[5];
        var offset = 12;
        for (var i = 0; i < count; i++)
        {
            var labels = new List<string>();
            while (query[offset] != 0)
            {
                labels.Add(Encoding.UTF8.GetString(query, offset + 1, query[offset]));
                offset += query[offset] + 1;
            }

            offset++;
            questions.Add((string.Join('.', labels), (query[offset] << 8) | query[offset + 1]));
            offset += 4;
        }

        return questions;
    }

    private static byte[] Name(string name)
        => name
            .Split('.')
            .SelectMany(x => new[] { (byte)Encoding.UTF8.GetByteCount(x) }.Concat(Encoding.UTF8.GetBytes(x)))
            .Append((byte)0)
            .ToArray();

    private DnsResponseBuilder Add(
        string name,
        ushort type,
        byte[] data)
    {
        // type, class IN with cache-flush, TTL 120, RDLENGTH, RDATA
        records.AddRange(Name(name));
        records.AddRange([(byte)(type >> 8), (byte)type, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, (byte)(data.Length >> 8), (byte)data.Length]);
        records.AddRange(data);
        recordCount++;
        return this;
    }
}