// ReSharper disable once CheckNamespace
namespace Atc.Network;

public enum IPScannerProgressReportingType
{
    None,
    IpAddressStart,
    IpAddressDone,
    Counters,
    Ping,
    HostName,
    MacAddress,
    MacVendor,
    Tcp,
    ServiceHttp,
}