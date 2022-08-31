// ReSharper disable CommentTypo
// ReSharper disable StaticMemberInitializerReferesToMemberBelow
namespace Atc.Network.Helpers;

public static class TerminationTypeHelper
{
    public const byte LineFeed = 0x0A;
    public const byte CarriageReturn = 0x0D;

    private static readonly byte[] LineFeedAsBytes = { LineFeed };
    private static readonly byte[] CarriageReturnAsBytes = { CarriageReturn };
    private static readonly byte[] CarriageReturnLineFeedAsBytes = { LineFeed, CarriageReturn };

    public static string ConvertToString(
        TerminationType terminationType)
        => terminationType switch
        {
            TerminationType.None => string.Empty,
            TerminationType.LineFeed => "\n",
            TerminationType.CarriageReturn => "\r",
            TerminationType.CarriageReturnLineFeed => "\r\n",
            _ => throw new SwitchCaseDefaultException(terminationType),
        };

    public static byte[] ConvertToBytes(
        TerminationType terminationType)
        => terminationType switch
        {
            TerminationType.None => Array.Empty<byte>(),
            TerminationType.LineFeed => LineFeedAsBytes,
            TerminationType.CarriageReturn => CarriageReturnAsBytes,
            TerminationType.CarriageReturnLineFeed => CarriageReturnLineFeedAsBytes,
            _ => throw new SwitchCaseDefaultException(terminationType),
        };
}