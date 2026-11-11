namespace Atc.Network.Test.Discovery;

public sealed class DnsMessageTests
{
    /// <summary>
    /// An mDNS response, written by hand, with compression pointers: one PTR answer and
    /// SRV, TXT, A, AAAA and NSEC (an unknown type, which is skipped) records in the additional section.
    /// </summary>
    private static readonly byte[] Response =
    [
        // 0: header - id 0, flags 0x8400 (response, authoritative), 0 questions, 1 answer, 0 authority, 5 additional
        0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05,

        // 12: answer name "_http._tcp.local" (5 _http, 4 _tcp, 5 local, 0); "local" starts at 23
        0x05, (byte)'_', (byte)'h', (byte)'t', (byte)'t', (byte)'p',
        0x04, (byte)'_', (byte)'t', (byte)'c', (byte)'p',
        0x05, (byte)'l', (byte)'o', (byte)'c', (byte)'a', (byte)'l',
        0x00,

        // 30: type PTR, class IN, TTL 4500, RDLENGTH 10
        0x00, 0x0C, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x0A,

        // 40: RDATA "Printer" + pointer to 12 => "Printer._http._tcp.local"
        0x07, (byte)'P', (byte)'r', (byte)'i', (byte)'n', (byte)'t', (byte)'e', (byte)'r', 0xC0, 0x0C,

        // 50: SRV - name pointer to 40, type 33, class IN with cache-flush, TTL 120, RDLENGTH 16
        0xC0, 0x28, 0x00, 0x21, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x10,

        // 62: priority 0, weight 0, port 8080, target "printer" (at 68) + pointer to 23 => "printer.local"
        0x00, 0x00, 0x00, 0x00, 0x1F, 0x90,
        0x07, (byte)'p', (byte)'r', (byte)'i', (byte)'n', (byte)'t', (byte)'e', (byte)'r', 0xC0, 0x17,

        // 78: TXT - name pointer to 40, type 16, class IN with cache-flush, TTL 4500, RDLENGTH 13
        0xC0, 0x28, 0x00, 0x10, 0x80, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x0D,

        // 90: "path=/x", "flag"
        0x07, (byte)'p', (byte)'a', (byte)'t', (byte)'h', (byte)'=', (byte)'/', (byte)'x',
        0x04, (byte)'f', (byte)'l', (byte)'a', (byte)'g',

        // 103: A - name pointer to 68 ("printer.local"), type 1, class IN with cache-flush, TTL 120, RDLENGTH 4, 192.168.1.10
        0xC0, 0x44, 0x00, 0x01, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x04, 0xC0, 0xA8, 0x01, 0x0A,

        // 119: AAAA - name pointer to 68, type 28, TTL 120, RDLENGTH 16, fe80::1
        0xC0, 0x44, 0x00, 0x1C, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x10,
        0xFE, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,

        // 147: NSEC (type 47) - name pointer to 68, RDLENGTH 5; skipped
        0xC0, 0x44, 0x00, 0x2F, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x05, 0xC0, 0x44, 0x00, 0x01, 0x40,
    ];

    [Fact]
    public void CreateQuery_WritesTheQuestionWithTheUnicastResponseBit()
    {
        // Act
        var actual = DnsMessage.CreateQuery([new DnsQuestion("_http._tcp.local", DnsRecordType.Ptr)]);

        // Assert - header (id 0, flags 0, 1 question), the name, type PTR (12), class IN (1) with the QU bit (0x8000).
        byte[] expected =
        [
            0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x05, (byte)'_', (byte)'h', (byte)'t', (byte)'t', (byte)'p',
            0x04, (byte)'_', (byte)'t', (byte)'c', (byte)'p',
            0x05, (byte)'l', (byte)'o', (byte)'c', (byte)'a', (byte)'l',
            0x00,
            0x00, 0x0C, 0x80, 0x01,
        ];
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreateQuery_SeveralQuestions_CountsThem()
    {
        // Act
        var actual = DnsMessage.CreateQuery(
        [
            new DnsQuestion("Printer._http._tcp.local", DnsRecordType.Srv),
            new DnsQuestion("Printer._http._tcp.local", DnsRecordType.Txt),
        ]);

        // Assert
        Assert.Equal(2, BinaryPrimitives.ReadUInt16BigEndian(actual.AsSpan(4)));
        Assert.True(DnsMessage.TryParse(actual, out var parsed));
        Assert.False(parsed.IsResponse);
    }

    [Fact]
    public void TryParse_Response_ReadsTheRecordsAndFollowsCompressionPointers()
    {
        // Act
        var success = DnsMessage.TryParse(Response, out var actual);

        // Assert
        Assert.True(success);
        Assert.True(actual.IsResponse);
        Assert.Equal(5, actual.Records.Count);

        var ptr = Assert.Single(actual.Records, x => x.Type == DnsRecordType.Ptr);
        Assert.Equal("_http._tcp.local", ptr.Name);
        Assert.Equal("Printer._http._tcp.local", ptr.DomainName);
        Assert.Equal(4500u, ptr.Ttl);

        var srv = Assert.Single(actual.Records, x => x.Type == DnsRecordType.Srv);
        Assert.Equal("Printer._http._tcp.local", srv.Name);
        Assert.Equal("printer.local", srv.DomainName);
        Assert.Equal(8080, srv.Port);

        var txt = Assert.Single(actual.Records, x => x.Type == DnsRecordType.Txt);
        Assert.Equal(["path=/x", "flag"], txt.Text);

        var a = Assert.Single(actual.Records, x => x.Type == DnsRecordType.A);
        Assert.Equal("printer.local", a.Name);
        Assert.Equal(IPAddress.Parse("192.168.1.10"), a.Address);

        var aaaa = Assert.Single(actual.Records, x => x.Type == DnsRecordType.Aaaa);
        Assert.Equal(IPAddress.Parse("fe80::1"), aaaa.Address);
    }

    [Fact]
    public void TryParse_TruncatedAnywhere_ReturnsFalseWithoutThrowing()
    {
        for (var length = 0; length < Response.Length; length++)
        {
            Assert.False(DnsMessage.TryParse(Response.AsSpan(0, length), out _), $"Length {length} should not parse.");
        }
    }

    [Fact]
    public void TryParse_PointerToItself_ReturnsFalse()
        => Assert.False(DnsMessage.TryParse(ResponseWithAnswerName([0xC0, 0x0C]), out _));

    [Fact]
    public void TryParse_ForwardPointer_ReturnsFalse()
        => Assert.False(DnsMessage.TryParse(ResponseWithAnswerName([0xC0, 0x0E, 0x01, (byte)'a', 0x00]), out _));

    [Fact]
    public void TryParse_LabelLongerThan63_ReturnsFalse()
    {
        // A length byte of 64 (0x40) uses the reserved label type bits.
        var name = new byte[66];
        name[0] = 64;
        Assert.False(DnsMessage.TryParse(ResponseWithAnswerName(name), out _));
    }

    [Fact]
    public void TryParse_NameLongerThan255_ReturnsFalse()
    {
        // 5 labels of 63 bytes: 5 * 64 + 1 = 321 bytes on the wire.
        var name = new List<byte>();
        for (var i = 0; i < 5; i++)
        {
            name.Add(63);
            name.AddRange(Enumerable.Repeat((byte)'a', 63));
        }

        name.Add(0);
        Assert.False(DnsMessage.TryParse(ResponseWithAnswerName(name.ToArray()), out _));
    }

    [Fact]
    public void TryParse_AddressRecordOfWrongLength_ReturnsFalse()
    {
        // An A record with 5 bytes of data.
        byte[] message =
        [
            0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x01, (byte)'a', 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x05, 1, 2, 3, 4, 5,
        ];
        Assert.False(DnsMessage.TryParse(message, out _));
    }

    [Fact]
    public void TryParse_Query_SkipsTheQuestions()
    {
        // Arrange - a response that repeats the question "a.local" (type A), as legacy unicast responses do,
        // then answers it with 10.0.0.1.
        byte[] message =
        [
            0x00, 0x00, 0x84, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x01, (byte)'a', 0x05, (byte)'l', (byte)'o', (byte)'c', (byte)'a', (byte)'l', 0x00, 0x00, 0x01, 0x80, 0x01,
            0x01, (byte)'a', 0x05, (byte)'l', (byte)'o', (byte)'c', (byte)'a', (byte)'l', 0x00, 0x00, 0x01, 0x80, 0x01,
            0x00, 0x00, 0x00, 0x78, 0x00, 0x04, 10, 0, 0, 1,
        ];

        // Act
        var success = DnsMessage.TryParse(message, out var actual);

        // Assert
        Assert.True(success);
        var record = Assert.Single(actual.Records);
        Assert.Equal("a.local", record.Name);
        Assert.Equal(IPAddress.Parse("10.0.0.1"), record.Address);
    }

    /// <summary>
    /// A response with one A record whose name is the given bytes.
    /// </summary>
    private static byte[] ResponseWithAnswerName(byte[] name)
    {
        byte[] header = [0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00];
        byte[] rest = [0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x04, 127, 0, 0, 1];
        return [.. header, .. name, .. rest];
    }
}