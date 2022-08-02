namespace Atc.Network.Tcp;

public class TcpClientConfig
{
    /// <summary>
    /// ReceiveBufferSize.
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 1024;

    /// <summary>
    /// The TerminationType.
    /// </summary>
    public TcpTerminationType TerminationType { get; set; } = TcpTerminationType.None;
}