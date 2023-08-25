[![NuGet Version](https://img.shields.io/nuget/v/atc.network.svg?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/atc.network)

# Atc.Network

Atc.Network is a C# library providing robust and flexible tools for network communication and scanning.

- **TcpClient/TcpServer:** Establish and manage TCP network connections.

- **UdpClient/UdpServer:** Establish and manage UDP network connections.

- - **IPScanner:** A flexible tool to scan a range of IP addresses or a single IP address. It comes with various configuration options such as:
  - ICMP Pinging
  - Host Name Resolution
  - MAC Address Resolution
  - Vendor Identification from MAC Address
  - Port Number Testing (None, Well-Known, Well-Known and Common, All)

## Using the TcpClient

A sample reference implementation can be found [`here`](sample/Atc.Network.Console.Tcp/Program.cs)

## Using the UdpClient and UdpServer

A sample reference implementation can be found [`here`](sample/Atc.Network.Console.Udp/Program.cs)

## Using the IPScanner

The IPScanner can scan a range of IPAddresses or just a single IPAddress as specified in the IPScannerConfig.

- If `IcmpPing` is enabled the result for given IPAddress will contain a PingResult with network quality information.
- If `ResolveHostName` is enabled the result for given IPAddress will contain the hostname if possible to resolve.
- If `ResolveMacAddress` is enabled the result for given IPAddress will contain the mac-address if possible to resolve.
- If `ResolveVendorFromMacAddress` is enabled the result for given IPAddress will contain the vendor name from the mac-address if possible to resolve.
- `TreatOpenPortsAsWebServices` defines what kind of port numbers should be tested, the options are: `None`, `WellKnown`, `WellKnownAndCommon`, `All`

### Example on ScanRange based on WellKnown port numbers

```csharp
    var ipScannerConfig = new IPScannerConfig
    {
        IcmpPing = true,
        ResolveHostName = true,
        ResolveMacAddress = true,
        ResolveVendorFromMacAddress = true,
        TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.WellKnown,
    };

    var ipScanner = new IPScanner(ipScannerConfig);
    ipScanner.ProgressReporting += IpScannerOnProgressReporting;

    var ipScanResults = await ipScanner.ScanRange(
        IPAddress.Parse("192.168.0.1"),
        IPAddress.Parse("192.168.0.254"),
        CancellationToken.None);
```

### Example on ScanRange based on specified port numbers

```csharp
    var ipScannerConfig = new IPScannerConfig
    {
        IcmpPing = true,
        ResolveHostName = true,
        ResolveMacAddress = true,
        ResolveVendorFromMacAddress = true,
        TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
    };

    ipScannerConfig.PortNumbers = new List<ushort> { 21, 80, 8080 };

    var ipScanner = new IPScanner(ipScannerConfig);
    ipScanner.ProgressReporting += IpScannerOnProgressReporting;

    var ipScanResults = await ipScanner.ScanRange(
        IPAddress.Parse("192.168.0.1"),
        IPAddress.Parse("192.168.0.254"),
        CancellationToken.None);
```

## Using the IPPortScan

### Example on CanConnectWithTcp

```csharp
    var ipPortScan = new IPPortScan(IPAddress.Parse("192.168.0.27"));
    var ipPortScanResult = await ipPortScan.CanConnectWithTcp(
        80,
        CancellationToken.None);
```

### Example on CanConnectWithHttp

```csharp
    var ipPortScan = new IPPortScan(IPAddress.Parse("192.168.0.27"));
    var ipPortScanResult = await ipPortScan.CanConnectWithHttp(
        80,
        CancellationToken.None);
```

## How to contribute

[Contribution Guidelines](https://atc-net.github.io/introduction/about-atc#how-to-contribute)

[Coding Guidelines](https://atc-net.github.io/introduction/about-atc#coding-guidelines)