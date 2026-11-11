[![NuGet Version](https://img.shields.io/nuget/v/atc.network.svg?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/atc.network)

# 🌐 Atc.Network

A .NET 10 library for network communication, scanning, and remote desktop — batteries included.

---

## 🤔 Why Atc.Network?

| | |
|---|---|
| 🔌 **TCP & UDP** | Event-driven clients and servers with auto-reconnect, keep-alive, optional TLS, and configurable timeouts |
| 🖥️ **VNC Client** | Cross-platform RFB protocol client — Raw, CopyRect, RRE, CoRRE, Hextile, ZRLE encodings |
| 🔍 **IP Scanning** | Scan ranges or CIDR blocks with ICMP ping, hostname/MAC resolution, vendor lookup, and port scanning |
| ⏰ **Wake-on-LAN** | Wake sleeping devices with a magic packet, by broadcast or to a chosen subnet |
| 🕰️ **NTP Time** | Get the time from an NTP server, with the local clock's offset and the round-trip delay |
| 📡 **Service Discovery** | Find printers, cameras, speakers, TVs and other devices with mDNS / DNS-SD and SSDP (UPnP) |
| 🧩 **Interface-first** | Every component has an `I*` interface — built for DI and unit testing |
| 📝 **Structured Logging** | `[LoggerMessage]` source generators throughout — zero-allocation, high-performance logging |
| 🐧 **Cross-platform** | Windows, Linux, macOS — no `System.Drawing`, no P/Invoke, no WinForms |

---

## 📦 Installation

```bash
dotnet add package Atc.Network
```

---

## ⚡ See It In Action

### 🔌 TCP Client

```csharp
var tcpClient = new TcpClient(logger, "myserver.example.com", 4242);
tcpClient.DataReceived += data => Console.WriteLine($"📥 {Encoding.ASCII.GetString(data)}");

if (await tcpClient.Connect())
{
    await tcpClient.Send("Hello!");
}
```

### 🖥️ VNC Remote Desktop

```csharp
var vnc = new VncClient("192.168.1.50", 5900);
vnc.FramebufferUpdated += (_, e) => RenderRegion(e.Rectangle, e.Framebuffer);

await vnc.Connect();
await vnc.Authenticate("my-password");
await vnc.Initialize();
await vnc.StartUpdates();
```

### 🔍 IP Range Scan

```csharp
var scanner = new IPScanner(new IPScannerConfig
{
    IcmpPing = true,
    ResolveHostName = true,
    ResolveMacAddress = true,
    ResolveVendorFromMacAddress = true,
});

var results = await scanner.ScanRange(
    IPAddress.Parse("192.168.0.1"),
    IPAddress.Parse("192.168.0.254"),
    CancellationToken.None);
```

---

## 📚 Documentation

| Topic | Description |
|-------|-------------|
| [🔌 TCP Client & Server](docs/tcp.md) | Connections, TLS, reconnection, keep-alive, events |
| [📡 UDP Client & Server](docs/udp.md) | Datagram communication, hosted server |
| [🖥️ VNC Client](docs/vnc.md) | Remote desktop, encodings, pixel data, input forwarding |
| [🔍 IP Scanning & Helpers](docs/ip-scanning.md) | IP scanner, port scan, ping, DNS, ARP, MAC vendor lookup |
| [⏰ Wake-on-LAN](docs/wake-on-lan.md) | Magic packets, subnet broadcast, SecureOn password |
| [🕰️ NTP Time](docs/ntp.md) | Server time, clock offset, round-trip delay, time zones |
| [📡 Service Discovery](docs/discovery.md) | mDNS / DNS-SD and SSDP (UPnP), in the IP scanner too |
| [🧵 Events, Threads & Errors](docs/events.md) | Which thread raises events, handler exceptions, log event ids |
| [📈 Metrics & Tracing](docs/telemetry.md) | OpenTelemetry metrics and traces: instruments, tags, spans |
| [🧪 Showcase with Aspire](sample/Showcase/README.md) | TCP and UDP test servers and clients, with logs and metrics in the Aspire dashboard |

---

## 🔧 Requirements

- .NET 10.0+
- NuGet dependencies, pulled in automatically:
  - [Atc](https://www.nuget.org/packages/Atc): ATC base library
  - [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting): `IHostedService` for `TcpServer` and `UdpServer`
  - [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions): `ILogger`

---

## 🤝 How to contribute

[Contribution Guidelines](https://atc-net.github.io/introduction/about-atc#how-to-contribute)

[Coding Guidelines](https://atc-net.github.io/introduction/about-atc#coding-guidelines)