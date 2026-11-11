// ReSharper disable DuplicatedStatements
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
    private readonly IMacAddressVendorLookup macAddressVendorLookup = new OnlineMacAddressVendorLookup();
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
    public IPScanner(ILogger logger)
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
    /// <summary>
    /// Initializes a new instance of the <see cref="IPScanner"/> class that resolves MAC address vendors with the given lookup,
    /// for example <see cref="IeeeOuiMacAddressVendorLookup"/> to look up offline.
    /// </summary>
    /// <param name="logger">The logger to use for logging information and errors during scan operations.</param>
    /// <param name="ipScannerConfig">Optional configuration settings for the scanner. If not provided, default settings are used.</param>
    /// <param name="macAddressVendorLookup">The MAC address vendor lookup, used when <see cref="IPScannerConfig.ResolveVendorFromMacAddress"/> is on.</param>
    public IPScanner(
        ILogger logger,
        IPScannerConfig? ipScannerConfig,
        IMacAddressVendorLookup macAddressVendorLookup)
        : this(logger, ipScannerConfig)
    {
        ArgumentNullException.ThrowIfNull(macAddressVendorLookup);
        this.macAddressVendorLookup = macAddressVendorLookup;
    }

    public IPScanner()
        : this(NullLogger.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScanner"/> class with optional configuration settings using a default logger instance.
    /// </summary>
    /// <param name="ipScannerConfig">Optional configuration settings for the scanner. If not provided, default settings are used.</param>
    public IPScanner(IPScannerConfig? ipScannerConfig)
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

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            throw new ArgumentException("IPv6 CIDR ranges can't be scanned; scan single IPv6 addresses instead.", nameof(ipAddress));
        }

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
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public async Task<IPScanResults> ScanRange(
        IPAddress startIpAddress,
        IPAddress endIpAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startIpAddress);
        ArgumentNullException.ThrowIfNull(endIpAddress);

        var scanResults = new IPScanResults();

        var (ipAddresses, errorMessage) = GetAddressesToScan(startIpAddress, endIpAddress);
        if (errorMessage is not null)
        {
            scanResults.ErrorMessage = errorMessage;
            scanResults.End = DateTime.Now;
            return scanResults;
        }

        if (ipAddresses.Count == 0)
        {
            scanResults.ErrorMessage = "Nothing to process";
            scanResults.End = DateTime.Now;
            return scanResults;
        }

        var lockAcquired = false;

        try
        {
            lockAcquired = await syncLock.WaitAsync(SyncLockTimeoutInMs, cancellationToken);

            if (!lockAcquired)
            {
                scanResults.ErrorMessage = "Another scan is already in progress";
                scanResults.End = DateTime.Now;
                return scanResults;
            }

            if (Configuration.ResolveMacAddress)
            {
                arpEntities = ArpHelper.GetArpResult();
            }

            tasksToProcessCount = ipAddresses.Count * Configuration.GetTasksToProcessCount();
            tasksProcessedCount = 0;
            processedScanResults.Clear();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                CancellationToken = cancellationToken,
            };

            try
            {
                await Parallel.ForEachAsync(
                    ipAddresses,
                    parallelOptions,
                    async (ipAddress, ct) =>
                    {
                        await DoScan(ipAddress, ct);
                    });
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelled
            }

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
            if (lockAcquired)
            {
                syncLock.Release();
            }
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
    protected virtual void Dispose(bool disposing)
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

    /// <summary>
    /// Returns the addresses to scan. An IPv6 address can be scanned on its own, but not as a range:
    /// even the smallest usual IPv6 subnet (/64) has 2^64 addresses.
    /// </summary>
    private static (IReadOnlyCollection<IPAddress> IPAddresses, string? ErrorMessage) GetAddressesToScan(
        IPAddress startIpAddress,
        IPAddress endIpAddress)
    {
        if (startIpAddress.AddressFamily == AddressFamily.InterNetworkV6 ||
            endIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return startIpAddress.Equals(endIpAddress)
                ? ([startIpAddress], null)
                : ([], "IPv6 ranges can't be scanned; scan single IPv6 addresses instead.");
        }

        var validationResult = IPv4AddressHelper.ValidateAddresses(startIpAddress, endIpAddress);
        return validationResult.IsValid
            ? (IPv4AddressHelper.GetAddressesInRange(startIpAddress, endIpAddress), null)
            : ([], validationResult.ErrorMessage);
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
            // Scan the ports concurrently: closed or filtered ports each wait for the TCP timeout,
            // so one after another they cost PortNumbers.Count x TimeoutTcp per IP address.
            var portScanOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = IPScannerConstants.MaxConcurrentPortScansPerIPAddress,
                CancellationToken = cancellationToken,
            };

            await Parallel.ForEachAsync(
                Configuration.PortNumbers,
                portScanOptions,
                async (portNumber, token) => await HandleTcpPort(ipScanResult, ipAddress, portNumber, token));

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
        // First try ARP lookup for all addresses as it's most reliable
        if (arpEntities is not null && arpEntities.Length != 0)
        {
            var arpEntity = arpEntities.FirstOrDefault(x => x.IPAddress.Equals(ipAddress));
            if (arpEntity is not null)
            {
                ipScanResult.MacAddress = arpEntity.MacAddress;
                Interlocked.Increment(ref tasksProcessedCount);
                RaiseProgressReporting(IPScannerProgressReportingType.MacAddress, ipScanResult);
                return;
            }
        }

        // If ARP lookup failed, check special cases

        // Check if this is a loopback address (127.x.x.x)
        if (ArpHelper.IsLoopbackAddress(ipAddress))
        {
            // Handle loopback address separately since it won't appear in ARP table
            var loopbackEntity = ArpHelper.GetLoopbackArpEntity(ipAddress);
            ipScanResult.MacAddress = loopbackEntity.MacAddress;
            Interlocked.Increment(ref tasksProcessedCount);
            RaiseProgressReporting(IPScannerProgressReportingType.MacAddress, ipScanResult);
            return;
        }

        // Check if this is the local machine's IP address
        if (ArpHelper.IsLocalMachineAddress(ipAddress))
        {
            // Handle local machine IP separately as it might not appear in ARP table
            var localEntity = ArpHelper.GetLocalMachineArpEntity(ipAddress);
            ipScanResult.MacAddress = localEntity.MacAddress;
            Interlocked.Increment(ref tasksProcessedCount);
            RaiseProgressReporting(IPScannerProgressReportingType.MacAddress, ipScanResult);
            return;
        }

        Interlocked.Increment(ref tasksProcessedCount);
        RaiseProgressReporting(IPScannerProgressReportingType.MacAddress, ipScanResult);
    }

    /// <summary>
    /// Returns the vendor name for a scanned address: a fixed name for the loopback and the local machine,
    /// otherwise the name from the MAC address vendor lookup.
    /// </summary>
    /// <param name="ipAddress">The scanned IP address.</param>
    /// <param name="macAddress">Its MAC address.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The vendor name, or <see langword="null"/> when it is unknown.</returns>
    internal Task<string?> ResolveMacVendor(
        IPAddress ipAddress,
        string macAddress,
        CancellationToken cancellationToken)
    {
        if (macAddress == ArpHelper.LoopbackMacAddress)
        {
            return Task.FromResult<string?>("Loopback Interface");
        }

        if (ArpHelper.IsLocalMachineAddress(ipAddress))
        {
            return Task.FromResult<string?>("Local Machine");
        }

        return macAddressVendorLookup.LookupVendorName(macAddress, cancellationToken);
    }

    private async Task HandleResolveVendorFromMacAddress(
        IPScanResult ipScanResult,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(ipScanResult.MacAddress))
        {
            var vendorName = await ResolveMacVendor(ipScanResult.IPAddress, ipScanResult.MacAddress, cancellationToken);
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