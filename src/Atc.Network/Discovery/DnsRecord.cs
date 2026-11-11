namespace Atc.Network.Discovery;

/// <summary>
/// A resource record from a DNS response.
/// </summary>
/// <param name="Name">The record's name.</param>
/// <param name="Type">The record type.</param>
/// <param name="Ttl">The time to live, in seconds.</param>
/// <param name="DomainName">The PTR record's target, or the SRV record's target host.</param>
/// <param name="Port">The SRV record's port.</param>
/// <param name="Address">The A or AAAA record's address.</param>
/// <param name="Text">The TXT record's strings.</param>
internal sealed record DnsRecord(
    string Name,
    DnsRecordType Type,
    uint Ttl,
    string? DomainName = null,
    int Port = 0,
    IPAddress? Address = null,
    IReadOnlyList<string>? Text = null);