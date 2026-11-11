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
| `TerminationType` | None | Appended to data you send, if it is not already there |

### 📡 Events

| Event | Type | Description |
|-------|------|-------------|
| `Connected` | `Action` | Fired when connection is established |
| `Disconnected` | `Action` | Fired when connection is destroyed |
| `ConnectionStateChanged` | `EventHandler<ConnectionStateEventArgs>` | Fired on any state transition |
| `DataReceived` | `Action<byte[]>` | Fired once per received datagram, with its bytes |

See [Events, Threads & Errors](events.md) for which thread raises the events and what happens when a handler throws.

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

To reply to whoever sent a datagram, use `DatagramReceived`. It carries the data and the sender's endpoint:

```csharp
udpServer.DatagramReceived += async (_, e) =>
{
    Console.WriteLine($"📥 {e.Data.Length} bytes from {e.RemoteEndPoint}");
    await udpServer.Send(e.RemoteEndPoint, "ack", CancellationToken.None);
};
```

A handler that throws is logged and does not stop the server. An `async` handler runs as `async void`, so an exception after its first `await` is not caught by the server and can end the process. Catch exceptions inside `async` handlers yourself.

### ⚙️ Server Configuration

`UdpServerConfig` adds two automatic replies to the settings above. Both are off by default.

| Property | Default | Description |
|----------|---------|-------------|
| `RespondToPing` | false | Answer a datagram that starts with `ping` with `pong` |
| `EchoOnReceivedData` | false | Send every other datagram back to the sender as `echo: <data>` |

> ⚠️ An echo reply is larger than the datagram it answers. Don't enable `EchoOnReceivedData` on a server that is reachable from untrusted networks, because it can be abused to reflect traffic.

---

## 📂 Sample

A complete working sample is available at [`sample/Atc.Network.Console.Udp/Program.cs`](../sample/Atc.Network.Console.Udp/Program.cs).

For a real-world setup, with a UDP server and client running as hosted services under Aspire and their logs in the Aspire dashboard, see the [Showcase](../sample/Showcase/README.md).