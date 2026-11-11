# 🔍 IP Scanning & Network Helpers

Atc.Network includes powerful tools for network discovery, port scanning, and diagnostics.

---

## 🌐 IPScanner

Scan single IPs or entire ranges with configurable discovery options.

### ✨ Features

- 🏓 ICMP ping with network quality metrics
- 🏷️ Hostname resolution
- 📡 MAC address discovery (via ARP)
- 🏭 Vendor identification from MAC address, online or [offline](#-offline-vendor-lookup)
- 🔌 Port scanning (None, WellKnown, WellKnownAndCommon, All), up to 16 ports per IP address at a time
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