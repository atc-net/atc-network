# 📈 Metrics & Tracing

Atc.Network emits metrics with `System.Diagnostics.Metrics` and traces with `System.Diagnostics.ActivitySource`, so any OpenTelemetry setup can collect them. Nothing is recorded until something listens.

---

## ⚙️ Turn it on

Add the meter and the activity source by name. The names are in `NetworkTelemetry`:

```csharp
builder.Services
    .AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter(NetworkTelemetry.MeterName))
    .WithTracing(tracing => tracing.AddSource(NetworkTelemetry.ActivitySourceName))
    .UseOtlpExporter();
```

Both are named `Atc.Network`. To see them in the Aspire dashboard, run the [Showcase](../sample/Showcase/README.md).

---

## 📊 Metrics

| Instrument | Type | Unit | What it counts |
|------------|------|------|----------------|
| `atc.network.connections.active` | UpDownCounter | `{connection}` | Connections that are open now. A server counts each connected client |
| `atc.network.connection.failures` | Counter | `{connection}` | Connect attempts that failed, and server-side TLS handshakes that failed |
| `atc.network.reconnects` | Counter | `{attempt}` | Reconnect attempts, tagged `atc.network.outcome` = `success` or `failure` |
| `atc.network.bytes.sent` | Counter | `By` | Bytes sent |
| `atc.network.bytes.received` | Counter | `By` | Bytes received |
| `atc.network.messages.sent` | Counter | `{message}` | Send calls that succeeded |
| `atc.network.messages.received` | Counter | `{message}` | `DataReceived` events: a TCP client read (or message, with `MessageFraming`), a TCP server message, a UDP datagram |
| `atc.network.handler.errors` | Counter | `{error}` | Event handlers that threw, tagged `atc.network.event` with the event name |

Which components record what:

| Component | Connections | Failures | Reconnects | Bytes & messages | Handler errors |
|-----------|:-----------:|:--------:|:----------:|:----------------:|:--------------:|
| `TcpClient` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `TcpServer` | ✅ (per client) | ✅ (TLS) | – | ✅ | ✅ |
| `UdpClient` | ✅ | ✅ | – | ✅ | ✅ |
| `UdpServer` | – | – | – | ✅ | ✅ |
| `VncClient` | ✅ | ✅ | ✅ | – | ✅ |
| `WakeOnLanHelper` | – | – | – | ✅ (sent) | – |

### 🏷️ Tags

Every measurement and span has these tags:

| Tag | Value |
|-----|-------|
| `atc.network.component` | `tcp_client`, `tcp_server`, `udp_client`, `udp_server`, `vnc_client` or `wake_on_lan` |
| `server.address` | The server a client connects to, the address a server listens on, or where a Wake-on-LAN packet is sent |
| `server.port` | The server's port |

Client endpoints of a server are not tags, so the number of time series stays small however many clients connect.

---

## 🔭 Traces

| Span | Kind | When |
|------|------|------|
| `TcpClient.Connect` | Client | `Connect`, including the TLS handshake. Status `Error` when it fails |
| `TcpClient.Send` | Client | Each `Send` |
| `TcpServer.Send` | Internal | Each `Send` to a client |
| `UdpClient.Connect` | Client | `Connect` |
| `UdpClient.Send` | Client | Each `Send` |
| `UdpServer.Send` | Internal | Each `Send` |
| `VncClient.Connect` | Client | `Connect`. Status `Error` when it fails |
| `WakeOnLan.Send` | Producer | Each `SendMagicPacket`. Status `Error` when it fails |

A span is a child of the caller's current activity, for example the request that called `Send`. The receive loops run outside any span, so the logs and spans of your `DataReceived` handlers are not attached to the connect span that started the loop.
