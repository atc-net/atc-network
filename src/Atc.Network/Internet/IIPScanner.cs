namespace Atc.Network.Internet;

/// <summary>
/// Defines a contract for scanning IP addresses or ranges to assess network
/// accessibility and gather information like open ports, hostnames, and more.
/// </summary>
public interface IIPScanner
{
    /// <summary>
    /// Occurs when there is progress to report during the scanning process.
    /// </summary>
    event EventHandler<IPScannerProgressReport>? ProgressReporting;

    /// <summary>
    /// Gets or sets the configuration settings for the IP scanner.
    /// </summary>
    /// <value>The configuration settings used by the scanner.</value>
    IPScannerConfig Configuration { get; set; }

    /// <summary>
    /// Initiates an asynchronous scan for the specified IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address to scan.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in the scan results.
    /// </returns>
    Task<IPScanResults> Scan(
        IPAddress ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates an asynchronous scan for a range of IP addresses specified by a CIDR notation.
    /// </summary>
    /// <param name="ipAddress">The starting IP address of the CIDR range.</param>
    /// <param name="cidrLength">The CIDR length that specifies the range of IP addresses to scan.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in the scan results.
    /// </returns>
    Task<IPScanResults> ScanCidrRange(
        IPAddress ipAddress,
        byte cidrLength,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates an asynchronous scan over a specified range of IP addresses.
    /// </summary>
    /// <param name="startIpAddress">The starting IP address of the range to scan.</param>
    /// <param name="endIpAddress">The ending IP address of the range to scan.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in the scan results.
    /// </returns>
    Task<IPScanResults> ScanRange(
        IPAddress startIpAddress,
        IPAddress endIpAddress,
        CancellationToken cancellationToken = default);
}