<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Internet

<br />

## IIPPortScan
Defines a contract for scanning IP ports to check for connectivity using TCP, HTTP, and HTTPS.

>```csharp
>public interface IIPPortScan
>```

### Methods

#### CanConnectWithHttp
>```csharp
>Task<bool> CanConnectWithHttp(int portNumber = 80, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Checks if an HTTP connection can be established on the specified port.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number to check, defaulting to 80.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in true if the connection was successful; otherwise, false.
#### CanConnectWithHttpOrHttps
>```csharp
>Task<bool> CanConnectWithHttpOrHttps(int portNumber = 80, bool useHttps = False, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Checks if a connection using either HTTP or HTTPS can be established on the specified port.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number to check, defaulting to 80.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`useHttps`&nbsp;&nbsp;-&nbsp;&nbsp;Indicates whether to use HTTPS (true) or HTTP (false).<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in true if the connection was successful; otherwise, false.
#### CanConnectWithHttps
>```csharp
>Task<bool> CanConnectWithHttps(int portNumber = 443, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Checks if an HTTPS connection can be established on the specified port.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number to check, defaulting to 443.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in true if the connection was successful; otherwise, false.
#### CanConnectWithTcp
>```csharp
>Task<bool> CanConnectWithTcp(int portNumber, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Checks if a TCP connection can be established on the specified port.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number to check.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in true if the connection was successful; otherwise, false.
#### SetIPAddress
>```csharp
>void SetIPAddress(IPAddress value)
>```
><b>Summary:</b> Sets the IP address to be used for scanning operations.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address to scan.<br />
#### SetTimeout
>```csharp
>void SetTimeout(TimeSpan value)
>```
><b>Summary:</b> Sets the timeout period for connection attempts.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value`&nbsp;&nbsp;-&nbsp;&nbsp;The maximum amount of time to wait for a connection attempt before timing out.<br />

<br />

## IIPScanner
Defines a contract for scanning IP addresses or ranges to assess network accessibility and gather information like open ports, hostnames, and more.

>```csharp
>public interface IIPScanner
>```

### Properties

#### Configuration
>```csharp
>Configuration
>```
><b>Summary:</b> Gets or sets the configuration settings for the IP scanner.
### Events

#### ProgressReporting
>```csharp
>ProgressReporting
>```
><b>Summary:</b> Occurs when there is progress to report during the scanning process.
### Methods

#### Scan
>```csharp
>Task<IPScanResults> Scan(IPAddress ipAddress, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Initiates an asynchronous scan for the specified IP address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address to scan.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token that can be used to request cancellation of the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in the scan results.
#### ScanCidrRange
>```csharp
>Task<IPScanResults> ScanCidrRange(IPAddress ipAddress, byte cidrLength, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Initiates an asynchronous scan for a range of IP addresses specified by a CIDR notation.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The starting IP address of the CIDR range.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cidrLength`&nbsp;&nbsp;-&nbsp;&nbsp;The CIDR length that specifies the range of IP addresses to scan.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token that can be used to request cancellation of the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in the scan results.
#### ScanRange
>```csharp
>Task<IPScanResults> ScanRange(IPAddress startIpAddress, IPAddress endIpAddress, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Initiates an asynchronous scan over a specified range of IP addresses.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The starting IP address of the range to scan.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`endIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The ending IP address of the range to scan.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token that can be used to request cancellation of the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in the scan results.

<br />

## IPPortScan
Provides functionality for scanning IP ports to check for TCP, HTTP, and HTTPS connectivity.

>```csharp
>public class IPPortScan : IIPPortScan, IDisposable
>```

### Methods

#### CanConnectWithHttp
>```csharp
>Task<bool> CanConnectWithHttp(int portNumber = 80, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Attempts to connect to a specified port using HTTP.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number to attempt connection, default is 80.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in a boolean indicating connection success.
#### CanConnectWithHttpOrHttps
>```csharp
>Task<bool> CanConnectWithHttpOrHttps(int portNumber = 80, bool useHttps = False, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Attempts to connect to a specified port using HTTP or HTTPS.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number to attempt connection, default is 80 for HTTP and 443 for HTTPS.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`useHttps`&nbsp;&nbsp;-&nbsp;&nbsp;Specifies whether to use HTTPS (true) or HTTP (false).<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in a boolean indicating connection success.
#### CanConnectWithHttps
>```csharp
>Task<bool> CanConnectWithHttps(int portNumber = 443, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Attempts to connect to a specified port using HTTPS.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number to attempt connection, default is 443.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in a boolean indicating connection success.
#### CanConnectWithTcp
>```csharp
>Task<bool> CanConnectWithTcp(int portNumber, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Attempts to connect to a specified port using TCP.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number to attempt connection.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in a boolean indicating connection success.
#### Dispose
>```csharp
>void Dispose()
>```
#### SetIPAddress
>```csharp
>void SetIPAddress(IPAddress value)
>```
><b>Summary:</b> Sets the IP address to scan.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address.<br />
#### SetTimeout
>```csharp
>void SetTimeout(TimeSpan value)
>```
><b>Summary:</b> Sets the timeout for connection attempts.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value`&nbsp;&nbsp;-&nbsp;&nbsp;The timeout as a .<br />

<br />

## IPScanner
Provides functionality for scanning IP addresses and ranges to determine open ports, resolve hostnames, MAC addresses, and vendor information.

>```csharp
>public class IPScanner : IIPScanner, IDisposable
>```

### Properties

#### Configuration
>```csharp
>Configuration
>```
><b>Summary:</b> Gets or sets the configuration settings for the IP scanner.
>
><b>Remarks:</b> This property allows for the dynamic adjustment of the scanner's settings after initialization, providing flexibility to change scanning behavior at runtime.
### Events

#### ProgressReporting
>```csharp
>ProgressReporting
>```
><b>Summary:</b> Occurs when there is progress to report during a scan operation.
### Methods

#### Dispose
>```csharp
>void Dispose()
>```
#### Scan
>```csharp
>Task<IPScanResults> Scan(IPAddress ipAddress, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Initiates an asynchronous scan for the specified IP address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address to scan.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token that can be used to request cancellation of the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in the scan results.
#### ScanCidrRange
>```csharp
>Task<IPScanResults> ScanCidrRange(IPAddress ipAddress, byte cidrLength, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Initiates an asynchronous scan for a range of IP addresses specified by a CIDR notation.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The starting IP address of the CIDR range.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cidrLength`&nbsp;&nbsp;-&nbsp;&nbsp;The CIDR length that specifies the range of IP addresses to scan.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token that can be used to request cancellation of the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in the scan results.
#### ScanRange
>```csharp
>Task<IPScanResults> ScanRange(IPAddress startIpAddress, IPAddress endIpAddress, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Scans a specified range of IP addresses for open ports and services.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The starting IP address of the range to scan.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`endIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The ending IP address of the range to scan.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to cancel the operation.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in a collection of scan results.

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
Represents a progress report for an ongoing IP scanning operation.

>```csharp
>public class IPScannerProgressReport
>```

### Properties

#### LatestUpdate
>```csharp
>LatestUpdate
>```
><b>Summary:</b> Gets or sets the latest update or result from the scanning operation.
#### PercentageCompleted
>```csharp
>PercentageCompleted
>```
><b>Summary:</b> Calculates and returns the percentage of tasks completed.
#### TasksProcessedCount
>```csharp
>TasksProcessedCount
>```
><b>Summary:</b> Gets or sets the number of tasks that have been processed so far.
#### TasksToProcessCount
>```csharp
>TasksToProcessCount
>```
><b>Summary:</b> Gets or sets the total number of tasks to process during the scan.
#### Type
>```csharp
>Type
>```
><b>Summary:</b> Gets or sets the type of the progress reporting event.
### Methods

#### ToString
>```csharp
>string ToString()
>```
><b>Summary:</b> Provides a string representation of the current progress report.
>
><b>Returns:</b> A string that represents the current state of the progress report, including details about the latest update, the type of event, and the current progress.
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
