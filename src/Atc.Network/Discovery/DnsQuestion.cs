namespace Atc.Network.Discovery;

/// <summary>
/// A question in a DNS query.
/// </summary>
/// <param name="Name">The name asked for, such as "_http._tcp.local".</param>
/// <param name="Type">The record type asked for.</param>
internal sealed record DnsQuestion(
    string Name,
    DnsRecordType Type);