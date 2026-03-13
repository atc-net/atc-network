# 📡 UDP Client & Server

Atc.Network provides event-driven UDP communication with the same clean patterns as the TCP module.

## ✨ Features

- 📡 Event-driven architecture matching the TCP client pattern
- 🧩 Interface-based design (`IUdpClient`, `IUdpServer`) for DI and testing
- 🏗️ `IHostedService` integration for the server
- 🔒 Thread-safe with `SemaphoreSlim` synchronization
- 📝 Structured logging via `ILogger`

---

## 🖥️ UdpClient

### Quick Start

```csharp
using Atc.Network.Udp;

var udpClient = new UdpClient(
    logger,
    new IPEndPoint(IPAddress.Parse("192.168.1.100"), 9000),
    new UdpClientConfig
    {
        SendTimeout = 5000,
        ReceiveTimeout = 5000,
    });

// 🎯 Wire up events
udpClient.Connected += () => Console.WriteLine("✅ Connected!");
udpClient.DataReceived += data =>
{
    Console.WriteLine($"📥 Received {data.Length} bytes");
};

// 🚀 Connect and send
if (await udpClient.Connect())
{
    await udpClient.Send("Hello UDP!");
}

await udpClient.Disconnect();
udpClient.Dispose();
```

### ⚙️ Configuration

| Property | Default | Description |
|----------|---------|-------------|
| `ConnectTimeout` | 10,000 ms | Connection timeout |
| `SendTimeout` | 600,000 ms | Send operation timeout |
| `ReceiveTimeout` | 600,000 ms | Receive operation timeout |
| `SendBufferSize` | 8,192 bytes | Send buffer size |
| `ReceiveBufferSize` | 8,192 bytes | Receive buffer size |
| `DefaultEncoding` | ASCII | Default text encoding |
| `TerminationType` | None | Line termination style |

### 📡 Events

| Event | Type | Description |
|-------|------|-------------|
| `Connected` | `Action` | Fired when connection is established |
| `Disconnected` | `Action` | Fired when connection is destroyed |
| `ConnectionStateChanged` | `EventHandler<ConnectionStateEventArgs>` | Fired on any state transition |
| `DataReceived` | `Action<byte[]>` | Fired when data arrives |

---

## 🏗️ UdpServer

The `UdpServer` implements `IHostedService` and supports sending to specific endpoints.

```csharp
var udpServer = new UdpServer(logger, new IPEndPoint(IPAddress.Any, 9000));
udpServer.DataReceived += data =>
{
    Console.WriteLine($"📥 Server received {data.Length} bytes");
};

await udpServer.StartAsync(CancellationToken.None);

// 📤 Send to a specific client
await udpServer.Send(
    clientEndPoint,
    "Response from server",
    CancellationToken.None);
```

---

## 📂 Sample

A complete working sample is available at [`sample/Atc.Network.Console.Udp/Program.cs`](../sample/Atc.Network.Console.Udp/Program.cs).