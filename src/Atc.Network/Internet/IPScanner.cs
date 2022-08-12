// ReSharper disable EmptyGeneralCatchClause
namespace Atc.Network.Internet;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "OK.")]
[SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty", Justification = "OK.")]
[SuppressMessage("Minor Code Smell", "S2486:Generic exceptions should not be ignored", Justification = "OK.")]
public partial class IPScanner : IDisposable
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private readonly IPScannerConfig scannerConfig;
    private readonly ConcurrentBag<IPScanResult> processedScanResults = new();
    private readonly IPScannerProgressReport progressReporting = new();
    private int tasksToProcessCount;
    private int tasksProcessedCount;

    public event EventHandler<IPScannerProgressReport>? ProgressReporting;

    public IPScanner(
        ILogger logger,
        IPScannerConfig? ipScannerConfig)
    {
        this.logger = logger;
        this.scannerConfig = ipScannerConfig ?? new IPScannerConfig();
    }

    public IPScanner(
        IPScannerConfig? ipScannerConfig)
        : this(NullLogger.Instance, ipScannerConfig)
    {
    }

    public Task<IPScanResults> Scan(
        IPAddress ipAddress,
        CancellationToken cancellationToken = default)
        => ScanRange(ipAddress, ipAddress, cancellationToken);

    public Task<IPScanResults> ScanCidrRange(
        IPAddress ipAddress,
        int cidrMaskLength,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (cidrMaskLength is < 0 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(cidrMaskLength));
        }

        var (startIpAddress, endIpAddress) = IPAddressV4Helper.GetStartAndEndAddressesInRange(
            ipAddress,
            cidrMaskLength);

        return ScanRange(startIpAddress, endIpAddress, cancellationToken);
    }

    public async Task<IPScanResults> ScanRange(
        IPAddress startIpAddress,
        IPAddress endIpAddress,
        CancellationToken cancellationToken = default)
    {
        var scanResults = new IPScanResults();

        var validationResult = IPAddressV4Helper.ValidateAddresses(startIpAddress, endIpAddress);
        if (!validationResult.IsValid)
        {
            scanResults.ErrorMessage = validationResult.ErrorMessage;
            scanResults.End = DateTime.Now;
            return await Task.FromResult(scanResults);
        }

        var ipAddresses = IPAddressV4Helper.GetAddressesInRange(startIpAddress, endIpAddress);
        if (!ipAddresses.Any())
        {
            scanResults.ErrorMessage = "Nothing to process";
            scanResults.End = DateTime.Now;
            return scanResults;
        }

        try
        {
            await SyncLock.WaitAsync(cancellationToken);

            tasksToProcessCount = ipAddresses.Length * scannerConfig.GetTasksToProcessCount();
            tasksProcessedCount = 0;
            processedScanResults.Clear();

            var tasks = ipAddresses.Select(
                async ipAddress =>
                {
                    await DoScan(ipAddress, cancellationToken);
                });

            var timeoutTask = Task.Delay((int)scannerConfig.Timeout.TotalMilliseconds, cancellationToken);

            await Task.WhenAny(
                TaskHelper.WhenAll(tasks),
                timeoutTask);

            foreach (var processedScanResult in processedScanResults)
            {
                scanResults.CollectedResults.Add(processedScanResult);
            }

            processedScanResults.Clear();

            scanResults.End = DateTime.Now;
            return scanResults;
        }
        finally
        {
            SyncLock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose.
    /// </summary>
    /// <param name="disposing">Indicates if we are disposing or not.</param>
    protected virtual void Dispose(
        bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        processedScanResults.Clear();
    }

    private void RaiseProgressReporting(
        IPScannerProgressReportingType reportingType,
        IPScanResult? ipScanResult)
    {
        progressReporting.Type = reportingType;
        progressReporting.TasksToProcessCount = tasksToProcessCount;
        progressReporting.TasksProcessedCount = tasksProcessedCount;
        progressReporting.LatestUpdate = ipScanResult;
        ProgressReporting?.Invoke(this, progressReporting);
    }

    private async Task DoScan(
        IPAddress ipAddress,
        CancellationToken cancellationToken)
    {
        var ipScanResult = new IPScanResult(ipAddress);
        processedScanResults.Add(ipScanResult);
        RaiseProgressReporting(IPScannerProgressReportingType.IpAddressStart, ipScanResult);

        if (scannerConfig.Ping)
        {
            await HandlePing(ipScanResult, ipAddress);
        }

        if (scannerConfig.ResolveHostName)
        {
            await HandleResolveHostName(ipScanResult, ipAddress, cancellationToken);
        }

        if (scannerConfig.ResolveMacAddress)
        {
            HandleResolveMacAddress(ipScanResult, ipAddress);
        }

        if (scannerConfig.ResolveMacAddress &&
            scannerConfig.ResolveVendorFromMacAddress)
        {
            await HandleResolveVendorFromMacAddress(ipScanResult, cancellationToken);
        }

        if (scannerConfig.PortNumbers.Any())
        {
            foreach (var portNumber in scannerConfig.PortNumbers)
            {
                await HandleTcpPort(ipScanResult, ipAddress, portNumber, cancellationToken);
            }

            if (scannerConfig.ResolveIPProtocolHttp)
            {
                await HandleHttpPort(ipScanResult, ipAddress, cancellationToken);
            }
        }

        ipScanResult.End = DateTime.Now;
        RaiseProgressReporting(IPScannerProgressReportingType.IpAddressDone, ipScanResult);
    }

    private async Task HandlePing(
        IPScanResult ipScanResult,
        IPAddress ipAddress)
    {
        ipScanResult.PingStatus = await PingHelper.GetStatus(ipAddress, (int)scannerConfig.TimeoutPing.TotalMilliseconds);
        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.Ping, ipScanResult);
    }

    private async Task HandleResolveHostName(
        IPScanResult ipScanResult,
        IPAddress ipAddress,
        CancellationToken cancellationToken)
    {
        ipScanResult.Hostname = await DnsLookupHelper.GetHostname(ipAddress, cancellationToken);
        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.HostName, ipScanResult);
    }

    private void HandleResolveMacAddress(
        IPScanResult ipScanResult,
        IPAddress ipAddress)
    {
        var arpEntities = ArpHelper.GetArpResult();
        if (arpEntities.Any())
        {
            var arpEntity = arpEntities.FirstOrDefault(x => x.IPAddress.Equals(ipAddress));
            if (arpEntity is not null)
            {
                ipScanResult.MacAddress = arpEntity.MacAddress;
            }
        }

        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.MacAddress, ipScanResult);
    }

    private async Task HandleResolveVendorFromMacAddress(
        IPScanResult ipScanResult,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(ipScanResult.MacAddress))
        {
            var vendorName = await MacAddressVendorLookupHelper.LookupVendorNameFromMacAddress(
                ipScanResult.MacAddress,
                cancellationToken);

            if (!string.IsNullOrEmpty(vendorName))
            {
                ipScanResult.MacVendor = vendorName;
            }
        }

        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.MacVendor, ipScanResult);
    }

    private async Task HandleTcpPort(
        IPScanResult ipScanResult,
        IPAddress ipAddress,
        int portNumber,
        CancellationToken cancellationToken)
    {
        var ipPortScan = new IPPortScan(ipAddress, (int)scannerConfig.TimeoutTcp.TotalMilliseconds);
        var canConnect = await ipPortScan.CanConnectWithTcp(
            portNumber,
            cancellationToken);

        var result = new IPScanPortResult
        {
            IPAddress = ipAddress,
            Port = portNumber,
            Protocol = IPProtocolType.Tcp,
            CanConnect = canConnect,
        };

        ipScanResult.Ports.Add(result);

        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.Tcp, ipScanResult);
    }

    private async Task HandleHttpPort(
        IPScanResult ipScanResult,
        IPAddress ipAddress,
        CancellationToken cancellationToken)
    {
        var handledCount = 0;
        foreach (var portNumber in ipScanResult.OpenPort)
        {
            if (scannerConfig.LimitResolveIPProtocolsToKnowIPPorts &&
                !KnowIPPortsLookupHelper.IsKnow(IPProtocolType.Http, portNumber) &&
                !KnowIPPortsLookupHelper.IsKnow(IPProtocolType.Http, portNumber))
            {
                continue;
            }

            await HandleHttpPort(ipScanResult, ipAddress, portNumber, cancellationToken);
            RaiseProgressReporting(IPScannerProgressReportingType.ServiceHttp, ipScanResult);
            handledCount++;
            if (!ipScanResult.Ports.Any(x => x.Protocol is IPProtocolType.Http or IPProtocolType.Https))
            {
                continue;
            }

            break;
        }

        var sendLastReport = false;
        for (var i = 0; i < ipScanResult.Ports.Count - handledCount; i++)
        {
            Interlocked.Increment(ref tasksProcessedCount);
            sendLastReport = true;
        }

        if (sendLastReport)
        {
            RaiseProgressReporting(IPScannerProgressReportingType.Counters, ipScanResult);
        }
    }

    private async Task HandleHttpPort(
        IPScanResult ipScanResult,
        IPAddress ipAddress,
        int portNumber,
        CancellationToken cancellationToken)
    {
        var workOnItem = ipScanResult.Ports.FirstOrDefault(x => x.Port == portNumber &&
                                                                x.IPAddress != null &&
                                                                x.IPAddress.Equals(ipAddress));

        if (workOnItem is null)
        {
            Interlocked.Increment(ref tasksProcessedCount);
            RaiseProgressReporting(IPScannerProgressReportingType.Counters, ipScanResult);
            return;
        }

        var ipPortScan = new IPPortScan(ipAddress, (int)scannerConfig.TimeoutHttp.TotalMilliseconds);
        var canConnectHttp = await ipPortScan.CanConnectWithHttp(
            portNumber,
            cancellationToken);

        if (canConnectHttp)
        {
            workOnItem.Protocol = IPProtocolType.Http;
        }
        else
        {
            var canConnectHttps = await ipPortScan.CanConnectWithHttps(
                portNumber,
                cancellationToken);

            if (canConnectHttps)
            {
                workOnItem.Protocol = IPProtocolType.Https;
            }
        }

        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.ServiceHttp, ipScanResult);
    }
}