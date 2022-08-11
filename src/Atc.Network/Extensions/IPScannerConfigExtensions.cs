// ReSharper disable once CheckNamespace
namespace Atc.Network;

public static class IPScannerConfigExtensions
{
    public static int GetTasksToProcessCount(
        this IPScannerConfig ipScannerConfig)
    {
        ArgumentNullException.ThrowIfNull(ipScannerConfig);

        var count = 0;
        if (ipScannerConfig.Ping)
        {
            count++;
        }

        if (ipScannerConfig.ResolveHostName)
        {
            count++;
        }

        if (ipScannerConfig.ResolveMacAddress)
        {
            count++;
        }

        if (ipScannerConfig.ResolveMacAddress &&
            ipScannerConfig.ResolveVendorFromMacAddress)
        {
            count++;
        }

        count += ipScannerConfig.PortNumbers.Count;

        if (ipScannerConfig.ResolveIPProtocolHttp)
        {
            count += ipScannerConfig.PortNumbers.Count;
        }

        return count;
    }
}