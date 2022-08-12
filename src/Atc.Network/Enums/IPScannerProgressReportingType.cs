// ReSharper disable once CheckNamespace
namespace Atc.Network;

public enum IPScannerProgressReportingType
{
    None,
    IPAddressStart,
    IPAddressDone,
    Counters,
    Ping,
    HostName,
    MacAddress,
    MacVendor,
    Tcp,
    ServiceHttp,
}