<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Helpers

<br />

## ArpHelper
Provides utilities for fetching and parsing ARP (Address Resolution Protocol) table results.

>```csharp
>public static class ArpHelper
>```

### Static Fields

#### LoopbackMacAddress
>```csharp
>string LoopbackMacAddress
>```
><b>Summary:</b> Well-known MAC address for loopback interface.
#### LoopbackType
>```csharp
>string LoopbackType
>```
><b>Summary:</b> Type used for loopback interface entries.
### Static Methods

#### GetArpResult
>```csharp
>ArpEntity[] GetArpResult()
>```
><b>Summary:</b> Retrieves the ARP table results, caching them for 90 seconds to limit frequent lookups.
>
><b>Returns:</b> An array of `Atc.Network.Models.ArpEntity` representing the current ARP table entries. Returns an empty array if no connection is available or if the ARP lookup fails.
>
><b>Remarks:</b> This method first checks if the results are cached and valid (less than 90 seconds old). If valid, cached results are returned. Otherwise, it performs a new ARP lookup using the system's 'arp' command. The results are parsed, cached, and then returned. If there's no network connection, an empty array is returned.
#### GetLocalMachineArpEntity
>```csharp
>ArpEntity GetLocalMachineArpEntity(IPAddress ipAddress)
>```
><b>Summary:</b> Creates an ArpEntity for the local machine's IP address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The local machine's IP address.<br />
>
><b>Returns:</b> An ArpEntity for the local machine with a standard MAC address.
#### GetLoopbackArpEntity
>```csharp
>ArpEntity GetLoopbackArpEntity(IPAddress ipAddress)
>```
><b>Summary:</b> Creates an ArpEntity for a loopback address (127.0.0.1).
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The loopback IP address.<br />
>
><b>Returns:</b> An ArpEntity with a standard loopback MAC address (00-00-00-00-00-00).
#### IsLocalMachineAddress
>```csharp
>bool IsLocalMachineAddress(IPAddress ipAddress)
>```
><b>Summary:</b> Checks if the provided IP address is the local machine's IP address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address to check.<br />
>
><b>Returns:</b> True if the IP address is the local machine's IP address, otherwise false.
#### IsLoopbackAddress
>```csharp
>bool IsLoopbackAddress(IPAddress ipAddress)
>```
><b>Summary:</b> Checks if the provided IP address is a loopback address (127.x.x.x).
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address to check.<br />
>
><b>Returns:</b> True if the IP address is a loopback address, otherwise false.

<br />

## DnsLookupHelper
Provides utilities for performing DNS lookups.

>```csharp
>public static class DnsLookupHelper
>```

### Static Methods

#### GetHostname
>```csharp
>Task<string> GetHostname(IPAddress ipAddress, CancellationToken cancellationToken)
>```
><b>Summary:</b> Resolves the hostname for a given IP address asynchronously.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address to resolve the hostname for.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to monitor for cancellation requests.<br />
>
><b>Returns:</b> The hostname associated with the specified IP address or null if the lookup fails or the address is a private IP address for which the local hostname cannot be resolved.
>
><b>Remarks:</b> This method uses a SemaphoreSlim to ensure thread-safe access to the hostname and hostAddresses static fields. It first checks if the IP address is a private address. If so, and if the hostname and hostAddresses have not been previously set, it attempts to set them by resolving the local machine's hostname and IP addresses. For public IP addresses, it performs a DNS lookup to resolve the hostname. This method suppresses all exceptions, returning null in case of any errors or if the operation is canceled.

<br />

## IMacAddressVendorLookup
Finds the manufacturer (vendor) of a network interface from its MAC address.
><b>Remarks:</b> Use `Atc.Network.Helpers.IeeeOuiMacAddressVendorLookup` to look up offline from the IEEE registry files, or `Atc.Network.Helpers.OnlineMacAddressVendorLookup` to ask the macvendors.com web service.

>```csharp
>public interface IMacAddressVendorLookup
>```

### Methods

#### LookupVendorName
>```csharp
>Task<string> LookupVendorName(string macAddress, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Returns the vendor name for the MAC address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`macAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The MAC address, for example "00:1B:C5:00:90:01", "00-1B-C5-00-90-01" or "001BC5009001".<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Returns:</b> The vendor name, or <see langword="null" /> when the vendor is unknown.

<br />

## IPv4AddressHelper
Provides utilities for validating and working with IPv4 addresses.

>```csharp
>public static class IPv4AddressHelper
>```

### Static Methods

#### GetAddressesInRange
>```csharp
>IReadOnlyCollection<IPAddress> GetAddressesInRange(IPAddress startIpAddress, IPAddress endIpAddress)
>```
><b>Summary:</b> Generates a collection of IPv4 addresses within a specified range.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The starting IP address of the range.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`endIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The ending IP address of the range.<br />
>
><b>Returns:</b> A read-only collection of IP addresses within the specified range.
#### GetAddressesInRange
>```csharp
>IReadOnlyCollection<IPAddress> GetAddressesInRange(IPAddress ipAddress, int cidrLength)
>```
><b>Summary:</b> Generates a collection of IPv4 addresses within a specified range.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The starting IP address of the range.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`endIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The ending IP address of the range.<br />
>
><b>Returns:</b> A read-only collection of IP addresses within the specified range.
#### GetFirstAndLastAddressInRange
>```csharp
>ValueTuple<IPAddress, IPAddress> GetFirstAndLastAddressInRange(IPAddress ipAddress, int cidrLength)
>```
><b>Summary:</b> Calculates the first and last IP addresses in a subnet given an IP address and CIDR length.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address within the subnet.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cidrLength`&nbsp;&nbsp;-&nbsp;&nbsp;The CIDR notation length of the subnet mask.<br />
>
><b>Returns:</b> A tuple containing the first and last IP addresses in the subnet range.
#### GetLocalAddress
>```csharp
>IPAddress GetLocalAddress()
>```
><b>Summary:</b> Retrieves the local machine's IPv4 address.
>
><b>Returns:</b> The local IPv4 address, or null if not found.
#### IsValid
>```csharp
>bool IsValid(string ipAddress)
>```
><b>Summary:</b> Validates if a string is a valid IPv4 address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address in string format to validate.<br />
>
><b>Returns:</b> True if the IP address is valid; otherwise, false.
>
><b>Remarks:</b> This method checks if the string can be parsed into an IPAddress object and belongs to the IPv4 address family. It also ensures that the IP address string has exactly four octets.
#### ValidateAddresses
>```csharp
>ValueTuple<bool, string> ValidateAddresses(IPAddress startIpAddress, IPAddress endIpAddress)
>```
><b>Summary:</b> Validates that two IP addresses are valid IPv4 addresses and that the start IP is less than or equal to the end IP.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The starting IP address of the range.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`endIpAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The ending IP address of the range.<br />
>
><b>Returns:</b> A tuple containing a boolean indicating if the addresses are valid and an error message if they are not.

<br />

## IeeeOuiMacAddressVendorLookup
Looks up MAC address vendors offline, from the IEEE registry CSV files. Nothing is sent over the network.
><b>Remarks:</b> Download the files from the IEEE Registration Authority (https://regauth.standards.ieee.org/standards-ra-web/pub/view.html): `oui.csv` (MA-L, 24-bit prefixes), `mam.csv` (MA-M, 28-bit) and `oui36.csv` (MA-S, 36-bit). `oui.csv` alone covers most devices.<br>When several files are loaded, the longest matching prefix wins, so a small block inside a larger one returns the small block's owner.

>```csharp
>public class IeeeOuiMacAddressVendorLookup : IMacAddressVendorLookup
>```

### Static Methods

#### FromCsv
>```csharp
>IeeeOuiMacAddressVendorLookup FromCsv(TextReader[] csvReaders)
>```
><b>Summary:</b> Loads IEEE registry CSV content, with the columns Registry, Assignment, Organization Name and Organization Address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`csvReaders`&nbsp;&nbsp;-&nbsp;&nbsp;The CSV content. The readers are read to the end but not disposed.<br />
>
><b>Returns:</b> The lookup.
#### FromFiles
>```csharp
>IeeeOuiMacAddressVendorLookup FromFiles(string[] csvFilePaths)
>```
><b>Summary:</b> Loads IEEE registry CSV files, such as `oui.csv`, `mam.csv` and `oui36.csv`.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`csvFilePaths`&nbsp;&nbsp;-&nbsp;&nbsp;The files.<br />
>
><b>Returns:</b> The lookup.
### Properties

#### Count
>```csharp
>Count
>```
><b>Summary:</b> Gets the number of assignments loaded.
### Methods

#### LookupVendorName
>```csharp
>Task<string> LookupVendorName(string macAddress, CancellationToken cancellationToken = null)
>```

<br />

## InternetWorldTimeHelper
Provides utilities for fetching the current time from the WorldTimeAPI.

>```csharp
>public static class InternetWorldTimeHelper
>```

### Static Methods

#### GetTimeForEuropeBerlin
>```csharp
>Task<DateTime?> GetTimeForEuropeBerlin(CancellationToken cancellationToken)
>```
><b>Summary:</b> Retrieves the current time for the Europe/Berlin timezone.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to monitor for cancellation requests.<br />
>
><b>Returns:</b> A `System.DateTime` representing the current time in Europe/Berlin timezone, or null if the operation fails or is canceled.
#### GetTimeForWorldTimezone
>```csharp
>Task<DateTime?> GetTimeForWorldTimezone(string worldTimezone, CancellationToken cancellationToken)
>```
><b>Summary:</b> Retrieves the current time for a specified WorldTimeAPI timezone.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`worldTimezone`&nbsp;&nbsp;-&nbsp;&nbsp;The timezone string as defined by WorldTimeAPI.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;A token to monitor for cancellation requests.<br />
>
><b>Returns:</b> A `System.DateTime` representing the current time in the specified timezone, or null if the operation fails or is canceled.
>
><b>Remarks:</b> This method ensures thread safety by using a SemaphoreSlim to limit concurrent access. It checks for network connectivity before making the HTTP request. If the network is unavailable or the request fails, the method returns null.

<br />

## MacAddressVendorLookupHelper

>```csharp
>public static class MacAddressVendorLookupHelper
>```

### Static Methods

#### LookupVendorNameFromMacAddress
>```csharp
>Task<string> LookupVendorNameFromMacAddress(string macAddress, CancellationToken cancellationToken = null)
>```

<br />

## OnlineMacAddressVendorLookup
Looks up MAC address vendors with the macvendors.com web service, through `Atc.Network.Helpers.MacAddressVendorLookupHelper`.
><b>Remarks:</b> Each MAC address that is not cached yet is sent to macvendors.com, at most once every 1.2 seconds. Use `Atc.Network.Helpers.IeeeOuiMacAddressVendorLookup` to look up offline instead.

>```csharp
>public class OnlineMacAddressVendorLookup : IMacAddressVendorLookup
>```

### Methods

#### LookupVendorName
>```csharp
>Task<string> LookupVendorName(string macAddress, CancellationToken cancellationToken = null)
>```

<br />

## OpcUaAddressHelper
Provides utilities for validating OPC UA (Open Platform Communications Unified Architecture) addresses.

>```csharp
>public static class OpcUaAddressHelper
>```

### Static Methods

#### IsValid
>```csharp
>bool IsValid(string url, bool restrictToIp4Address = False)
>```
><b>Summary:</b> Validates the format of a given OPC UA address specified as a URL string.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`url`&nbsp;&nbsp;-&nbsp;&nbsp;The OPC UA address to validate.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`restrictToIp4Address`&nbsp;&nbsp;-&nbsp;&nbsp;Indicates whether to restrict validation to IPv4 addresses only.<br />
>
><b>Returns:</b> True if the address is a valid OPC UA address; otherwise, false.
>
><b>Remarks:</b> This method checks if the URL is an absolute URI with the scheme "opc.tcp". If `restrictToIp4Address` is true, it further validates that the host part of the URI is a valid IPv4 address.
#### IsValid
>```csharp
>bool IsValid(Uri uri, bool restrictToIp4Address = False)
>```
><b>Summary:</b> Validates the format of a given OPC UA address specified as a URL string.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`url`&nbsp;&nbsp;-&nbsp;&nbsp;The OPC UA address to validate.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`restrictToIp4Address`&nbsp;&nbsp;-&nbsp;&nbsp;Indicates whether to restrict validation to IPv4 addresses only.<br />
>
><b>Returns:</b> True if the address is a valid OPC UA address; otherwise, false.
>
><b>Remarks:</b> This method checks if the URL is an absolute URI with the scheme "opc.tcp". If `restrictToIp4Address` is true, it further validates that the host part of the URI is a valid IPv4 address.

<br />

## PingHelper
Provides utilities for performing ping operations to assess network connectivity and response time.

>```csharp
>public static class PingHelper
>```

### Static Methods

#### GetStatus
>```csharp
>Task<PingStatusResult> GetStatus(IPAddress ipAddress, TimeSpan timeout)
>```
><b>Summary:</b> Initiates a ping request to a specified IP address with a timeout.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address to ping.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`timeout`&nbsp;&nbsp;-&nbsp;&nbsp;The maximum amount of time to wait for a ping response.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in a `Atc.Network.Models.PingStatusResult`.
>
><b>Remarks:</b> This overload accepts a `System.TimeSpan` for the timeout and converts it to milliseconds before calling the main asynchronous ping method.
#### GetStatus
>```csharp
>Task<PingStatusResult> GetStatus(IPAddress ipAddress, int timeoutInMs = 1000)
>```
><b>Summary:</b> Initiates a ping request to a specified IP address with a timeout.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The IP address to ping.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`timeout`&nbsp;&nbsp;-&nbsp;&nbsp;The maximum amount of time to wait for a ping response.<br />
>
><b>Returns:</b> A task that represents the asynchronous operation, resulting in a `Atc.Network.Models.PingStatusResult`.
>
><b>Remarks:</b> This overload accepts a `System.TimeSpan` for the timeout and converts it to milliseconds before calling the main asynchronous ping method.

<br />

## TerminationHelper
Provides utilities for appending termination sequences to data arrays.

>```csharp
>public static class TerminationHelper
>```

### Static Methods

#### AppendTerminationBytesIfNeeded
>```csharp
>void AppendTerminationBytesIfNeeded(ref byte data, TerminationType terminationType)
>```
><b>Summary:</b> Appends termination bytes to a data array if the specified termination type is not already present at the end of the array.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data array to append the termination bytes to, if necessary.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`terminationType`&nbsp;&nbsp;-&nbsp;&nbsp;The type of termination sequence to append.<br />
>
><b>Remarks:</b> This method first checks if the termination type is None, in which case it does nothing. If the termination type is specified, it converts the termination type to its byte array representation and checks if the data array already ends with this sequence. If not, it appends the termination bytes to the end of the data array.

<br />

## TerminationTypeHelper
Provides utilities for handling different types of message termination characters and sequences.

>```csharp
>public static class TerminationTypeHelper
>```

### Static Fields

#### CarriageReturn
>```csharp
>byte CarriageReturn
>```
#### EndOfText
>```csharp
>byte EndOfText
>```
#### EndOfTransmission
>```csharp
>byte EndOfTransmission
>```
#### LineFeed
>```csharp
>byte LineFeed
>```
### Static Methods

#### ConvertToBytes
>```csharp
>byte[] ConvertToBytes(TerminationType terminationType)
>```
><b>Summary:</b> Converts a termination type to its byte array representation.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`terminationType`&nbsp;&nbsp;-&nbsp;&nbsp;The termination type to convert.<br />
>
><b>Returns:</b> A byte array representation of the specified termination type.
#### ConvertToString
>```csharp
>string ConvertToString(TerminationType terminationType)
>```
><b>Summary:</b> Converts a termination type to its string representation.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`terminationType`&nbsp;&nbsp;-&nbsp;&nbsp;The termination type to convert.<br />
>
><b>Returns:</b> A string representation of the specified termination type.
#### HasTerminationType
>```csharp
>bool HasTerminationType(TerminationType terminationType, byte[] data)
>```
><b>Summary:</b> Checks if the specified data contains the termination sequence for the given termination type.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`terminationType`&nbsp;&nbsp;-&nbsp;&nbsp;The termination type to check for.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to search within.<br />
>
><b>Returns:</b> True if the data contains the termination sequence; otherwise, false.
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
