# 🧪 Showcase: TCP and UDP with Aspire

A real-world setup of Atc.Network: a TCP server and a UDP server, each with a client that talks to it, run together by [Aspire](https://aspire.dev). The Aspire dashboard shows their logs, metrics and traces in one place.

| App | What it does |
|-----|--------------|
| `Showcase.TcpTestServer` | `TcpServer` as a hosted service on port 9100. Answers every line with `ack: <line>`. |
| `Showcase.TcpTestClient` | `TcpClient` that sends `ping <n>` every 2 seconds and reconnects when the server goes away. It uses `MessageFraming` to get one message per reply line, and reads the replies with `await foreach` over `ReadAllAsync` instead of the `DataReceived` event. |
| `Showcase.UdpTestServer` | `UdpServer` as a hosted service on port 9101, over IPv6 (`::1`). Answers every datagram with `ack: <text>`, sent back to the sender. |
| `Showcase.UdpTestClient` | `UdpClient` that sends `ping <n>` every 2 seconds. |
| `Showcase.ServiceDefaults` | Shared OpenTelemetry setup: logs, metrics and traces, exported to the dashboard. |
| `Showcase.Aspire` | The Aspire AppHost that runs the four apps. |

## ▶️ Run it

```bash
dotnet run --project sample/Showcase/Showcase.Aspire
```

The dashboard opens at <http://localhost:15210>. Use the login link printed in the console the first time.

Each app also runs on its own with `dotnet run`. It then uses the default ports and exports nothing.

## 👀 What to look for

- **Structured logs:** the showcase apps log what they send and receive. The library logs under its own categories, such as `Atc.Network.Tcp.TcpClient` and `Atc.Network.Tcp.TcpServer`, with the event ids listed in [Events, Threads & Errors](../../docs/events.md). The `Atc.Network` categories log at `Debug` (see each app's `appsettings.json`).
- **Reconnect:** restart `tcp-test-server` from the dashboard. The client logs `Reconnecting` and then `Reconnected to 127.0.0.1:9100`, and the ping/ack loop goes on.
- **UDP has no connection:** stop `udp-test-server`. The client keeps sending, and the pings are lost until the server is back. Each lost ping comes back as a "port unreachable", which the client reports through its `ErrorOccurred` event; the app logs it as a `Receive` error.
- **Errors you can program against:** every app subscribes to `ErrorOccurred` and logs what it reports: the kind (`EventHandler`, `Receive`, `ClientConnection`, `Listener`), the event whose handler threw, and the exception.
- **Metrics:** open *Metrics* and pick a resource. The `Atc.Network` meter shows active connections, bytes and messages sent and received, reconnects and handler errors, tagged with the component, address and port. Restart `tcp-test-server` to see `atc.network.reconnects`. See [Metrics & Tracing](../../docs/telemetry.md).
- **Traces:** open *Traces* to see a `TcpClient.Send`, `TcpServer.Send`, `UdpClient.Send` or `UdpServer.Send` span for every ping and reply, and a `TcpClient.Connect` span for every connect.

## 🔧 How it is wired

- The servers listen on fixed ports without the Aspire proxy, because the proxy doesn't handle UDP: TCP on 127.0.0.1:9100 (IPv4), UDP on [::1]:9101 (IPv6). The AppHost passes each port to its server in `TCP_PORT` / `UDP_PORT`, and the endpoint to its client with `WithReference`.
- Each server is registered as a singleton and then as a hosted service. An event-handler service is added first, so it subscribes to the server's events before the server starts.
- The reply handlers send on another task and catch their own exceptions. See [When a handler throws](../../docs/events.md#-when-a-handler-throws).
- The showcase has its own solution, `Showcase.slnx`, so the Aspire packages are not part of the library's build.
