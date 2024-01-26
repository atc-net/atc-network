namespace Atc.Network;

public static class LoggingEventIdConstants
{
    internal static class TcpClient
    {
        public const int Connecting = 10000;
        public const int Connected = 10001;
        public const int Reconnecting = 10002;
        public const int Reconnected = 10003;
        public const int ConnectionError = 10004;
        public const int ReconnectionWarning = 10005;
        public const int ReconnectionMaxRetryExceededError = 10006;
        public const int ClientNotConnected = 10007;
        public const int Disconnecting = 10008;
        public const int Disconnected = 10009;

        public const int DataSendingByteLength = 10010;
        public const int DataSendingSocketError = 10011;
        public const int DataSendingError = 10012;
        public const int DataReceivedByteLength = 10013;
        public const int DataReceiveTimeout = 10014;
        public const int DataReceiveNoData = 10015;
        public const int DataReceiveError = 10016;
    }

    internal static class TcpServer
    {
        public const int NotRunning = 10120;
        public const int Starting = 10121;
        public const int Started = 10122;
        public const int Stopping = 10123;
        public const int Stopped = 10124;
        public const int DataReceivedByteLength = 10111;
        public const int DataReceivedChunkByteLength = 10112;
    }

    internal static class UdpClient
    {
        public const int Connecting = 10200;
        public const int Connected = 10201;
        public const int ConnectionError = 10204;
        public const int ClientNotConnected = 10207;
        public const int Disconnecting = 10208;
        public const int Disconnected = 10209;
    }

    internal static class UdpServer
    {
        public const int DataReceivedByteLength = 10311;
        public const int NotRunning = 10320;
    }
}