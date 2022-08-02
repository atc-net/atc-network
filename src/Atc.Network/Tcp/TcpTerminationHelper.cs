namespace Atc.Network.Tcp;

public static class TcpTerminationHelper
{
    public static string GetTermination(
        TcpTerminationType tcpTerminationType)
        => tcpTerminationType switch
        {
            TcpTerminationType.None => string.Empty,
            TcpTerminationType.LineFeed => "\n",
            TcpTerminationType.CarriageReturn => "\r",
            TcpTerminationType.CarriageReturnLineFeed => "\r\n",
            _ => throw new SwitchCaseDefaultException(tcpTerminationType),
        };
}