namespace Atc.Network.Discovery;

/// <summary>
/// The DNS record types that mDNS discovery reads. Other types are skipped.
/// </summary>
internal enum DnsRecordType : ushort
{
    A = 1,
    Ptr = 12,
    Txt = 16,
    Aaaa = 28,
    Srv = 33,
}