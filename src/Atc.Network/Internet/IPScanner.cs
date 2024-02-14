// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable LocalizableElement
namespace Atc.Network.Internet;

/// <summary>
/// Provides functionality for scanning IP addresses and ranges to determine open ports,
/// resolve hostnames, MAC addresses, and vendor information.
/// </summary>
[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK.")]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "OK.")]
[SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty", Justification = "OK.")]
[SuppressMessage("Minor Code Smell", "S2486:Generic exceptions should not be ignored", Justification = "OK.")]
public partial class IPScanner : IIPScanner, IDisposable
{
    private const int SyncLockTimeoutInMs = 30_000;

    private readonly SemaphoreSlim syncLock = new(1, 1);
    private readonly ConcurrentBag<IPScanResult> processedScanResults = new();
    private readonly IPScannerProgressReport progressReporting = new();
    private ArpEntity[]? arpEntities;
    private int tasksToProcessCount;
    private int tasksProcessedCount;

    /// <summary>
    /// Occurs when there is progress to report during a scan operation.
    /// </summary>
    public event EventHandler<IPScannerProgressReport>? ProgressReporting;

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScanner"/> class with various configuration options.
    /// </summary>
    /// <param name="logger">The logger to use for logging information and errors during scan operations.</param>
    public IPScanner(
        ILogger logger)
    {
        this.logger = logger;
        this.Configuration = new IPScannerConfig();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScanner"/> class with a specified logger and optional configuration settings.
    /// </summary>
    /// <param name="logger">The logger to use for logging information and errors during scan operations.</param>
    /// <param name="ipScannerConfig">Optional configuration settings for the scanner. If not provided, default settings are used.</param>
    public IPScanner(
        ILogger logger,
        IPScannerConfig? ipScannerConfig)
    {
        this.logger = logger;
        this.Configuration = ipScannerConfig ?? new IPScannerConfig();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScanner"/> class using a default logger instance.
    /// </summary>
    public IPScanner()
        : this(NullLogger.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScanner"/> class with optional configuration settings using a default logger instance.
    /// </summary>
    /// <param name="ipScannerConfig">Optional configuration settings for the scanner. If not provided, default settings are used.</param>
    public IPScanner(
        IPScannerConfig? ipScannerConfig)
        : this(NullLogger.Instance, ipScannerConfig)
    {
    }

    /// <summary>
    /// Gets or sets the configuration settings for the IP scanner.
    /// </summary>
    /// <value>The configuration settings used by the scanner.</value>
    /// <remarks>
    /// This property allows for the dynamic adjustment of the scanner's settings after initialization,
    /// providing flexibility to change scanning behavior at runtime.
    /// </remarks>
    public IPScannerConfig Configuration { get; set; }

    /// <summary>
    /// Initiates an asynchronous scan for the specified IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address to scan.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in the scan results.
    /// </returns>
    public Task<IPScanResults> Scan(
        IPAddress ipAddress,
        CancellationToken cancellationToken = default)
        => ScanRange(ipAddress, ipAddress, cancellationToken);

    /// <summary>
    /// Initiates an asynchronous scan for a range of IP addresses specified by a CIDR notation.
    /// </summary>
    /// <param name="ipAddress">The starting IP address of the CIDR range.</param>
    /// <param name="cidrLength">The CIDR length that specifies the range of IP addresses to scan.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in the scan results.
    /// </returns>
    public Task<IPScanResults> ScanCidrRange(
        IPAddress ipAddress,
        byte cidrLength,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (cidrLength > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(cidrLength), "CIDR length can maximum be 32");
        }

        var (startIpAddress, endIpAddress) = IPv4AddressHelper.GetFirstAndLastAddressInRange(
            ipAddress,
            cidrLength);

        return ScanRange(startIpAddress, endIpAddress, cancellationToken);
    }

    /// <summary>
    /// Scans a specified range of IP addresses for open ports and services.
    /// </summary>
    /// <param name="startIpAddress">The starting IP address of the range to scan.</param>
    /// <param name="endIpAddress">The ending IP address of the range to scan.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in a collection of scan results.
    /// </returns>
    public async Task<IPScanResults> ScanRange(
        IPAddress startIpAddress,
        IPAddress endIpAddress,
        CancellationToken cancellationToken = default)
    {
        var scanResults = new IPScanResults();

        var validationResult = IPv4AddressHelper.ValidateAddresses(startIpAddress, endIpAddress);
        if (!validationResult.IsValid)
        {
            scanResults.ErrorMessage = validationResult.ErrorMessage;
            scanResults.End = DateTime.Now;
            return await Task.FromResult(scanResults);
        }

        var ipAddresses = IPv4AddressHelper.GetAddressesInRange(startIpAddress, endIpAddress);
        if (ipAddresses.Count == 0)
        {
            scanResults.ErrorMessage = "Nothing to process";
            scanResults.End = DateTime.Now;
            return scanResults;
        }

        try
        {
            await syncLock.WaitAsync(SyncLockTimeoutInMs, cancellationToken);

            if (Configuration.ResolveMacAddress)
            {
                arpEntities = ArpHelper.GetArpResult();
            }

            tasksToProcessCount = ipAddresses.Count * Configuration.GetTasksToProcessCount();
            tasksProcessedCount = 0;
            processedScanResults.Clear();

            var tasks = ipAddresses.Select(
                async ipAddress =>
                {
                    await DoScan(ipAddress, cancellationToken);
                });

            var timeoutTask = Task.Delay((int)Configuration.Timeout.TotalMilliseconds, cancellationToken);

            await Task.WhenAny(
                TaskHelper.WhenAll(tasks),
                timeoutTask);

            foreach (var processedScanResult in processedScanResults)
            {
                scanResults.CollectedResults.Add(processedScanResult);
            }

            processedScanResults.Clear();

            scanResults.PercentageCompleted = MathHelper.Percentage(tasksToProcessCount, tasksProcessedCount);
            scanResults.End = DateTime.Now;
            return scanResults;
        }
        finally
        {
            syncLock.Release();
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
        syncLock.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="ProgressReporting"/> event with current progress information.
    /// </summary>
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
        RaiseProgressReporting(IPScannerProgressReportingType.IPAddressStart, ipScanResult);

        if (Configuration.IcmpPing)
        {
            await HandlePing(ipScanResult, ipAddress);
        }

        if (Configuration.ResolveHostName)
        {
            await HandleResolveHostName(ipScanResult, ipAddress, cancellationToken);
        }

        if (Configuration.ResolveMacAddress)
        {
            HandleResolveMacAddress(ipScanResult, ipAddress);
        }

        if (Configuration is { ResolveMacAddress: true, ResolveVendorFromMacAddress: true })
        {
            await HandleResolveVendorFromMacAddress(ipScanResult, cancellationToken);
        }

        if (Configuration.PortNumbers.Count != 0)
        {
            foreach (var portNumber in Configuration.PortNumbers)
            {
                await HandleTcpPort(ipScanResult, ipAddress, portNumber, cancellationToken);
            }

            if (Configuration.TreatOpenPortsAsWebServices != IPServicePortExaminationLevel.None)
            {
                await HandleTreatOpenPortsAsWebServices(ipScanResult, ipAddress, cancellationToken);
            }
        }

        ipScanResult.End = DateTime.Now;
        RaiseProgressReporting(IPScannerProgressReportingType.IPAddressDone, ipScanResult);
    }

    private async Task HandlePing(
        IPScanResult ipScanResult,
        IPAddress ipAddress)
    {
        ipScanResult.PingStatus = await PingHelper.GetStatus(ipAddress, Configuration.TimeoutPing);
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
        arpEntities ??= ArpHelper.GetArpResult();
        if (arpEntities.Length != 0)
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
        ushort portNumber,
        CancellationToken cancellationToken)
    {
        using var ipPortScan = new IPPortScan(ipAddress, (int)Configuration.TimeoutTcp.TotalMilliseconds);

        var canConnect = await ipPortScan.CanConnectWithTcp(
            portNumber,
            cancellationToken);

        var result = new IPScanPortResult
        {
            IPAddress = ipAddress,
            Port = portNumber,
            TransportProtocol = TransportProtocolType.Tcp,
            CanConnect = canConnect,
        };

        ipScanResult.Ports.Add(result);

        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.Tcp, ipScanResult);
    }

    private async Task HandleTreatOpenPortsAsWebServices(
        IPScanResult ipScanResult,
        IPAddress ipAddress,
        CancellationToken cancellationToken)
    {
        var handledCount = 0;
        foreach (var portNumber in ipScanResult.OpenPortNumbers)
        {
            if (!portNumber.IsPortForIPService(ServiceProtocolType.Http, Configuration.TreatOpenPortsAsWebServices) &&
                !portNumber.IsPortForIPService(ServiceProtocolType.Https, Configuration.TreatOpenPortsAsWebServices))
            {
                continue;
            }

            await HandleHttpPort(ipScanResult, ipAddress, portNumber, cancellationToken);
            RaiseProgressReporting(IPScannerProgressReportingType.ServiceHttp, ipScanResult);
            handledCount++;
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

        using var ipPortScan = new IPPortScan(ipAddress, (int)Configuration.TimeoutHttp.TotalMilliseconds);

        var canConnectHttp = await ipPortScan.CanConnectWithHttp(
            portNumber,
            cancellationToken);

        if (canConnectHttp)
        {
            workOnItem.ServiceProtocol = ServiceProtocolType.Http;
        }
        else
        {
            var canConnectHttps = await ipPortScan.CanConnectWithHttps(
                portNumber,
                cancellationToken);

            if (canConnectHttps)
            {
                workOnItem.ServiceProtocol = ServiceProtocolType.Https;
            }
        }

        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.ServiceHttp, ipScanResult);
    }
}