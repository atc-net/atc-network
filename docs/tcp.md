# 🔌 TCP Client & Server

Atc.Network provides a robust, event-driven TCP client and server with automatic reconnection, keep-alive support, and configurable timeouts.

## ✨ Features

- 🔄 Automatic reconnection with configurable retry policy
- 💓 TCP keep-alive support
- 🔐 Optional TLS for client and server (see [TLS](#-tls))
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
| `SslOptions` | `null` | TLS options; `null` means plain TCP (see [TLS](#-tls)) |
| `MessageFraming` | None | How received data is split into messages (see [What `DataReceived` delivers](#-what-datareceived-delivers)) |
| `MaxMessageSize` | 16 MB | The largest message that `MessageFraming` accepts |

### 📥 What `DataReceived` delivers

By default, `TcpClient.DataReceived` is raised once per read from the socket, with the bytes of that read. TCP is a stream, so one read can hold part of a message, a whole message or several messages.

Set `MessageFraming` to get one `DataReceived` per message instead:

| `MessageFraming` | Receiving | Sending |
|------------------|-----------|---------|
| `None` (default) | One event per read | Appends `TerminationType` |
| `TerminationType` | Collects reads until `TerminationType`, then one event per message, including the terminator. Several messages in one read give several events | Appends `TerminationType` |
| `LengthPrefix` | Each message starts with its length as a 4-byte big-endian integer. One event per message, without the length | Adds the length. `TerminationType` is not used |

```csharp
var tcpClient = new TcpClient(
    logger,
    "device.example.com",
    4000,
    new TcpClientConfig
    {
        TerminationType = TerminationType.LineFeed,
        MessageFraming = TcpMessageFraming.TerminationType,
    });
```

### 🌊 Reading as an async stream

`ReadAllAsync` returns the same data as `DataReceived` (one item per event, after framing) as an `IAsyncEnumerable<ReadOnlyMemory<byte>>`:

```csharp
await foreach (var message in tcpClient.ReadAllAsync(cancellationToken))
{
    Console.WriteLine(Encoding.UTF8.GetString(message.Span));
}
```

- Exceptions in your loop stay in your code; there is no event handler that can throw into the client.
- **Back-pressure:** each enumeration buffers up to 256 items. When it falls behind, the client stops reading from the socket until there is room, so TCP flow control slows down the sender instead of the client using more memory. `DataReceived` handlers are then called later, too.
- The stream continues across reconnects and ends when the `cancellationToken` is cancelled (with an `OperationCanceledException`) or the client is disposed.
- Data received before the enumeration starts is not included. Several enumerations at the same time each get every item.

`UdpClient.ReadAllAsync` works the same way, with one item per datagram.

A message longer than `MaxMessageSize` (16 MB by default) is reported through `ErrorOccurred` as a `Receive` error with an `InvalidDataException`, and the connection is closed (and reconnected, if reconnect is on). Bytes of a message that was cut off by a dropped connection are discarded on reconnect.

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

## 🔐 TLS

TLS is off by default. Set `SslOptions` on the client or server config to encrypt the connection with TLS (`SslStream`). The options are the standard .NET types, so certificate validation, client certificates and protocol versions are set the usual way.

### Client

```csharp
var tcpClient = new TcpClient(
    logger,
    "device.example.com",
    8443,
    new TcpClientConfig
    {
        SslOptions = new SslClientAuthenticationOptions(),
    });
```

- The server certificate is validated by the operating system: it must be trusted and match the host name.
- When `TargetHost` is empty, the hostname or IP address you connect to is used. Your options object is not changed, so several clients can share it.
- The TLS handshake runs inside `Connect` and must finish within `ConnectTimeout`. If it fails, `Connect` returns `false` and raises `ConnectionStateChanged` with `ConnectionFailed`; `Connected` is not raised.
- Every reconnect does a new handshake.
- For a self-signed or private certificate, set `RemoteCertificateValidationCallback` and check the certificate yourself, for example by its thumbprint. Do not return `true` for every certificate: that turns off the protection TLS gives.
- For a client certificate, set `ClientCertificates`.

### Server

```csharp
var tcpServer = new TcpServer(
    logger,
    IPAddress.Any,
    8443,
    new TcpServerConfig
    {
        SslOptions = new SslServerAuthenticationOptions
        {
            ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile("server.pfx", password),
        },
    });
```

- Each client does its TLS handshake in its own task, so a slow client does not hold up the others.
- A client counts as connected only after its handshake succeeds. A client that fails the handshake, or doesn't finish it within 10 seconds, is closed without `ClientConnected` and is logged (event 10129).
- To require client certificates, set `ClientCertificateRequired = true`, and set `RemoteCertificateValidationCallback` if the client certificates are not trusted by the operating system.

---

## 📂 Sample

A complete working sample is available at [`sample/Atc.Network.Console.Tcp/Program.cs`](../sample/Atc.Network.Console.Tcp/Program.cs).

For a real-world setup, with a TCP server and client running as hosted services under Aspire and their logs in the Aspire dashboard, see the [Showcase](../sample/Showcase/README.md).