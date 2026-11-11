// ReSharper disable once CheckNamespace
namespace Atc.Network;

/// <summary>
/// How a <see cref="Tcp.TcpClient"/> splits the TCP byte stream into messages.
/// </summary>
public enum TcpMessageFraming
{
    /// <summary>
    /// No framing: <c>DataReceived</c> is raised once per read from the socket, which can hold
    /// part of a message, a whole message or several messages. Sends append the <see cref="TerminationType"/>.
    /// </summary>
    None = 0,

    /// <summary>
    /// Received data is collected until the configured <see cref="TerminationType"/>, and <c>DataReceived</c>
    /// is raised once per message, including the terminator. Sends append the <see cref="TerminationType"/>.
    /// </summary>
    TerminationType = 1,

    /// <summary>
    /// Every message starts with its length as a 4-byte big-endian integer. Sends add the length,
    /// and <c>DataReceived</c> is raised once per message, without the length.
    /// The <see cref="TerminationType"/> is not used.
    /// </summary>
    LengthPrefix = 2,
}