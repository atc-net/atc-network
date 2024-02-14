namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for fetching the current time from the WorldTimeAPI.
/// </summary>
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
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
    public static async Task<DateTime?> GetTimeForWorldTimezone(
        string worldTimezone,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(worldTimezone);

        try
        {
            await SyncLock.WaitAsync(SyncLockTimeoutInMs, cancellationToken);

            if (!NetworkInformationHelper.HasConnection())
            {
                return null;
            }

            var uri = new Uri(WorldTimeBaseApi + worldTimezone);

            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(uri, cancellationToken);
            var timeInfo = JsonSerializer.Deserialize<TimeResponse>(response);

            return timeInfo?.DateTime;
        }
        catch
        {
            return null;
        }
        finally
        {
            SyncLock.Release();
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