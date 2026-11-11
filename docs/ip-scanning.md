# 🔍 IP Scanning & Network Helpers

Atc.Network includes powerful tools for network discovery, port scanning, and diagnostics.

---

## 🌐 IPScanner

Scan single IPs or entire ranges with configurable discovery options.

### ✨ Features

- 🏓 ICMP ping with network quality metrics, and [traceroute](#-traceroutehelper)
- 🏷️ Hostname resolution
- 🪟 NetBIOS computer name and workgroup (`ResolveNetBiosName`), for Windows computers and Samba servers that DNS doesn't know
- 📡 MAC address discovery (via ARP)
- 🏭 Vendor identification from MAC address, online or [offline](#-offline-vendor-lookup)
- 🔌 Port scanning (None, WellKnown, WellKnownAndCommon, All), up to 16 ports per IP address at a time
- 📡 Service discovery with mDNS and SSDP (`DiscoverServices`) and ONVIF (`DiscoverOnvif`): printers, cameras, speakers and more, see [Service Discovery](discovery.md#-use-it-in-the-ip-scanner)
- 📊 Progress reporting via events

### 🚀 Scan a Range

```csharp
var config = new IPScannerConfig
{
    IcmpPing = true,
    ResolveHostName = true,
    ResolveMacAddress = true,
    ResolveVendorFromMacAddress = true,
    TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.WellKnown,
};

var scanner = new IPScanner(config);
scanner.ProgressReporting += (_, report) =>
    Console.WriteLine($"📊 {report}");

var results = await scanner.ScanRange(
    IPAddress.Parse("192.168.0.1"),
    IPAddress.Parse("192.168.0.254"),
    CancellationToken.None);
```

### 🌍 IPv6

A single IPv6 address can be scanned like an IPv4 address: ping, host name and ports.

- Ranges and CIDR ranges are IPv4 only: even the smallest usual IPv6 subnet (/64) has 2^64 addresses.
  `ScanRange` returns an error message and `ScanCidrRange` throws for IPv6.
- MAC address and vendor are not resolved for IPv6 addresses (ARP is IPv4 only), except `::1`, which is reported as the loopback interface.

### 🎯 Scan a Single IP

```csharp
var scanner = new IPScanner();
var results = await scanner.Scan(
    IPAddress.Parse("192.168.0.1"),
    CancellationToken.None);
```

### 📡 Scan a CIDR Range

```csharp
var results = await scanner.ScanCidrRange(
    IPAddress.Parse("10.0.0.0"),
    24,     // /24 = 256 addresses
    CancellationToken.None);
```

### 🔌 Custom Port Numbers

```csharp
var config = new IPScannerConfig
{
    IcmpPing = true,
    TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
};

config.PortNumbers = new List<ushort> { 22, 80, 443, 3389, 8080 };
```

---

## 🔌 IPPortScan

Test connectivity on specific ports with TCP, HTTP, or HTTPS.

```csharp
var portScan = new IPPortScan(IPAddress.Parse("192.168.0.27"));

// 🔗 Test raw TCP connectivity
bool canTcp = await portScan.CanConnectWithTcp(22, CancellationToken.None);

// 🌐 Test HTTP connectivity
bool canHttp = await portScan.CanConnectWithHttp(80, CancellationToken.None);

// 🔒 Test HTTPS connectivity
bool canHttps = await portScan.CanConnectWithHttps(443, CancellationToken.None);
```

---

## 🛠️ Network Helpers

### 🏓 PingHelper

```csharp
var result = await PingHelper.GetStatus(
    IPAddress.Parse("8.8.8.8"),
    timeoutInMs: 2000);

// result.Status, result.PingInMs, result.QualityCategory
```

### 🏷️ DnsLookupHelper

```csharp
string? hostname = await DnsLookupHelper.GetHostname(
    IPAddress.Parse("8.8.8.8"),
    CancellationToken.None);
// → "dns.google"
```

### 🧭 TracerouteHelper

Traces the route to a host, as `tracert` and `traceroute` do: each router on the way, with its answer time.

```csharp
await foreach (var hop in TracerouteHelper.Trace(IPAddress.Parse("1.1.1.1")))
{
    Console.WriteLine(hop);
}
// 1  192.168.1.254  21 ms
// 2  77.33.20.129  6 ms
// ...
// 7  *
// 8  162.158.132.2  6 ms
// 10  1.1.1.1  2 ms
```

| `TracerouteHop` | Meaning |
|-----------------|---------|
| `HopNumber` | 1, 2, 3, … (the time to live of the probe) |
| `IPAddress` | The router or target that answered; `null` when nothing answered in time (`*`) |
| `RoundtripTime` | The time until the answer; `null` when nothing answered |
| `Status` | `TtlExpired` (or `TimeExceeded` for IPv6) for a router, `Success` for the target, `TimedOut` for no answer |
| `IsDestination` | Whether the target itself answered |

The trace ends at the target, at a router that reports the target unreachable, or after `maxHops` hops (30 by default). Each hop waits up to `timeoutPerHop` (1 second by default) and gets one probe. IPv4 and IPv6 both work; when the machine has no route at all (for example no IPv6 internet), `Trace` throws a `PingException`. Many routers don't answer pings, so `*` hops in the middle are normal.

To show router names, look each address up with `DnsLookupHelper.GetHostname`.

### 🪟 NetBiosHelper

Asks a host for its NetBIOS names and MAC address, as `nbtstat -A` does. Windows computers (with NetBIOS over TCP/IP) and Samba servers, such as NAS boxes, answer on UDP port 137, often when DNS knows nothing about them.

```csharp
NetBiosNodeStatus? status = await NetBiosHelper.GetNodeStatus(IPAddress.Parse("192.168.1.212"));
// status.ComputerName → "NETDISK1512"
// status.Workgroup    → "WORKGROUP"
// status.MacAddress   → null: Samba reports none; Windows reports its MAC, such as "3C-97-0E-D6-3C-CB"
// status.Names        → NETDISK1512<00>, NETDISK1512<20>, WORKGROUP<00>, ...
```

It returns `null` when the host doesn't answer within the timeout (1 second by default), which is normal for devices without NetBIOS. It's IPv4 only.

In the IP scanner, turn on `ResolveNetBiosName` to fill `IPScanResult.NetBiosStatus` for each address; `IPScanResult.ToString()` shows the computer name when DNS found none. Addresses that don't answer wait up to `TimeoutNetBios`, so a scan of 254 addresses takes a few seconds longer.

### 📡 ArpHelper

```csharp
// Get full ARP table (cached for 90 seconds)
ArpEntity[] arpTable = ArpHelper.GetArpResult();

// Check if address is local
bool isLocal = ArpHelper.IsLocalMachineAddress(
    IPAddress.Parse("192.168.0.5"));
```

### 🏭 MacAddressVendorLookupHelper

Resolve a MAC address to the manufacturer's name:

```csharp
string? vendor = await MacAddressVendorLookupHelper.LookupVendorNameFromMacAddress(
    "AA:BB:CC:DD:EE:FF",
    CancellationToken.None);
// → "Manufacturer Name", or null when the vendor is unknown or the lookup failed
```

> 🔒 **Privacy:** there is no built-in database. Each MAC address that is not cached yet is sent over HTTPS to the
> third-party service [macvendors.com](https://macvendors.com). Answers, including "not found", are cached in
> `%TEMP%/AtcCache/macvendors.txt` (the system temp folder), and the service is called at most once every 1.2 seconds.
> `IPScanner` uses this helper when `ResolveVendorFromMacAddress` is `true` (the default). To keep MAC addresses on
> your network, look up offline (below) or set `ResolveVendorFromMacAddress` to `false`.

### 📴 Offline vendor lookup

`IeeeOuiMacAddressVendorLookup` looks up vendors in the IEEE registry files instead, with no network calls and no rate limit.
Download the CSV files once from the [IEEE Registration Authority](https://regauth.standards.ieee.org/standards-ra-web/pub/view.html):
`oui.csv` (MA-L) covers most devices; add `mam.csv` (MA-M) and `oui36.csv` (MA-S) for the smaller blocks. When several
files are loaded, the longest matching prefix wins.

```csharp
var vendorLookup = IeeeOuiMacAddressVendorLookup.FromFiles("oui.csv", "mam.csv", "oui36.csv");

string? vendor = await vendorLookup.LookupVendorName("00:1B:C5:00:90:01");

// Let the scanner use it
using var ipScanner = new IPScanner(logger, new IPScannerConfig(), vendorLookup);
```

Both lookups implement `IMacAddressVendorLookup` (`OnlineMacAddressVendorLookup` is the macvendors.com one), so you can
also plug in your own, for example one backed by a database.