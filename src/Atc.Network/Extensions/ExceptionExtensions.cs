namespace Atc.Network.Extensions;

public static class ExceptionExtensions
{
    public static (bool IsKnownException, SocketError? SocketError) IsKnownException(
        this Exception exception)
    {
        if (exception is not AggregateException aggregateException)
        {
            return (false, null);
        }

        if (aggregateException.InnerException is not IOException ioException)
        {
            return (false, null);
        }

        if (ioException.InnerException is not SocketException socketException)
        {
            return (false, null);
        }

        return socketException.SocketErrorCode switch
        {
            // Network cable unplugged at endpoint
            SocketError.TimedOut => (true, socketException.SocketErrorCode),
            SocketError.ConnectionReset => (true, socketException.SocketErrorCode),
            _ => (false, null),
        };
    }
}