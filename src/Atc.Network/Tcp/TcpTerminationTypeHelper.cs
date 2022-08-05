// ReSharper disable CommentTypo
// ReSharper disable StaticMemberInitializerReferesToMemberBelow
namespace Atc.Network.Tcp;

public static class TcpTerminationTypeHelper
{
    public const byte LineFeed = 0x0A;
    public const byte CarriageReturn = 0x0D;

    private static readonly byte[] LineFeedAsBytes = { LineFeed };
    private static readonly byte[] CarriageReturnAsBytes = { CarriageReturn };
    private static readonly byte[] CarriageReturnLineFeedAsBytes = { LineFeed, CarriageReturn };

    public static string ConvertToString(
        TcpTerminationType tcpTerminationType)
        => tcpTerminationType switch
        {
            TcpTerminationType.None => string.Empty,
            TcpTerminationType.LineFeed => "\n",
            TcpTerminationType.CarriageReturn => "\r",
            TcpTerminationType.CarriageReturnLineFeed => "\r\n",
            _ => throw new SwitchCaseDefaultException(tcpTerminationType),
        };

    public static byte[] ConvertToBytes(
        TcpTerminationType tcpTerminationType)
        => tcpTerminationType switch
        {
            TcpTerminationType.None => Array.Empty<byte>(),
            TcpTerminationType.LineFeed => LineFeedAsBytes,
            TcpTerminationType.CarriageReturn => CarriageReturnAsBytes,
            TcpTerminationType.CarriageReturnLineFeed => CarriageReturnLineFeedAsBytes,
            _ => throw new SwitchCaseDefaultException(tcpTerminationType),
        };
}