namespace Atc.Network.Models;

public class IPScanPortResult
{
    public IPAddress? IPAddress { get; set; }

    public int Port { get; set; }

    public IPProtocolType Protocol { get; set; }

    public bool CanConnect { get; set; }

    public override string ToString()
        => $"{nameof(IPAddress)}: {IPAddress}, {nameof(Port)}: {Port}, {nameof(Protocol)}: {Protocol}, {nameof(CanConnect)}: {CanConnect}";
}