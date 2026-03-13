# 🔍 IP Scanning & Network Helpers

Atc.Network includes powerful tools for network discovery, port scanning, and diagnostics.

---

## 🌐 IPScanner

Scan single IPs or entire ranges with configurable discovery options.

### ✨ Features

- 🏓 ICMP ping with network quality metrics
- 🏷️ Hostname resolution
- 📡 MAC address discovery (via ARP)
- 🏭 Vendor identification from MAC address
- 🔌 Port scanning (None, WellKnown, WellKnownAndCommon, All)
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

// result.Status, result.RoundtripTime, etc.
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

Resolve MAC addresses to vendor names using a built-in database:

```csharp
var helper = new MacAddressVendorLookupHelper();
string? vendor = helper.LookupVendorName("AA:BB:CC:DD:EE:FF");
// → "Manufacturer Name"
```