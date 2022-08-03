namespace Atc.Network.Extensions;

[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "OK.")]
public static class ExceptionExtensions
{
    public static (bool IsKnownException, SocketError? SocketError) IsKnownExceptionForNetworkCableUnplugged(
        this Exception exception)
    {
        var (isKnownSocketException, socketError) = exception.IsKnownSocketExceptionForNetworkCableUnplugged();
        if (isKnownSocketException)
        {
            return (isKnownSocketException, socketError);
        }

        return exception.InnerException?.IsKnownSocketExceptionForNetworkCableUnplugged() ?? (false, null);
    }

    public static (bool IsKnownSocketException, SocketError? SocketError) IsKnownExceptionForConsumerDisposed(
        this Exception exception)
    {
        var (isKnownSocketException, socketError) = exception.IsKnownSocketExceptionForConsumerDisposed();
        if (isKnownSocketException)
        {
            return (isKnownSocketException, socketError);
        }

        return exception.InnerException?.IsKnownSocketExceptionForConsumerDisposed() ?? (false, null);
    }

    private static (bool IsKnownSocketException, SocketError? SocketError) IsKnownSocketExceptionForNetworkCableUnplugged(
        this Exception exception)
    {
        if (exception is not SocketException socketException)
        {
            return (false, null);
        }

        return socketException.SocketErrorCode switch
        {
            SocketError.TimedOut => (true, socketException.SocketErrorCode),
            SocketError.ConnectionReset => (true, socketException.SocketErrorCode),
            _ => (false, null),
        };
    }

    private static (bool IsKnownSocketException, SocketError? SocketError) IsKnownSocketExceptionForConsumerDisposed(
        this Exception exception)
    {
        if (exception is not SocketException socketException)
        {
            return (false, null);
        }

        return socketException.SocketErrorCode switch
        {
            SocketError.OperationAborted => (true, SocketError: socketException.SocketErrorCode),
            _ => (false, null),
        };
    }
}