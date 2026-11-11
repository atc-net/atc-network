# 📹 ONVIF Cameras

Find IP cameras on the network, and read their model and stream addresses, with ONVIF, the standard most IP cameras support.

| Step | API | Needs credentials |
|------|-----|-------------------|
| Find the cameras | `OnvifDiscovery.Probe()` (see [Service Discovery](discovery.md#-onvif-ws-discovery)) | No |
| Read the camera's clock | `OnvifClient.GetSystemDateAndTime()` | No |
| Read manufacturer and model | `OnvifClient.GetDeviceInformation()` | Yes |
| Read the RTSP stream URLs | `OnvifClient.GetStreams()` | Yes |

---

## 🚀 From Discovery to Stream URL

```csharp
await foreach (var camera in OnvifDiscovery.Probe())
{
    if (!camera.IsNetworkVideoTransmitter || camera.DeviceServiceUrl is null)
    {
        continue;
    }

    var client = new OnvifClient(camera.DeviceServiceUrl, new NetworkCredential("onvif-user", password));

    var information = await client.GetDeviceInformation();
    Console.WriteLine($"📹 {information.Manufacturer} {information.Model} at {camera.IPAddress}");

    foreach (var stream in await client.GetStreams())
    {
        Console.WriteLine($"   {stream.Profile.Name}: {stream.StreamUri} ({stream.Profile.Width}x{stream.Profile.Height} {stream.Profile.Encoding})");
    }
}
// 📹 TP-Link C210 at 192.168.1.132
//    mainStream: rtsp://192.168.1.132:554/stream1 (2304x1296 H264)
//    minorStream: rtsp://192.168.1.132:554/stream2 (640x360 H264)
```

The stream URLs carry no credentials; add the user name and password when you play them.

---

## 🔌 OnvifClient

```csharp
var client = new OnvifClient(
    new Uri("http://192.168.1.132:2020/onvif/device_service"),
    new NetworkCredential("onvif-user", password));
```

| Method | Returns |
|--------|---------|
| `GetSystemDateAndTime()` | The camera's clock (UTC); also sets `ClockOffset`. Needs no credentials, so it also tells whether ONVIF answers at the URL |
| `GetDeviceInformation()` | Manufacturer, model, firmware version, serial number, hardware id |
| `GetCapabilities()` | The media, PTZ, imaging and events service URLs |
| `GetProfiles()` | The media profiles: token, name, encoding, resolution, frame rate |
| `GetStreamUri(profileToken)` | The RTSP URL of one profile, as the camera reports it |
| `GetStreams()` | Every profile with its RTSP URL |
| `GetNetworkInterfaces()` | The network interfaces, with MAC address, IPv4 addresses and whether each is Wi-Fi |

- **Clock:** before the first request with credentials, the client reads the camera's clock, so the security token's time matches the camera. Cameras with a wrong clock reject a token with the local time.
- **Stream hosts:** cameras often report an old or internal address in their stream URLs. `GetStreams()` replaces the host with the one you reached the camera at; pass `replaceHostWithDeviceHost: false` to keep it.
- **HTTP client:** the client uses a shared `HttpClient` by default; pass your own to the constructor (you keep owning it). `Timeout` (5 seconds by default) applies to each request.

---

## ⚠️ Errors

| Exception | When |
|-----------|------|
| `OnvifException` with `IsNotAuthorized` | Wrong user or password (`ter:NotAuthorized`, often HTTP 400), or HTTP 401 |
| `OnvifException` | Another SOAP fault (`Code`, `Subcode`, `Reason`), an HTTP error, or an unreadable answer |
| `TimeoutException` | The camera didn't answer within `Timeout` |
| `OperationCanceledException` | The `CancellationToken` was cancelled |

The password is never logged and never part of an exception.

---

## 🛠️ Camera Setup

- **ONVIF must be on.** Many cameras ship with it off. Hikvision: *Configuration → Network → Advanced → Integration Protocol*. TP-Link Tapo: *Camera Settings → Advanced Settings → Camera Account* (the camera account is the ONVIF user).
- **Port:** the device service URL from discovery has the right port; Tapo cameras use 2020, most others 80.

---

## 🚧 Limits

- **Media service 1 only.** Nearly every camera offers it; Media 2 (`ver20`) isn't used.
- **WS-Security only.** Cameras that only accept HTTP digest authentication answer HTTP 401, reported as an `OnvifException` that says so.
- No PTZ, events or recording; only what is needed to find a camera and its streams.

---

## 📈 Telemetry

Each request is a span named `OnvifClient.<Operation>` (for example `OnvifClient.GetProfiles`) with the component tag `onvif_client`, and is counted on the message and byte counters. See [Metrics & Tracing](telemetry.md).