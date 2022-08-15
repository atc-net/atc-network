namespace Atc.Network.Models;

public class ArpEntity
{
    public ArpEntity(
        IPAddress ipAddress,
        string macAddress,
        string type)
    {
        IPAddress = ipAddress;
        MacAddress = macAddress;
        Type = type;
    }

    public IPAddress IPAddress { get; }

    public string MacAddress { get; }

    public string Type { get; }

    public override string ToString()
        => $"{nameof(IPAddress)}: {IPAddress}, {nameof(MacAddress)}: {MacAddress}, {nameof(Type)}: {Type}";
}