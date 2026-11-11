# 📡 Service Discovery (mDNS, SSDP & ONVIF)

Find devices and services on the local network that announce themselves: printers, cameras, speakers, TVs, Chromecasts, NAS boxes, routers and smart-home hubs. Reverse DNS usually knows nothing about these devices; mDNS, SSDP and ONVIF give their names, models and services.

| Protocol | Also known as | Typical devices |
|----------|---------------|-----------------|
| **mDNS / DNS-SD** (RFC 6762 / 6763) | Bonjour, Avahi, Zeroconf | Printers, AirPlay, Chromecast, Sonos, NAS, cameras, Matter devices |
| **SSDP** | UPnP | Routers, media servers and renderers, TVs, receivers |
| **ONVIF WS-Discovery** | ONVIF | IP cameras of every brand with ONVIF on, video encoders, NVRs |

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

## 📹 ONVIF (WS-Discovery)

```csharp
await foreach (var device in OnvifDiscovery.Probe())
{
    Console.WriteLine($"{device.IPAddress} {device.Name} ({device.Hardware}) - {device.DeviceServiceUrl}");
}
// 192.168.1.132 C210 (C210) - http://192.168.1.132:2020/onvif/device_service
```

Two probes are sent: one for cameras (`NetworkVideoTransmitter`) and one for every ONVIF device (`Device`), since some devices only answer one of them.

### 📋 OnvifDevice

| Property | Meaning |
|----------|---------|
| `IPAddress` | The address the answer came from |
| `Name`, `Hardware`, `Location` | From the device's scopes, such as `C210`, `HIKVISION DS-2CD2183G0-IU` |
| `DeviceServiceUrl` | The ONVIF device service, such as `http://192.168.1.132:2020/onvif/device_service`; pass it to [`OnvifClient`](onvif.md) for the model and stream URLs |
| `XAddrs` | Every device service URL the device listed |
| `IsNetworkVideoTransmitter` | Whether it is a camera or video encoder (not, say, an NVR) |
| `Types`, `Scopes` | As the device sent them (scopes percent-decoded) |
| `EndpointReference` | A stable id that stays the same when the device's IP changes; each device is returned once by it |

Many cameras ship with ONVIF turned off (common on Hikvision); turn it on in the camera's web interface, or find the camera through mDNS or SSDP instead.

---

## 🌐 Use It in the IP Scanner

Turn on `DiscoverServices` (mDNS and SSDP) and `DiscoverOnvif` to attach the services and devices to the scanned addresses:

```csharp
var scanner = new IPScanner(new IPScannerConfig
{
    DiscoverServices = true,
    DiscoverOnvif = true,
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
| `OnvifDevices` | The ONVIF devices that answered from this address, or whose device service URL is on it |
| `MdnsHostName` | The host name the address announces, such as `printer.local`; `ToString()` shows it when reverse DNS found none |

Discovery runs once per scan, next to the other checks, so it adds at most `TimeoutDiscovery` to a scan. When it fails (for example without a network), the failure is logged (event id 10500) and the scan completes without services.

---

## ⏱️ Duration and Cancellation

Discovery collects answers for a fixed time: 3 seconds by default, or the `duration` you pass. The sequence then ends normally. Cancelling the `CancellationToken` before that throws an `OperationCanceledException`.

---

## ⚙️ How It Works

- Queries are sent from a random port, never from 5353, 1900 or 3702, which the operating system often uses already (Windows' DNS Client and SSDP Discovery services, Bonjour). Responders answer by unicast to that port.
- Queries go out on every network interface that is up and has an IPv4 address, so devices behind every adapter are found, not only those behind the default one.
- Answers are read defensively: malformed packets are ignored.

**Limitations:** IPv4 only (mDNS over IPv6, `ff02::fb`, is not used, though IPv6 addresses in answers are kept). Firewalls that block unicast replies to multicast queries hide the answers.

---

## 📈 Telemetry

Queries and answers are recorded on the message and byte counters with the component tags `mdns_discovery`, `ssdp_discovery` and `onvif_discovery`, with the spans `MdnsDiscovery.Browse`, `MdnsDiscovery.BrowseAll`, `MdnsDiscovery.GetServiceTypes`, `SsdpDiscovery.Search` and `OnvifDiscovery.Probe`. See [Metrics & Tracing](telemetry.md).