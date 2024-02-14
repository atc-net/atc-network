namespace Atc.Network.Internet;

/// <summary>
/// Defines a contract for scanning IP ports to check for connectivity using TCP, HTTP, and HTTPS.
/// </summary>
public interface IIPPortScan
{
    /// <summary>
    /// Sets the IP address to be used for scanning operations.
    /// </summary>
    /// <param name="value">The IP address to scan.</param>
    void SetIPAddress(
        IPAddress value);

    /// <summary>
    /// Sets the timeout period for connection attempts.
    /// </summary>
    /// <param name="value">The maximum amount of time to wait for a connection attempt before timing out.</param>
    void SetTimeout(
        TimeSpan value);

    /// <summary>
    /// Checks if a TCP connection can be established on the specified port.
    /// </summary>
    /// <param name="portNumber">The port number to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in true if the connection was successful; otherwise, false.
    /// </returns>
    Task<bool> CanConnectWithTcp(
        int portNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an HTTP connection can be established on the specified port.
    /// </summary>
    /// <param name="portNumber">The port number to check, defaulting to 80.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in true if the connection was successful; otherwise, false.
    /// </returns>
    Task<bool> CanConnectWithHttp(
        int portNumber = 80,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an HTTPS connection can be established on the specified port.
    /// </summary>
    /// <param name="portNumber">The port number to check, defaulting to 443.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in true if the connection was successful; otherwise, false.
    /// </returns>
    Task<bool> CanConnectWithHttps(
        int portNumber = 443,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a connection using either HTTP or HTTPS can be established on the specified port.
    /// </summary>
    /// <param name="portNumber">The port number to check, defaulting to 80.</param>
    /// <param name="useHttps">Indicates whether to use HTTPS (true) or HTTP (false).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, resulting in true if the connection was successful; otherwise, false.
    /// </returns>
    Task<bool> CanConnectWithHttpOrHttps(
        int portNumber = 80,
        bool useHttps = false,
        CancellationToken cancellationToken = default);
}