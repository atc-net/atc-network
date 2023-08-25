// ReSharper disable once CheckNamespace
namespace Atc.Network;

public enum TerminationType
{
    /// <summary>
    /// The none - known as '\0'.
    /// </summary>
    None = 0,

    /// <summary>
    /// The line feed - know as '\n' (0x0A).
    /// </summary>
    LineFeed = 1,

    /// <summary>
    /// The carriage return - know as '\r' (0x0D).
    /// </summary>
    CarriageReturn = 2,

    /// <summary>
    /// The carriage return line feed - know as '\r\n' (0x0D, 0x0A).
    /// </summary>
    CarriageReturnLineFeed = 3,

    /// <summary>
    /// The end of transmission - known as ETX (0x03).
    /// </summary>
    EndOfText = 4,

    /// <summary>
    /// The end of transmission - known as EOT (0x04).
    /// </summary>
    EndOfTransmission = 5,
}