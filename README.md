[![NuGet Version](https://img.shields.io/nuget/v/atc.network.svg?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/atc.network)

# 🌐 Atc.Network

A .NET 8 library for network communication, scanning, and remote desktop — batteries included, zero external dependencies.

---

## 🤔 Why Atc.Network?

| | |
|---|---|
| 🔌 **TCP & UDP** | Event-driven clients and servers with auto-reconnect, keep-alive, and configurable timeouts |
| 🖥️ **VNC Client** | Cross-platform RFB protocol client — Raw, CopyRect, RRE, CoRRE, Hextile, ZRLE encodings |
| 🔍 **IP Scanning** | Scan ranges or CIDR blocks with ICMP ping, hostname/MAC resolution, vendor lookup, and port scanning |
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
| [🔌 TCP Client & Server](docs/tcp.md) | Connections, reconnection, keep-alive, events |
| [📡 UDP Client & Server](docs/udp.md) | Datagram communication, hosted server |
| [🖥️ VNC Client](docs/vnc.md) | Remote desktop, encodings, pixel data, input forwarding |
| [🔍 IP Scanning & Helpers](docs/ip-scanning.md) | IP scanner, port scan, ping, DNS, ARP, MAC vendor lookup |

---

## 🔧 Requirements

- .NET 8.0+
- No additional NuGet dependencies beyond the .NET SDK

---

## 🤝 How to contribute

[Contribution Guidelines](https://atc-net.github.io/introduction/about-atc#how-to-contribute)

[Coding Guidelines](https://atc-net.github.io/introduction/about-atc#coding-guidelines)