namespace Atc.Network.Helpers;

public static class PingHelper
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
    public static PingStatusResult GetStatus(
        IPAddress ipAddress,
        int timeoutInMs = 1000)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var ping = new Ping();
            var ipStatus = ping.Send(ipAddress, timeoutInMs).Status;
            sw.Stop();

            return new PingStatusResult(ipAddress, ipStatus, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return new PingStatusResult(ipAddress, ex);
        }
    }
}