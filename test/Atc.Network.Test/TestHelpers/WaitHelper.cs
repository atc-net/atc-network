namespace Atc.Network.Test.TestHelpers;

public static class WaitHelper
{
    public static async Task<bool> WaitUntil(
        Func<bool> condition,
        int timeoutInMs = 3000)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < timeoutInMs)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(20);
        }

        return condition();
    }
}