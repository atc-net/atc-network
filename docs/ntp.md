# 🕰️ NTP Time

Get the time from an NTP server, and how far the local clock is from it, with SNTP (RFC 4330).

---

## 🚀 Get the Time

```csharp
var result = await NtpHelper.GetTime();

Console.WriteLine($"🕰️ Server time (UTC): {result.UtcTime:O}");
Console.WriteLine($"⏱️ Local clock is off by {result.Offset.TotalMilliseconds:N0} ms");
Console.WriteLine($"📶 Round trip: {result.RoundTripDelay.TotalMilliseconds:N0} ms, stratum {result.Stratum}");
```

Without arguments, `GetTime` asks `pool.ntp.org`, which picks a nearby public server, and waits up to 5 seconds.

---

## 📋 The Result

| Property | Meaning |
|----------|---------|
| `UtcTime` | The server's time (UTC) when the reply arrived |
| `Offset` | How far the server's clock is ahead of the local clock; negative when the local clock is ahead |
| `RoundTripDelay` | Time on the network, without the time the server needed to answer |
| `Stratum` | The server's distance from a reference clock: 1 has its own clock (GPS, atomic), 2 syncs with a stratum 1 server, and so on |
| `Server` | The address and port that answered |

The offset and delay come from the four NTP timestamps: when the request was sent and received, and when the reply was sent and received. The offset is accurate when the network takes about as long in both directions.

To get the server's time later, add the offset to the local clock:

```csharp
var serverNow = DateTime.UtcNow + result.Offset;
```

---

## 🎯 Choose a Server

```csharp
// A host name or an IP address, on port 123
var result = await NtpHelper.GetTime("time.cloudflare.com");

// A server on the LAN, such as a router or a domain controller - no internet needed
var lanResult = await NtpHelper.GetTime(new IPEndPoint(IPAddress.Parse("192.168.1.1"), NtpHelper.DefaultPort));

// A shorter timeout
var quick = await NtpHelper.GetTime("pool.ntp.org", TimeSpan.FromSeconds(1));
```

A host name with both IPv4 and IPv6 addresses is asked over IPv4. IPv6 servers work when given as an `IPEndPoint`.

---

## 🌍 Time Zones

NTP serves UTC. Convert it with `TimeZoneInfo`:

```csharp
var berlin = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
var berlinTime = TimeZoneInfo.ConvertTimeFromUtc(result.UtcTime, berlin);
```

This replaces `InternetWorldTimeHelper`, which asks the WorldTimeAPI web service and returns `null` when that fails. `InternetWorldTimeHelper` still works, but `NtpHelper` is recommended for new code.

---

## ⚠️ Errors

`GetTime` throws instead of returning `null`, so you can tell why it failed:

| Exception | When |
|-----------|------|
| `TimeoutException` | The server didn't answer in time |
| `InvalidDataException` | The server refused the request ("kiss-o'-death", for example `RATE` when asked too often), or its own clock isn't synchronized |
| `SocketException` | The host name couldn't be resolved, or the server's port is closed |
| `OperationCanceledException` | The `CancellationToken` was cancelled |

Replies that don't echo the request's transmit timestamp are ignored, so an old or spoofed reply can't be taken for the answer.

Public servers limit how often a client may ask. Don't call `GetTime` more than about once a minute per server.

---

## 📈 Telemetry

Each call records `atc.network.bytes.sent`, `atc.network.bytes.received` and the message counters with the component tag `ntp_client`, and an `Ntp.GetTime` span. See [Metrics & Tracing](telemetry.md).