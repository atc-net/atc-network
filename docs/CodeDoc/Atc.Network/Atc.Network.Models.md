<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Models

<br />

## ArpEntity

>```csharp
>public class ArpEntity
>```

### Properties

#### IPAddress
>```csharp
>IPAddress
>```
#### MacAddress
>```csharp
>MacAddress
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

<br />

## IPScanPortResult

>```csharp
>public class IPScanPortResult
>```

### Properties

#### CanConnect
>```csharp
>CanConnect
>```
#### IPAddress
>```csharp
>IPAddress
>```
#### Port
>```csharp
>Port
>```
#### ServiceProtocol
>```csharp
>ServiceProtocol
>```
#### TransportProtocol
>```csharp
>TransportProtocol
>```
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## IPScanResult

>```csharp
>public class IPScanResult
>```

### Properties

#### End
>```csharp
>End
>```
#### ErrorMessage
>```csharp
>ErrorMessage
>```
#### HasConnection
>```csharp
>HasConnection
>```
#### Hostname
>```csharp
>Hostname
>```
#### IPAddress
>```csharp
>IPAddress
>```
#### IsCompleted
>```csharp
>IsCompleted
>```
#### MacAddress
>```csharp
>MacAddress
>```
#### MacVendor
>```csharp
>MacVendor
>```
#### MdnsHostName
>```csharp
>MdnsHostName
>```
><b>Summary:</b> Gets the host name the address announces with mDNS, such as "printer.local", or <see langword="null" />. It often names devices that reverse DNS (`Atc.Network.Models.IPScanResult.Hostname`) does not know.
#### MdnsServices
>```csharp
>MdnsServices
>```
><b>Summary:</b> Gets or sets the mDNS / DNS-SD services found on this address, when `Atc.Network.Internet.IPScannerConfig.DiscoverServices` is on.
#### NetBiosStatus
>```csharp
>NetBiosStatus
>```
><b>Summary:</b> Gets or sets the NetBIOS names and MAC address the address reported, when `Atc.Network.Internet.IPScannerConfig.ResolveNetBiosName` is on and it answered; otherwise <see langword="null" />.
#### OnvifDevices
>```csharp
>OnvifDevices
>```
><b>Summary:</b> Gets or sets the ONVIF devices (cameras, NVRs) found on this address, when `Atc.Network.Internet.IPScannerConfig.DiscoverOnvif` is on.
#### OpenPortNumbers
>```csharp
>OpenPortNumbers
>```
#### PingStatus
>```csharp
>PingStatus
>```
#### Ports
>```csharp
>Ports
>```
#### SsdpDevices
>```csharp
>SsdpDevices
>```
><b>Summary:</b> Gets or sets the SSDP (UPnP) devices and services found on this address, when `Atc.Network.Internet.IPScannerConfig.DiscoverServices` is on.
#### Start
>```csharp
>Start
>```
#### TimeDiff
>```csharp
>TimeDiff
>```
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## IPScanResults

>```csharp
>public class IPScanResults
>```

### Properties

#### CollectedResults
>```csharp
>CollectedResults
>```
#### CollectedResultsFilteredOnHasConnections
>```csharp
>CollectedResultsFilteredOnHasConnections
>```
#### End
>```csharp
>End
>```
#### ErrorMessage
>```csharp
>ErrorMessage
>```
#### IsCompleted
>```csharp
>IsCompleted
>```
#### PercentageCompleted
>```csharp
>PercentageCompleted
>```
#### Start
>```csharp
>Start
>```
#### TimeDiff
>```csharp
>TimeDiff
>```
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## NetBiosName
A name a host has registered with NetBIOS.

>```csharp
>public class NetBiosName : IEquatable<NetBiosName>
>```

### Properties

#### IsGroup
>```csharp
>IsGroup
>```
><b>Summary:</b> Whether it is a group name (a workgroup or domain) rather than a unique name.
#### Name
>```csharp
>Name
>```
><b>Summary:</b> The name, such as the computer or workgroup name.
#### Suffix
>```csharp
>Suffix
>```
><b>Summary:</b> The 16th byte, which says what the name is for: 0x00 workstation (or workgroup, for a group name), 0x20 file server, 0x1B domain master browser, 0x1C domain controllers, 0x1D master browser.
### Methods

#### <Clone>$
>```csharp
>NetBiosName <Clone>$()
>```
#### Deconstruct
>```csharp
>void Deconstruct(out string Name, out byte Suffix, out bool IsGroup)
>```
#### Equals
>```csharp
>bool Equals(object obj)
>```
#### Equals
>```csharp
>bool Equals(NetBiosName other)
>```
#### GetHashCode
>```csharp
>int GetHashCode()
>```
#### ToString
>```csharp
>string ToString()
>```
><b>Summary:</b> Returns the name with its suffix in hexadecimal, as nbtstat shows it: "DESKTOP-ABC&lt;20&gt;".
>
><b>Returns:</b> The name and suffix.

<br />

## NetBiosNodeStatus
The answer of a host to a NetBIOS node status query: its names and MAC address.

>```csharp
>public class NetBiosNodeStatus
>```

### Properties

#### ComputerName
>```csharp
>ComputerName
>```
><b>Summary:</b> Gets the computer name: the unique name with suffix 0x00 (the workstation service), or <see langword="null" />.
#### IPAddress
>```csharp
>IPAddress
>```
><b>Summary:</b> Gets the address that answered.
#### MacAddress
>```csharp
>MacAddress
>```
><b>Summary:</b> Gets the MAC address the host reports, such as "00-15-5D-01-02-03", or <see langword="null" /> when it reports none (Samba reports zeros).
#### Names
>```csharp
>Names
>```
><b>Summary:</b> Gets every name the host has registered.
#### Workgroup
>```csharp
>Workgroup
>```
><b>Summary:</b> Gets the workgroup or domain: the group name with suffix 0x00, or <see langword="null" />.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## NtpResult
The time from an NTP server, and how far the local clock is from it.

>```csharp
>public class NtpResult
>```

### Properties

#### Offset
>```csharp
>Offset
>```
><b>Summary:</b> Gets how far the server's clock is ahead of the local clock. A negative value means the local clock is ahead. Add it to `System.DateTime.UtcNow` to get the server's time.
#### RoundTripDelay
>```csharp
>RoundTripDelay
>```
><b>Summary:</b> Gets the time the request and the reply spent on the network, without the time the server needed to answer.
#### Server
>```csharp
>Server
>```
><b>Summary:</b> Gets the server that answered.
#### Stratum
>```csharp
>Stratum
>```
><b>Summary:</b> Gets the server's distance from a reference clock: 1 for a server with its own clock (GPS, atomic), 2 for a server that synchronizes with a stratum 1 server, and so on.
#### UtcTime
>```csharp
>UtcTime
>```
><b>Summary:</b> Gets the server's time (UTC) at the moment the reply arrived: the local time then, plus `Atc.Network.Models.NtpResult.Offset`.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## PingStatusResult

>```csharp
>public class PingStatusResult
>```

### Properties

#### Exception
>```csharp
>Exception
>```
#### IPAddress
>```csharp
>IPAddress
>```
#### PingInMs
>```csharp
>PingInMs
>```
#### QualityCategory
>```csharp
>QualityCategory
>```
#### QualityCategoryDescriptionAndTime
>```csharp
>QualityCategoryDescriptionAndTime
>```
#### Status
>```csharp
>Status
>```
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## TracerouteHop
One hop of a traceroute: a router on the way, or the target.

>```csharp
>public class TracerouteHop : IEquatable<TracerouteHop>
>```

### Properties

#### HopNumber
>```csharp
>HopNumber
>```
><b>Summary:</b> The hop's number, starting at 1: the time to live of the probe.
#### IPAddress
>```csharp
>IPAddress
>```
><b>Summary:</b> The address that answered, or <see langword="null" /> when nothing answered in time.
#### IsDestination
>```csharp
>IsDestination
>```
><b>Summary:</b> Whether the target itself answered.
#### RoundtripTime
>```csharp
>RoundtripTime
>```
><b>Summary:</b> The time until the answer, or <see langword="null" /> when nothing answered in time.
#### Status
>```csharp
>Status
>```
><b>Summary:</b> `System.Net.NetworkInformation.IPStatus.TtlExpired` (or `System.Net.NetworkInformation.IPStatus.TimeExceeded`) for a router, `System.Net.NetworkInformation.IPStatus.Success` for the target, `System.Net.NetworkInformation.IPStatus.TimedOut` when nothing answered, or another status, such as `System.Net.NetworkInformation.IPStatus.DestinationHostUnreachable`, that ends the trace.
### Methods

#### <Clone>$
>```csharp
>TracerouteHop <Clone>$()
>```
#### Deconstruct
>```csharp
>void Deconstruct(out int HopNumber, out IPAddress IPAddress, out TimeSpan? RoundtripTime, out IPStatus Status, out bool IsDestination)
>```
#### Equals
>```csharp
>bool Equals(object obj)
>```
#### Equals
>```csharp
>bool Equals(TracerouteHop other)
>```
#### GetHashCode
>```csharp
>int GetHashCode()
>```
#### ToString
>```csharp
>string ToString()
>```
><b>Summary:</b> Returns the hop as a traceroute line: "1 192.168.1.1 1 ms", or "2 *" when nothing answered.
>
><b>Returns:</b> The line.
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
