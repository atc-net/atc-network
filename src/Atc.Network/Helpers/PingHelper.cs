namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for performing ping operations to assess network connectivity and response time.
/// </summary>
public static class PingHelper
{
    /// <summary>
    /// Initiates a ping request to a specified IP address with a timeout.
    /// </summary>
    /// <param name="ipAddress">The IP address to ping.</param>
    /// <param name="timeout">The maximum amount of time to wait for a ping response.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in a <see cref="PingStatusResult"/>.
    /// </returns>
    /// <remarks>
    /// This overload accepts a <see cref="TimeSpan"/> for the timeout and converts it to milliseconds
    /// before calling the main asynchronous ping method.
    /// </remarks>
    public static Task<PingStatusResult> GetStatus(
        IPAddress ipAddress,
        TimeSpan timeout)
        => GetStatus(ipAddress, (int)timeout.TotalMilliseconds);

    /// <summary>
    /// Initiates a ping request to a specified IP address with a timeout specified in milliseconds.
    /// </summary>
    /// <param name="ipAddress">The IP address to ping.</param>
    /// <param name="timeoutInMs">The maximum amount of time, in milliseconds, to wait for a ping response.
    /// Defaults to 1000 milliseconds.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in a <see cref="PingStatusResult"/>,
    /// which includes the IP address, ping status, and response time.
    /// </returns>
    /// <remarks>
    /// This method sends an asynchronous ping request to the specified IP address, measuring the response time.
    /// If an exception occurs during the ping operation, it returns a <see cref="PingStatusResult"/> with the exception details.
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
    public static async Task<PingStatusResult> GetStatus(
        IPAddress ipAddress,
        int timeoutInMs = 1000)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var ping = new Ping();

            var pingReply = await ping.SendPingAsync(ipAddress, timeoutInMs);
            sw.Stop();

            return new PingStatusResult(ipAddress, pingReply.Status, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return new PingStatusResult(ipAddress, ex);
        }
    }
}