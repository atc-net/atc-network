namespace Atc.Network.Internet;

public interface IIPPortScan
{
    Task<bool> CanConnectWithTcp(
        int portNumber,
        CancellationToken cancellationToken = default);

    Task<bool> CanConnectWithHttp(
        int portNumber = 80,
        CancellationToken cancellationToken = default);

    Task<bool> CanConnectWithHttps(
        int portNumber = 443,
        CancellationToken cancellationToken = default);

    Task<bool> CanConnectWithHttpOrHttps(
        int portNumber = 80,
        bool useHttps = false,
        CancellationToken cancellationToken = default);
}