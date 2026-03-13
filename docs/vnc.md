# 🖥️ VNC Client

Atc.Network includes a cross-platform VNC (RFB protocol) client for remote desktop access — no external dependencies required.

## ✨ Features

- 🔐 VNC authentication (DES challenge-response) and no-auth mode
- 🎨 Multiple encodings: Raw, CopyRect, RRE, CoRRE, Hextile, ZRLE
- 🖼️ Configurable pixel formats: 8-bit, 16-bit, 32-bit
- ⌨️ Keyboard and mouse input forwarding
- 📋 Clipboard sharing (server ↔ client)
- 🔔 Bell notification support
- 👁️ View-only mode
- 🐧 Fully cross-platform (Windows, Linux, macOS) — no `System.Drawing` dependency
- 🔄 Auto-reconnect with configurable retry policy
- 💓 TCP keep-alive for early dead-connection detection
- 🧩 Interface-based design (`IVncClient`) for DI and testing
- 📝 Structured logging via `ILogger`

---

## 🚀 Quick Start

```csharp
using Atc.Network.Vnc;

// 🧩 Without DI — logger-less overload for quick prototyping
var vncClient = new VncClient("192.168.1.50", 5900);

// 🧩 With DI / ILogger
var vncClient = new VncClient(
    logger,
    "192.168.1.50",
    5900,
    new VncClientConfig
    {
        BitsPerPixel = 32,
        Depth = 24,
        SharedDesktop = true,
    });

// 🎯 Wire up events
vncClient.Connected += () => Console.WriteLine("✅ Connected!");
vncClient.Disconnected += () => Console.WriteLine("❌ Disconnected!");
vncClient.ConnectionLost += () => Console.WriteLine("⚠️ Connection lost!");
vncClient.FramebufferUpdated += (_, e) =>
    Console.WriteLine($"🖼️ Update: {e.Rectangle}");
vncClient.BellReceived += () => Console.WriteLine("🔔 Bell!");
vncClient.ServerCutText += text =>
    Console.WriteLine($"📋 Clipboard: {text}");

// 🚀 Connect → Authenticate → Initialize → Start
var connected = await vncClient.Connect();
if (!connected) return;

// ⚠️ IMPORTANT: Always call Authenticate — even without a password!
// This method handles the full RFB security handshake, not just password auth.
// Skipping it will leave unread bytes in the stream and cause Initialize() to fail.
var authenticated = await vncClient.Authenticate("my-password");
if (!authenticated) return;

await vncClient.Initialize();

var fb = vncClient.Framebuffer!;
Console.WriteLine($"🖥️ Desktop: {fb.DesktopName} ({fb.Width}x{fb.Height})");

await vncClient.StartUpdates();

// ... later
await vncClient.Disconnect();
vncClient.Dispose();
```

> 💡 **No password?** Still call `Authenticate(string.Empty)` — it negotiates `SecurityType.None` with the server.

---

## ⚙️ Configuration

| Property | Default | Description |
|----------|---------|-------------|
| `ConnectTimeout` | 10,000 ms | Connection timeout |
| `SendTimeout` | 30,000 ms | Send operation timeout |
| `ReceiveTimeout` | 30,000 ms | Receive operation timeout |
| `BitsPerPixel` | 32 | Pixel format (8, 16, or 32) |
| `Depth` | 24 | Colour depth |
| `ViewOnly` | false | Disable all input forwarding |
| `SharedDesktop` | true | Allow other clients to connect simultaneously |
| `Port` | 5900 | VNC server port |

---

## 🔄 Auto-Reconnect

When the connection drops unexpectedly, the VNC client can automatically retry the full VNC sequence (TCP connect → RFB handshake → authenticate → initialize → start updates).

```csharp
var vncClient = new VncClient(
    logger,
    "192.168.1.50",
    5900,
    clientConfig: new VncClientConfig(),
    reconnectConfig: new VncClientReconnectConfig
    {
        Enable = true,           // Enabled by default
        RetryInterval = 2_000,   // 2 seconds between attempts (default)
        RetryMaxAttempts = 1800, // 1800 × 2s = 1 hour (default)
    });
```

| Property | Default | Description |
|----------|---------|-------------|
| `Enable` | true | Enable auto-reconnect on unexpected disconnect |
| `RetryInterval` | 2,000 ms | Time between reconnect attempts |
| `RetryMaxAttempts` | 1800 | Max attempts before giving up (1800 × 2s = 1 hour) |

> 💡 Reconnect requires a stored password — `Authenticate()` must have been called before the connection dropped. On explicit `Disconnect()`, auto-reconnect is disabled.

> 💡 Monitor reconnection via `ConnectionStateChanged` — you'll see `Reconnecting`, `Reconnected`, or `ReconnectionFailed` states.

---

## 💓 TCP Keep-Alive

Socket-level keep-alive probes detect dead connections earlier than waiting for a read timeout:

```csharp
var vncClient = new VncClient(
    logger,
    "192.168.1.50",
    5900,
    keepAliveConfig: new VncClientKeepAliveConfig
    {
        Enable = true,    // Enabled by default
        Time = 2,         // Start probes after 2s of idle (default)
        Interval = 2,     // Probe every 2s (default)
        RetryCount = 3,   // Give up after 3 failed probes (default)
    });
```

| Property | Default | Description |
|----------|---------|-------------|
| `Enable` | true | Enable TCP keep-alive |
| `Time` | 2 | Seconds of idle before first keep-alive probe |
| `Interval` | 2 | Seconds between keep-alive probes |
| `RetryCount` | 3 | Failed probes before connection is considered dead |

---

## 📡 Events

| Event | Type | Description |
|-------|------|-------------|
| `Connected` | `Action` | Connection established |
| `Disconnected` | `Action` | Connection destroyed |
| `ConnectionStateChanged` | `EventHandler<ConnectionStateEventArgs>` | Any state transition |
| `FramebufferUpdated` | `EventHandler<VncFramebufferUpdateEventArgs>` | Pixel data updated (fires per rectangle) |
| `BellReceived` | `Action` | Server bell notification |
| `ServerCutText` | `Action<string>` | Server clipboard text |
| `ConnectionLost` | `Action` | Unexpected disconnection |

> 💡 **`FramebufferUpdated` fires once per updated rectangle**, not once per frame. A single server update can contain multiple rectangles. For rendering, update each region as it arrives and let your UI framework coalesce the visual updates.

---

## ⌨️ Input Forwarding

```csharp
// 🖱️ Send mouse events
await vncClient.SendPointerEvent(
    buttonMask: 0x01,   // Left button pressed
    x: 100,
    y: 200);

// ⌨️ Send key events (X11 keysyms)
await vncClient.SendKeyEvent(keysym: 0xFF0D, pressed: true);   // Enter key down
await vncClient.SendKeyEvent(keysym: 0xFF0D, pressed: false);  // Enter key up

// 📋 Send clipboard text
await vncClient.SendClientCutText("Hello from client!");
```

> 💡 When `ViewOnly = true`, input methods silently no-op via `IVncInputPolicy`.

---

## 👁️ View-Only Mode

Set `ViewOnly = true` in the config to observe the remote desktop without sending any input:

```csharp
var vncClient = new VncClient(
    logger,
    "192.168.1.50",
    5900,
    new VncClientConfig { ViewOnly = true });
```

---

## 🖼️ Accessing Pixel Data

The `VncFramebuffer` exposes pixel data as a flat `int[]` array of 32-bit ARGB values. Each pixel is at index `(y * Width) + x`.

```csharp
var fb = vncClient.Framebuffer!;

// Access individual pixels
int pixel = fb[x, y];

// Access the raw buffer (for rendering to a UI surface)
int[] pixelData = fb.PixelData;   // Length = Width * Height
```

### 🎨 WPF Rendering Example

The pixel format maps directly to WPF `PixelFormats.Bgra32` via `Marshal.Copy` to a `WriteableBitmap`:

```csharp
vncClient.FramebufferUpdated += (_, e) =>
{
    var fb = e.Framebuffer;
    var rect = e.Rectangle;

    Dispatcher.BeginInvoke(() =>
    {
        writeableBitmap.Lock();

        for (var y = rect.Y; y < rect.Y + rect.Height && y < fb.Height; y++)
        {
            var destOffset = (y * writeableBitmap.BackBufferStride) + (rect.X * 4);
            var srcOffset = (y * fb.Width) + rect.X;
            var pixelCount = Math.Min(rect.Width, fb.Width - rect.X);

            Marshal.Copy(fb.PixelData, srcOffset, writeableBitmap.BackBuffer + destOffset, pixelCount);
        }

        writeableBitmap.AddDirtyRect(new Int32Rect(rect.X, rect.Y, rect.Width, rect.Height));
        writeableBitmap.Unlock();
    });
};
```

---

## 🎨 Supported Encodings

| Encoding | ID | Description |
|----------|----|-------------|
| Raw | 0 | Uncompressed pixel data |
| CopyRect | 1 | Copy from another framebuffer region |
| RRE | 2 | Rise-and-Run-length Encoding |
| CoRRE | 4 | Compact RRE |
| Hextile | 5 | 16×16 tile-based encoding |
| ZRLE | 16 | Zlib Run-Length Encoding (best compression) |

---

## 📂 Sample

A complete working sample is available at [`sample/Atc.Network.Console.Vnc/Program.cs`](../sample/Atc.Network.Console.Vnc/Program.cs).