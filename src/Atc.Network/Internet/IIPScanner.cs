namespace Atc.Network.Internet;

public interface IIPScanner
{
    event EventHandler<IPScannerProgressReport>? ProgressReporting;

    IPScannerConfig Configuration { get; set; }

    Task<IPScanResults> Scan(
        IPAddress ipAddress,
        CancellationToken cancellationToken = default);

    Task<IPScanResults> ScanCidrRange(
        IPAddress ipAddress,
        byte cidrLength,
        CancellationToken cancellationToken = default);

    Task<IPScanResults> ScanRange(
        IPAddress startIpAddress,
        IPAddress endIpAddress,
        CancellationToken cancellationToken = default);
}