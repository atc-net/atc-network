# 📡 Service Discovery (mDNS & SSDP)

Find devices and services on the local network that announce themselves: printers, cameras, speakers, TVs, Chromecasts, NAS boxes, routers and smart-home hubs. Reverse DNS usually knows nothing about these devices; mDNS and SSDP give their names, models and services.

| Protocol | Also known as | Typical devices |
|----------|---------------|-----------------|
| **mDNS / DNS-SD** (RFC 6762 / 6763) | Bonjour, Avahi, Zeroconf | Printers, AirPlay, Chromecast, Sonos, NAS, cameras, Matter devices |
| **SSDP** | UPnP | Routers, media servers and renderers, TVs, receivers |

---

## 🔍 mDNS / DNS-SD

### Find Every Service

```csharp
await foreach (var service in MdnsDiscovery.BrowseAll())
{
    Console.WriteLine($"{service.InstanceName} ({service.ServiceType}) at {service.HostName}:{service.Port}");
}
```

`BrowseAll` first asks which service types exist, then asks for the instances of each.

### Find One Service Type

```csharp
await foreach (var printer in MdnsDiscovery.Browse("_ipp._tcp"))
{
    Console.WriteLine($"🖨️ {printer.InstanceName} - {string.Join(", ", printer.IPAddresses)}");
    Console.WriteLine($"   Model: {printer.Properties.GetValueOrDefault("ty")}");
}
```

".local" is added when it is missing. Common service types:

| Service type | What |
|--------------|------|
| `_http._tcp` | Web interfaces |
| `_ipp._tcp`, `_printer._tcp` | Printers |
| `_googlecast._tcp` | Chromecast and Google speakers |
| `_airplay._tcp`, `_raop._tcp` | AirPlay |
| `_smb._tcp`, `_afpovertcp._tcp` | File shares |
| `_ssh._tcp` | SSH servers |
| `_matter._tcp` | Matter smart-home devices |

### List the Service Types

```csharp
await foreach (var serviceType in MdnsDiscovery.GetServiceTypes())
{
    Console.WriteLine(serviceType); // _http._tcp.local, _googlecast._tcp.local, ...
}
```

### 📋 MdnsServiceInstance

| Property | Meaning |
|----------|---------|
| `InstanceName` | The service's own name, such as "Office Printer" |
| `ServiceType` | Such as `_ipp._tcp.local` |
| `FullName` | The instance name followed by the service type |
| `HostName` | The host, such as `printer.local` |
| `Port` | The service's port |
| `IPAddresses` | The host's IPv4 and IPv6 addresses |
| `Properties` | The TXT record's key/value pairs (case-insensitive keys; a key without `=` has a `null` value) |

Each service is returned once, as soon as its host, port and address are known. When a responder leaves out records, they are asked for. A service that is still incomplete when the time is up is returned at the end with what is known (`HostName` `null`, `Port` 0 or no addresses).

---

## 📺 SSDP (UPnP)

```csharp
await foreach (var device in SsdpDiscovery.Search())
{
    Console.WriteLine($"{device.IPAddress} {device.Server} - {device.SearchTarget}");
}
```

| Search target | Finds |
|---------------|-------|
| `ssdp:all` (default) | Every device and service; a device answers once per device and service type |
| `upnp:rootdevice` | One answer per device |
| `urn:schemas-upnp-org:device:MediaRenderer:1` | One device type |

### 📋 SsdpDevice

| Property | Meaning |
|----------|---------|
| `IPAddress` | The address the answer came from |
| `Server` | The operating system and product, such as `Linux UPnP/1.0 Sonos/57.23` |
| `SearchTarget` | The device or service type that matched (`ST`) |
| `UniqueServiceName` | Identifies the device or service (`USN`); each is returned once |
| `Location` | The URL of the device description (an XML document with the model and manufacturer) |
| `Headers` | All headers, with case-insensitive names |

The device description at `Location` is not fetched.

---

## 🌐 Use It in the IP Scanner

Turn on `DiscoverServices` to attach the services to the scanned addresses:

```csharp
var scanner = new IPScanner(new IPScannerConfig
{
    DiscoverServices = true,
    TimeoutDiscovery = TimeSpan.FromSeconds(3),
});

var results = await scanner.ScanRange(
    IPAddress.Parse("192.168.1.1"),
    IPAddress.Parse("192.168.1.254"),
    CancellationToken.None);

foreach (var result in results.CollectedResults.Where(x => x.MdnsServices.Count > 0))
{
    Console.WriteLine($"{result.IPAddress} {result.MdnsHostName}");
    foreach (var service in result.MdnsServices)
    {
        Console.WriteLine($"   {service.ServiceType} '{service.InstanceName}' :{service.Port}");
    }
}
```

| `IPScanResult` property | Meaning |
|-------------------------|---------|
| `MdnsServices` | The mDNS services whose host has this address |
| `SsdpDevices` | The SSDP answers that came from this address |
| `MdnsHostName` | The host name the address announces, such as `printer.local`; `ToString()` shows it when reverse DNS found none |

Discovery runs once per scan, next to the other checks, so it adds at most `TimeoutDiscovery` to a scan. When it fails (for example without a network), the failure is logged (event id 10500) and the scan completes without services.

---

## ⏱️ Duration and Cancellation

Discovery collects answers for a fixed time: 3 seconds by default, or the `duration` you pass. The sequence then ends normally. Cancelling the `CancellationToken` before that throws an `OperationCanceledException`.

---

## ⚙️ How It Works

- Queries are sent from a random port, never from 5353 or 1900, which the operating system often uses already (Windows' DNS Client and SSDP Discovery services, Bonjour). Responders answer by unicast to that port.
- Queries go out on every network interface that is up and has an IPv4 address, so devices behind every adapter are found, not only those behind the default one.
- Answers are read defensively: malformed packets are ignored.

**Limitations:** IPv4 only (mDNS over IPv6, `ff02::fb`, is not used, though IPv6 addresses in answers are kept). Firewalls that block unicast replies to multicast queries hide the answers.

---

## 📈 Telemetry

Queries and answers are recorded on the message and byte counters with the component tags `mdns_discovery` and `ssdp_discovery`, with the spans `MdnsDiscovery.Browse`, `MdnsDiscovery.BrowseAll`, `MdnsDiscovery.GetServiceTypes` and `SsdpDiscovery.Search`. See [Metrics & Tracing](telemetry.md).