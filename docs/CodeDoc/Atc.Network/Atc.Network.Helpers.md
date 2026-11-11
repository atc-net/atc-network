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
><b>Remarks:</b> For new code, `Atc.Network.Helpers.NtpHelper.GetTime(System.String,System.Nullable{System.TimeSpan},System.Threading.CancellationToken)` is the better way to get the time. It asks an NTP server directly, which is the standard for time synchronization and is not tied to one web service. It works on a LAN without internet access (against a router or a domain controller), reports failures with an exception instead of <see langword="null" />, and also returns the local clock's offset and the round-trip delay. NTP gives UTC; for the time in a time zone, use `TimeZoneInfo.ConvertTimeFromUtc(result.UtcTime, TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"))`.

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
>
><b>Remarks:</b> For new code, `Atc.Network.Helpers.NtpHelper.GetTime(System.String,System.Nullable{System.TimeSpan},System.Threading.CancellationToken)` is recommended instead; see the class remarks.
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
><b>Remarks:</b> This method ensures thread safety by using a SemaphoreSlim to limit concurrent access. It checks for network connectivity before making the HTTP request. If the network is unavailable or the request fails, the method returns null. For new code, `Atc.Network.Helpers.NtpHelper.GetTime(System.String,System.Nullable{System.TimeSpan},System.Threading.CancellationToken)` is recommended instead: it uses NTP, the standard time protocol, reports why it failed, and converts to a time zone with `System.TimeZoneInfo`.

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

## NetBiosHelper
Asks a host for its NetBIOS names and MAC address with a node status query (RFC 1002, as `nbtstat -A` does).
><b>Remarks:</b> Windows computers with NetBIOS over TCP/IP, and Samba servers (NAS boxes, Linux file servers), answer on UDP port 137 with their computer name and workgroup, even when DNS knows nothing about them. Most other devices don't answer.

>```csharp
>public static class NetBiosHelper
>```

### Static Fields

#### DefaultPort
>```csharp
>int DefaultPort
>```
><b>Summary:</b> The NetBIOS name service port.
#### DefaultTimeoutInMs
>```csharp
>int DefaultTimeoutInMs
>```
><b>Summary:</b> The default time to wait for the answer, in milliseconds.
### Static Methods

#### GetNodeStatus
>```csharp
>Task<NetBiosNodeStatus> GetNodeStatus(IPAddress ipAddress, TimeSpan? timeout = null, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Asks a host for its NetBIOS names and MAC address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The host's IPv4 address.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`timeout`&nbsp;&nbsp;-&nbsp;&nbsp;How long to wait for the answer. The default is 1 second.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The host's names and MAC address, or <see langword="null" /> when it does not answer in time, which is normal for hosts without NetBIOS.

<br />

## NtpHelper
Gets the time from an NTP server with SNTP (RFC 4330), and how far the local clock is from it.
><b>Remarks:</b> One request is sent per call, as a UDP datagram to port 123. The result has the clock offset and the round-trip delay, from the four NTP timestamps. NTP servers serve UTC; convert it with `System.TimeZoneInfo.ConvertTimeFromUtc(System.DateTime,System.TimeZoneInfo)` for a time zone. Public servers limit how often a client may ask; don't call this more than about once a minute per server.

>```csharp
>public static class NtpHelper
>```

### Static Fields

#### DefaultHost
>```csharp
>string DefaultHost
>```
><b>Summary:</b> The default server: the NTP Pool, which picks a nearby public server.
#### DefaultPort
>```csharp
>int DefaultPort
>```
><b>Summary:</b> The NTP port.
#### DefaultTimeoutInMs
>```csharp
>int DefaultTimeoutInMs
>```
><b>Summary:</b> The default time to wait for the reply, in milliseconds.
### Static Methods

#### GetTime
>```csharp
>Task<NtpResult> GetTime(string host = pool.ntp.org, TimeSpan? timeout = null, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Gets the time from an NTP server by host name or IP address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`host`&nbsp;&nbsp;-&nbsp;&nbsp;The server's host name or IP address. The default is .<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`timeout`&nbsp;&nbsp;-&nbsp;&nbsp;How long to wait for the reply. The default is 5 seconds. Resolving the host name is not included.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The server's time and the local clock's offset from it.
#### GetTime
>```csharp
>Task<NtpResult> GetTime(IPEndPoint endPoint, TimeSpan? timeout = null, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Gets the time from an NTP server by host name or IP address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`host`&nbsp;&nbsp;-&nbsp;&nbsp;The server's host name or IP address. The default is .<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`timeout`&nbsp;&nbsp;-&nbsp;&nbsp;How long to wait for the reply. The default is 5 seconds. Resolving the host name is not included.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The server's time and the local clock's offset from it.

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

<br />

## WakeOnLanHelper
Wakes a sleeping or powered-off device on the network by sending it a Wake-on-LAN "magic packet".
><b>Remarks:</b> The magic packet is 6 bytes of 0xFF followed by the device's MAC address 16 times (102 bytes), optionally followed by a 4- or 6-byte SecureOn password. It is sent as a UDP datagram, by default as a broadcast to port 9. The device must have Wake-on-LAN enabled.

>```csharp
>public static class WakeOnLanHelper
>```

### Static Fields

#### DefaultPort
>```csharp
>int DefaultPort
>```
><b>Summary:</b> The default port for Wake-on-LAN (the discard port). Port 7 is also common.
### Static Properties

#### DefaultEndPoint
>```csharp
>DefaultEndPoint
>```
><b>Summary:</b> Gets the default endpoint: the global broadcast address (255.255.255.255) on port 9.
>
><b>Remarks:</b> Routers do not forward the global broadcast. To wake a device on another subnet, send to that subnet's broadcast address instead, such as 192.168.1.255.
### Static Methods

#### CreateMagicPacket
>```csharp
>byte[] CreateMagicPacket(string macAddress, byte[] secureOnPassword = null)
>```
><b>Summary:</b> Creates the magic packet for a MAC address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`macAddress`&nbsp;&nbsp;-&nbsp;&nbsp;
            The MAC address, such as "01-23-45-67-89-AB", "01:23:45:67:89:AB", "0123456789AB" or "0123.4567.89AB".
            <br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`secureOnPassword`&nbsp;&nbsp;-&nbsp;&nbsp;An optional SecureOn password of 4 or 6 bytes, appended to the packet.<br />
>
><b>Returns:</b> The magic packet.
#### CreateMagicPacket
>```csharp
>byte[] CreateMagicPacket(PhysicalAddress macAddress, byte[] secureOnPassword = null)
>```
><b>Summary:</b> Creates the magic packet for a MAC address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`macAddress`&nbsp;&nbsp;-&nbsp;&nbsp;
            The MAC address, such as "01-23-45-67-89-AB", "01:23:45:67:89:AB", "0123456789AB" or "0123.4567.89AB".
            <br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`secureOnPassword`&nbsp;&nbsp;-&nbsp;&nbsp;An optional SecureOn password of 4 or 6 bytes, appended to the packet.<br />
>
><b>Returns:</b> The magic packet.
#### SendMagicPacket
>```csharp
>Task SendMagicPacket(string macAddress, IPEndPoint endPoint = null, byte[] secureOnPassword = null, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Sends the magic packet for a MAC address.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`macAddress`&nbsp;&nbsp;-&nbsp;&nbsp;
            The MAC address, such as "01-23-45-67-89-AB", "01:23:45:67:89:AB", "0123456789AB" or "0123.4567.89AB".
            <br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`endPoint`&nbsp;&nbsp;-&nbsp;&nbsp;
            Where to send the packet. The default is  (255.255.255.255:9).
            <br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`secureOnPassword`&nbsp;&nbsp;-&nbsp;&nbsp;An optional SecureOn password of 4 or 6 bytes.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> A task that completes when the packet has been sent.
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
