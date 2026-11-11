<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Discovery

<br />

## MdnsDiscovery
Finds services on the local network with mDNS / DNS-SD (Bonjour, Avahi): printers, web interfaces, Chromecasts, AirPlay devices, NAS boxes and other devices that announce themselves.
><b>Remarks:</b> Queries are sent to 224.0.0.251:5353 on every IPv4 network interface, from a random port, so the responders answer by unicast ("one-shot" queries, RFC 6762, section 5.1). Port 5353 itself is not used, because the operating system often has it already. IPv4 only.<br>Each sequence ends normally when the duration is over. Cancelling the token before that throws an `System.OperationCanceledException`.

>```csharp
>public static class MdnsDiscovery
>```

### Static Fields

#### DefaultDurationInMs
>```csharp
>int DefaultDurationInMs
>```
><b>Summary:</b> The default time to collect answers, in milliseconds.
#### ServiceTypesName
>```csharp
>string ServiceTypesName
>```
><b>Summary:</b> The DNS-SD name that lists every service type on the network (RFC 6763, section 9).
### Static Properties

#### MulticastEndPoint
>```csharp
>MulticastEndPoint
>```
><b>Summary:</b> Gets the mDNS multicast endpoint: 224.0.0.251:5353.
### Static Methods

#### Browse
>```csharp
>IAsyncEnumerable<MdnsServiceInstance> Browse(string serviceType, TimeSpan? duration = null, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Finds the instances of one service type, such as `_http._tcp.local` or `_ipp._tcp`.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`serviceType`&nbsp;&nbsp;-&nbsp;&nbsp;The service type. ".local" is added when it is missing.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`duration`&nbsp;&nbsp;-&nbsp;&nbsp;How long to collect answers. The default is 3 seconds.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> Each instance once, as soon as its host, port and address are known. Instances that are still incomplete when the duration is over are returned at the end, with what is known.
#### BrowseAll
>```csharp
>IAsyncEnumerable<MdnsServiceInstance> BrowseAll(TimeSpan? duration = null, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Finds every service type on the network, and the instances of each.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`duration`&nbsp;&nbsp;-&nbsp;&nbsp;How long to collect answers. The default is 3 seconds.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> Each instance once, as soon as its host, port and address are known. Instances that are still incomplete when the duration is over are returned at the end, with what is known.
#### GetServiceTypes
>```csharp
>IAsyncEnumerable<string> GetServiceTypes(TimeSpan? duration = null, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Finds the service types on the network, such as `_http._tcp.local` and `_googlecast._tcp.local`.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`duration`&nbsp;&nbsp;-&nbsp;&nbsp;How long to collect answers. The default is 3 seconds.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> Each service type once, as it is found.

<br />

## MdnsServiceInstance
A service found with mDNS / DNS-SD, such as a web interface, a printer or a Chromecast.

>```csharp
>public class MdnsServiceInstance
>```

### Properties

#### FullName
>```csharp
>FullName
>```
><b>Summary:</b> Gets the full name: the instance name followed by the service type.
#### HostName
>```csharp
>HostName
>```
><b>Summary:</b> Gets the host the service runs on, such as "printer.local", or <see langword="null" /> when it did not answer in time.
#### IPAddresses
>```csharp
>IPAddresses
>```
><b>Summary:</b> Gets the host's IPv4 and IPv6 addresses. Empty when they did not arrive in time.
#### InstanceName
>```csharp
>InstanceName
>```
><b>Summary:</b> Gets the instance's own name, such as "Office Printer".
#### Port
>```csharp
>Port
>```
><b>Summary:</b> Gets the service's port, or 0 when it did not answer in time.
#### Properties
>```csharp
>Properties
>```
><b>Summary:</b> Gets the TXT record's key/value pairs, such as "model" or "path". A key without '=' has a <see langword="null" /> value. Keys are case-insensitive.
#### ServiceType
>```csharp
>ServiceType
>```
><b>Summary:</b> Gets the service type, such as "_ipp._tcp.local".
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## SsdpDevice
A device or service that answered an SSDP (UPnP) search.

>```csharp
>public class SsdpDevice
>```

### Properties

#### Headers
>```csharp
>Headers
>```
><b>Summary:</b> Gets all headers of the answer. Names are case-insensitive.
#### IPAddress
>```csharp
>IPAddress
>```
><b>Summary:</b> Gets the address the answer came from.
#### Location
>```csharp
>Location
>```
><b>Summary:</b> Gets the URL of the device description (LOCATION header), an XML document with the model and manufacturer.
#### SearchTarget
>```csharp
>SearchTarget
>```
><b>Summary:</b> Gets the search target that matched (ST header), such as "upnp:rootdevice" or a device type URN.
#### Server
>```csharp
>Server
>```
><b>Summary:</b> Gets the operating system and product (SERVER header), such as "Linux/5.4 UPnP/1.1 MiniUPnPd/2.2".
#### UniqueServiceName
>```csharp
>UniqueServiceName
>```
><b>Summary:</b> Gets the unique service name (USN header), which identifies the device or service.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## SsdpDiscovery
Finds UPnP devices and services on the local network with SSDP (Simple Service Discovery Protocol): routers, TVs, media servers, printers, NAS boxes and other devices.
><b>Remarks:</b> An M-SEARCH request is sent to 239.255.255.250:1900 on every IPv4 network interface; devices answer by unicast with their headers, such as SERVER and LOCATION. IPv4 only.

>```csharp
>public static class SsdpDiscovery
>```

### Static Fields

#### DefaultDurationInMs
>```csharp
>int DefaultDurationInMs
>```
><b>Summary:</b> The default time to collect answers, in milliseconds.
#### DefaultSearchTarget
>```csharp
>string DefaultSearchTarget
>```
><b>Summary:</b> The search target for all devices and services.
### Static Properties

#### MulticastEndPoint
>```csharp
>MulticastEndPoint
>```
><b>Summary:</b> Gets the SSDP multicast endpoint: 239.255.255.250:1900.
### Static Methods

#### Search
>```csharp
>IAsyncEnumerable<SsdpDevice> Search(string searchTarget = ssdp:all, TimeSpan? duration = null, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Searches the local network, and returns each device or service as it answers.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`searchTarget`&nbsp;&nbsp;-&nbsp;&nbsp;
            What to search for: ssdp:all (the default), upnp:rootdevice for one answer per device,
            or a device or service type such as urn:schemas-upnp-org:device:MediaRenderer:1.
            <br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`duration`&nbsp;&nbsp;-&nbsp;&nbsp;How long to collect answers. The default is 3 seconds.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The devices and services, each unique service name (USN) once. The sequence ends when the duration is over.
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
