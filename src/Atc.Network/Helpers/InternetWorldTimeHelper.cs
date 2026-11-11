namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for fetching the current time from the WorldTimeAPI.
/// </summary>
/// <remarks>
/// For new code, <see cref="NtpHelper.GetTime(string, TimeSpan?, CancellationToken)"/> is the better way to get the time.
/// It asks an NTP server directly, which is the standard for time synchronization and is not tied to one web service.
/// It works on a LAN without internet access (against a router or a domain controller), reports failures
/// with an exception instead of <see langword="null"/>, and also returns the local clock's offset and the round-trip delay.
/// NTP gives UTC; for the time in a time zone, use
/// <c>TimeZoneInfo.ConvertTimeFromUtc(result.UtcTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"))</c>.
/// </remarks>
public static class InternetWorldTimeHelper
{
    private const string WorldTimeBaseApi = "https://worldtimeapi.org/api/timezone/";
    private const int SyncLockTimeoutInMs = 30_000;
    private static readonly SemaphoreSlim SyncLock = new(1, 1);

    /// <summary>
    /// Retrieves the current time for the Europe/Berlin timezone.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="DateTime"/> representing the current time in Europe/Berlin timezone, or null if the operation fails or is canceled.
    /// </returns>
    /// <remarks>
    /// For new code, <see cref="NtpHelper.GetTime(string, TimeSpan?, CancellationToken)"/> is recommended instead; see the class remarks.
    /// </remarks>
    public static Task<DateTime?> GetTimeForEuropeBerlin(
        CancellationToken cancellationToken)
        => GetTimeForWorldTimezone("europe/Berlin", cancellationToken);

    /// <summary>
    /// Retrieves the current time for a specified WorldTimeAPI timezone.
    /// </summary>
    /// <param name="worldTimezone">The timezone string as defined by WorldTimeAPI.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="DateTime"/> representing the current time in the specified timezone, or null if the operation fails or is canceled.
    /// </returns>
    /// <remarks>
    /// This method ensures thread safety by using a SemaphoreSlim to limit concurrent access. It checks for network connectivity before making the HTTP request. If the network is unavailable or the request fails, the method returns null.
    /// <para>
    /// For new code, <see cref="NtpHelper.GetTime(string, TimeSpan?, CancellationToken)"/> is recommended instead:
    /// it uses NTP, the standard time protocol, reports why it failed, and converts to a time zone with <see cref="TimeZoneInfo"/>.
    /// </para>
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
    public static async Task<DateTime?> GetTimeForWorldTimezone(
        string worldTimezone,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(worldTimezone);

        var lockAcquired = false;

        try
        {
            lockAcquired = await SyncLock.WaitAsync(SyncLockTimeoutInMs, cancellationToken);

            if (!lockAcquired)
            {
                return null;
            }

            if (!await NetworkInformationHelper.HasConnectionAsync(cancellationToken))
            {
                return null;
            }

            var uri = new Uri(WorldTimeBaseApi + worldTimezone);

            var response = await SharedHttpClients.Api.GetStringAsync(uri, cancellationToken);
            var timeInfo = JsonSerializer.Deserialize<TimeResponse>(response);

            return timeInfo?.DateTime;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (lockAcquired)
            {
                SyncLock.Release();
            }
        }
    }

    /// <summary>
    /// Internal class to deserialize the JSON response from WorldTimeAPI.
    /// </summary>
    private sealed class TimeResponse
    {
        [JsonPropertyName("datetime")]
        public DateTime? DateTime { get; init; }
    }
}