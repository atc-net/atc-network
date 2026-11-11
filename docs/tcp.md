# 🔌 TCP Client & Server

Atc.Network provides a robust, event-driven TCP client and server with automatic reconnection, keep-alive support, and configurable timeouts.

## ✨ Features

- 🔄 Automatic reconnection with configurable retry policy
- 💓 TCP keep-alive support
- 📡 Event-driven architecture (Connected, Disconnected, DataReceived, etc.)
- 🔒 Thread-safe with `SemaphoreSlim` synchronization
- 🧩 Interface-based design (`ITcpClient`, `ITcpServer`) for easy DI and testing
- 📝 Structured logging via `ILogger` and `[LoggerMessage]` source generators

---

## 🖥️ TcpClient

### Quick Start

```csharp
using Atc.Network.Tcp;

var tcpClient = new TcpClient(
    logger,
    "myserver.example.com",
    4242,
    new TcpClientConfig
    {
        ConnectTimeout = 5000,
        TerminationType = TerminationType.LineFeed,
    });

// 🎯 Wire up events
tcpClient.Connected += () => Console.WriteLine("✅ Connected!");
tcpClient.Disconnected += () => Console.WriteLine("❌ Disconnected!");
tcpClient.DataReceived += data =>
{
    var text = Encoding.ASCII.GetString(data);
    Console.WriteLine($"📥 Received: {text}");
};

// 🚀 Connect and send data
if (await tcpClient.Connect())
{
    await tcpClient.Send("Hello, server!");
}

// 👋 Disconnect when done
await tcpClient.Disconnect();
tcpClient.Dispose();
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

### 📥 What `DataReceived` delivers

`TcpClient.DataReceived` is raised once per read from the socket, with the bytes of that read. TCP is a stream, so one read can hold part of a message, a whole message or several messages. `TerminationType` is not used to split received data on the client. If your protocol has message boundaries, collect the bytes and split them yourself.

`TcpServer` is different: it collects reads until the data contains its `TerminationType`, then raises `DataReceived` once for the whole message.

See [Events, Threads & Errors](events.md) for which thread raises the events and what happens when a handler throws.

### 🔄 Reconnection

```csharp
var tcpClient = new TcpClient(
    logger,
    "myserver.example.com",
    4242,
    clientConfig: new TcpClientConfig(),
    reconnectConfig: new TcpClientReconnectConfig
    {
        Enable = true,
        RetryInterval = 2000,       // 2 seconds between retries
        RetryMaxAttempts = 100,     // Max 100 attempts
    });
```

### 💓 Keep-Alive

```csharp
var tcpClient = new TcpClient(
    logger,
    "myserver.example.com",
    4242,
    keepAliveConfig: new TcpClientKeepAliveConfig
    {
        Enable = true,
        Interval = 2,       // Probe interval in seconds
        Time = 2,           // Idle time before probing
        RetryCount = 3,     // Probes before declaring dead
    });
```

### 📡 Events

| Event | Type | Description |
|-------|------|-------------|
| `Connected` | `Action` | Fired when connection is established |
| `Disconnected` | `Action` | Fired when connection is destroyed |
| `ConnectionStateChanged` | `EventHandler<ConnectionStateEventArgs>` | Fired on any state transition |
| `DataReceived` | `Action<byte[]>` | Fired when data arrives from server |
| `NoDataReceived` | `Action` | Fired when a read cycle returns empty |

---

## 🏗️ TcpServer

The `TcpServer` implements `IHostedService`, making it easy to integrate with .NET's hosting model.

```csharp
var tcpServer = new TcpServer(logger, IPAddress.Any, 8080);
tcpServer.DataReceived += data =>
{
    Console.WriteLine($"📥 Server received {data.Length} bytes");
};

await tcpServer.StartAsync(CancellationToken.None);
```

`StartAsync` throws a `SocketException` if the listener can't start, for example because the port is in use.

### 👥 Several clients

The server serves every connected client at the same time. Each client gets an id when it connects. Use it to tell clients apart and to reply to one of them:

```csharp
tcpServer.ClientConnected += (_, e) => Console.WriteLine($"✅ {e.ClientId} from {e.RemoteEndPoint}");
tcpServer.ClientDisconnected += (_, e) => Console.WriteLine($"❌ {e.ClientId}");

tcpServer.ClientDataReceived += async (_, e) =>
{
    var text = Encoding.ASCII.GetString(e.Data.Span);
    await tcpServer.Send(e.ClientId, $"echo: {text}", CancellationToken.None);
};
```

- `Send` returns `false` if the client is no longer connected.
- `StopAsync` stops accepting clients and closes the connected ones.
- A handler that throws is logged and does not stop the server.
- An `async` handler runs as `async void`: an exception after its first `await` is not caught by the server and can end the process. Catch exceptions inside `async` handlers yourself.

---

## 📂 Sample

A complete working sample is available at [`sample/Atc.Network.Console.Tcp/Program.cs`](../sample/Atc.Network.Console.Tcp/Program.cs).