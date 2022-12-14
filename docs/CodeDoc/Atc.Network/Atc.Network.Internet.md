<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Internet

<br />

## IIPPortScan

>```csharp
>public interface IIPPortScan
>```

### Methods

#### CanConnectWithHttp
>```csharp
>Task<bool> CanConnectWithHttp(int portNumber = 80, CancellationToken cancellationToken = null)
>```
#### CanConnectWithHttpOrHttps
>```csharp
>Task<bool> CanConnectWithHttpOrHttps(int portNumber = 80, bool useHttps = False, CancellationToken cancellationToken = null)
>```
#### CanConnectWithHttps
>```csharp
>Task<bool> CanConnectWithHttps(int portNumber = 443, CancellationToken cancellationToken = null)
>```
#### CanConnectWithTcp
>```csharp
>Task<bool> CanConnectWithTcp(int portNumber, CancellationToken cancellationToken = null)
>```
#### SetIPAddress
>```csharp
>void SetIPAddress(IPAddress value)
>```
#### SetTimeout
>```csharp
>void SetTimeout(TimeSpan value)
>```

<br />

## IIPScanner

>```csharp
>public interface IIPScanner
>```

### Properties

#### Configuration
>```csharp
>Configuration
>```
### Events

#### ProgressReporting
>```csharp
>ProgressReporting
>```
### Methods

#### Scan
>```csharp
>Task<IPScanResults> Scan(IPAddress ipAddress, CancellationToken cancellationToken = null)
>```
#### ScanCidrRange
>```csharp
>Task<IPScanResults> ScanCidrRange(IPAddress ipAddress, byte cidrLength, CancellationToken cancellationToken = null)
>```
#### ScanRange
>```csharp
>Task<IPScanResults> ScanRange(IPAddress startIpAddress, IPAddress endIpAddress, CancellationToken cancellationToken = null)
>```

<br />

## IPPortScan

>```csharp
>public class IPPortScan : IIPPortScan
>```

### Methods

#### CanConnectWithHttp
>```csharp
>Task<bool> CanConnectWithHttp(int portNumber = 80, CancellationToken cancellationToken = null)
>```
#### CanConnectWithHttpOrHttps
>```csharp
>Task<bool> CanConnectWithHttpOrHttps(int portNumber = 80, bool useHttps = False, CancellationToken cancellationToken = null)
>```
#### CanConnectWithHttps
>```csharp
>Task<bool> CanConnectWithHttps(int portNumber = 443, CancellationToken cancellationToken = null)
>```
#### CanConnectWithTcp
>```csharp
>Task<bool> CanConnectWithTcp(int portNumber, CancellationToken cancellationToken = null)
>```
#### SetIPAddress
>```csharp
>void SetIPAddress(IPAddress value)
>```
#### SetTimeout
>```csharp
>void SetTimeout(TimeSpan value)
>```

<br />

## IPScanner
IPScanner LoggerMessages.

>```csharp
>public class IPScanner : IIPScanner, IDisposable
>```

### Properties

#### Configuration
>```csharp
>Configuration
>```
### Events

#### ProgressReporting
>```csharp
>ProgressReporting
>```
### Methods

#### Dispose
>```csharp
>void Dispose()
>```
#### Scan
>```csharp
>Task<IPScanResults> Scan(IPAddress ipAddress, CancellationToken cancellationToken = null)
>```
#### ScanCidrRange
>```csharp
>Task<IPScanResults> ScanCidrRange(IPAddress ipAddress, byte cidrLength, CancellationToken cancellationToken = null)
>```
#### ScanRange
>```csharp
>Task<IPScanResults> ScanRange(IPAddress startIpAddress, IPAddress endIpAddress, CancellationToken cancellationToken = null)
>```

<br />

## IPScannerConfig

>```csharp
>public class IPScannerConfig
>```

### Properties

#### IcmpPing
>```csharp
>IcmpPing
>```
#### PortNumbers
>```csharp
>PortNumbers
>```
#### ResolveHostName
>```csharp
>ResolveHostName
>```
#### ResolveMacAddress
>```csharp
>ResolveMacAddress
>```
#### ResolveVendorFromMacAddress
>```csharp
>ResolveVendorFromMacAddress
>```
#### Timeout
>```csharp
>Timeout
>```
#### TimeoutHttp
>```csharp
>TimeoutHttp
>```
#### TimeoutPing
>```csharp
>TimeoutPing
>```
#### TimeoutTcp
>```csharp
>TimeoutTcp
>```
#### TreatOpenPortsAsWebServices
>```csharp
>TreatOpenPortsAsWebServices
>```
### Methods

#### SetPortNumbers
>```csharp
>void SetPortNumbers(IPServicePortExaminationLevel ipServicePortExaminationLevel)
>```
#### SetPortNumbers
>```csharp
>void SetPortNumbers(IPServicePortExaminationLevel ipServicePortExaminationLevel, ServiceProtocolType serviceProtocolType)
>```
#### SetPortNumbers
>```csharp
>void SetPortNumbers(IPServicePortExaminationLevel ipServicePortExaminationLevel, ServiceProtocolType[] serviceProtocolTypes)
>```
#### ToString
>```csharp
>string ToString()
>```

<br />

## IPScannerConstants

>```csharp
>public static class IPScannerConstants
>```

### Static Fields

#### TimeoutHttpInMs
>```csharp
>int TimeoutHttpInMs
>```
><b>Summary:</b> The connect time-out for http/https value, in milliseconds. The default is 500 (500 msec);
#### TimeoutInMs
>```csharp
>int TimeoutInMs
>```
><b>Summary:</b> The connect time-out value, in milliseconds. The default is 180000 (3 min);
#### TimeoutPingInMs
>```csharp
>int TimeoutPingInMs
>```
><b>Summary:</b> The connect time-out for ping (ICMP) value, in milliseconds. The default is 4000 (4 sec);
>
><b>Remarks:</b> See: https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/ping With parameter "/w".
#### TimeoutTcpInMs
>```csharp
>int TimeoutTcpInMs
>```
><b>Summary:</b> The connect time-out for tpc value, in milliseconds. The default is 100 (100 msec);

<br />

## IPScannerProgressReport

>```csharp
>public class IPScannerProgressReport
>```

### Properties

#### LatestUpdate
>```csharp
>LatestUpdate
>```
#### PercentageCompleted
>```csharp
>PercentageCompleted
>```
#### TasksProcessedCount
>```csharp
>TasksProcessedCount
>```
#### TasksToProcessCount
>```csharp
>TasksToProcessCount
>```
#### Type
>```csharp
>Type
>```
### Methods

#### ToString
>```csharp
>string ToString()
>```
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
