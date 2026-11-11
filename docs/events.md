# 🧵 Events, Threads & Errors

How the clients and servers raise their events, and what happens when an event handler fails.

---

## 🧵 Which thread raises the events

Events are raised on **background thread-pool threads**, not on the thread that called `Connect` or `StartAsync`, and never on a UI thread. In a WPF, WinForms or MAUI app, dispatch to the UI thread yourself before touching UI elements.

| Component | Raised from | Can handlers run at the same time? |
|-----------|-------------|------------------------------------|
| `TcpClient`, `UdpClient` | The client's receive loop | No: one message at a time |
| `TcpServer` | One receive task per connected client | **Yes**, for different clients |
| `UdpServer` | The server's receive loop | No: one datagram at a time |
| `VncClient` | The update loop | No: one server message at a time |
| `IPScanner` | Several IP addresses and ports are scanned in parallel | **Yes** |

Connection events (`Connected`, `Disconnected`, `ConnectionStateChanged`) are also raised from inside `Connect`, `Disconnect` and the reconnect logic.

### ⏱️ Keep handlers fast

The receive loop waits for your handler before it reads the next message. A slow handler delays everything after it, and for TCP the unread data builds up in the socket buffers. Do slow work elsewhere, for example by writing the data to a `Channel<T>` and processing it on another task.

---

## 💥 When a handler throws

Each subscriber is called on its own. If one throws:

- the exception is **logged** through the `ILogger` you passed in;
- the other subscribers are still called;
- the client or server **keeps running**: it keeps receiving and stays connected.

The same applies to the `On…` methods you can override in a subclass.

### ⚠️ `async` handlers

An `async` lambda used as an event handler runs as `async void`. Only the code before its first `await` runs inside the library's `try/catch`. An exception after the first `await` is not caught by the library and can end the process. Catch exceptions inside `async` handlers yourself:

```csharp
tcpServer.ClientDataReceived += async (_, e) =>
{
    try
    {
        await tcpServer.Send(e.ClientId, "ack", CancellationToken.None);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Reply failed");
    }
};
```

---

## 📋 Log event ids

Use these ids to find handler failures and receive errors in your logs.

| Component | Event id | Logged when |
|-----------|----------|-------------|
| `TcpClient` | 10017 | A `DataReceived` handler throws |
| `TcpClient` | 10018 | The receive loop hits an unexpected error (the client disconnects, and reconnects if enabled) |
| `TcpClient` | 10019 | A `Connected`, `Disconnected`, `ConnectionStateChanged` or `NoDataReceived` handler throws |
| `UdpClient` | 10212 | A `DataReceived` handler throws |
| `UdpClient` | 10213 | A `Connected`, `Disconnected` or `ConnectionStateChanged` handler throws |
| `TcpServer` | 10125 | A `DataReceived` or `ClientDataReceived` handler throws |
| `TcpServer` | 10126 | A client connection fails (the client is closed; the server keeps running) |
| `TcpServer` | 10127 | The listener stops unexpectedly (`IsRunning` becomes `false`) |
| `TcpServer` | 10128 | A `ClientConnected` or `ClientDisconnected` handler throws |
| `UdpServer` | 10323 | A `DataReceived` or `DatagramReceived` handler throws |
| `UdpServer` | 10324 | A receive fails with a socket error (the server keeps receiving) |
| `VncClient` | 10421 | Any `VncClient` event handler throws |