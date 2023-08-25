// ReSharper disable CommentTypo
// ReSharper disable StaticMemberInitializerReferesToMemberBelow
namespace Atc.Network.Helpers;

public static class TerminationTypeHelper
{
    public const byte LineFeed = 0x0A;
    public const byte CarriageReturn = 0x0D;
    public const byte EndOfText = 0x03;
    public const byte EndOfTransmission = 0x04;

    private static readonly byte[] LineFeedAsBytes = { LineFeed };
    private static readonly byte[] CarriageReturnAsBytes = { CarriageReturn };
    private static readonly byte[] CarriageReturnLineFeedAsBytes = { LineFeed, CarriageReturn };
    private static readonly byte[] EndOfTextAsBytes = { EndOfText };
    private static readonly byte[] EndOfTransmissionAsBytes = { EndOfTransmission };

    public static string ConvertToString(
        TerminationType terminationType)
        => terminationType switch
        {
            TerminationType.None => string.Empty,
            TerminationType.LineFeed => "\n",
            TerminationType.CarriageReturn => "\r",
            TerminationType.CarriageReturnLineFeed => "\r\n",
            TerminationType.EndOfText => string.Empty,
            TerminationType.EndOfTransmission => string.Empty,
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
            TerminationType.EndOfText => EndOfTextAsBytes,
            TerminationType.EndOfTransmission => EndOfTransmissionAsBytes,
            _ => throw new SwitchCaseDefaultException(terminationType),
        };

    public static bool HasTerminationType(
        TerminationType terminationType,
        byte[] data)
    {
        switch (terminationType)
        {
            case TerminationType.None:
                return true;
            case TerminationType.LineFeed:
                return data.Contains(LineFeed);
            case TerminationType.CarriageReturn:
                return data.Contains(CarriageReturn);
            case TerminationType.CarriageReturnLineFeed:
                var s = Encoding.ASCII.GetString(data);
                return s.Contains("\r\n", StringComparison.Ordinal) ||
                       s.Contains("\n\r", StringComparison.Ordinal);
            case TerminationType.EndOfText:
                return data.Contains(EndOfText);
            case TerminationType.EndOfTransmission:
                return data.Contains(EndOfTransmission);
            default:
                throw new SwitchCaseDefaultException(terminationType);
        }
    }
}