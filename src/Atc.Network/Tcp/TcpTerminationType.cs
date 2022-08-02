namespace Atc.Network.Tcp;

public enum TcpTerminationType
{
    None = 0,
    LineFeed = 1,
    CarriageReturn = 2,
    CarriageReturnLineFeed = 3,
}