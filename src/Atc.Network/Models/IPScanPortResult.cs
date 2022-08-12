namespace Atc.Network.Models;

public class IPScanPortResult
{
    public IPAddress? IPAddress { get; set; }

    public ushort Port { get; set; }

    public TransportProtocolType TransportProtocol { get; set; }

    public ServiceProtocolType ServiceProtocol { get; set; }

    public bool CanConnect { get; set; }

    public override string ToString()
        => $"{nameof(IPAddress)}: {IPAddress}, {nameof(Port)}: {Port}, {nameof(TransportProtocol)}: {TransportProtocol}, {nameof(ServiceProtocol)}: {ServiceProtocol}, {nameof(CanConnect)}: {CanConnect}";
}