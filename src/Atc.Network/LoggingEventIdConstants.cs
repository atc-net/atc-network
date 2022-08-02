namespace Atc.Network;

public static class LoggingEventIdConstants
{
    public const int Connecting = 10000;
    public const int Connected = 10001;
    public const int ConnectionError = 10002;
    public const int ClientNotConnected = 10003;
    public const int Disconnecting = 10004;
    public const int Disconnected = 10005;

    public const int DataSendingByteLength = 10010;
    public const int DataReceivedByteLength = 10011;
    public const int DataReceiveTimeout = 10012;
    public const int DataReceiveNoData = 10013;
    public const int DataReceiveError = 10014;
}