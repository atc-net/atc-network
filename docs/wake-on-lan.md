# ⏰ Wake-on-LAN

Wake a sleeping or powered-off device by sending it a Wake-on-LAN "magic packet".

---

## 🚀 Wake a Device

```csharp
await WakeOnLanHelper.SendMagicPacket("01-23-45-67-89-AB");
```

This sends the packet as a UDP broadcast to `255.255.255.255:9`, which reaches every device on the local network.

The MAC address can be written as:

| Form | Example |
|------|---------|
| Dashes | `01-23-45-67-89-AB` |
| Colons | `01:23:45:67:89:AB` |
| Plain | `0123456789AB` |
| Dotted (Cisco) | `0123.4567.89AB` |

Upper and lower case both work. Anything else throws an `ArgumentException`.

---

## 🎯 Choose Where to Send

Routers don't forward the global broadcast. To wake a device on another subnet, send to that subnet's broadcast address:

```csharp
await WakeOnLanHelper.SendMagicPacket(
    "01-23-45-67-89-AB",
    new IPEndPoint(IPAddress.Parse("192.168.1.255"), WakeOnLanHelper.DefaultPort));
```

Some devices listen on port 7 instead of port 9.

---

## 🔐 SecureOn Password

Some network cards only wake when the packet ends with a SecureOn password of 4 or 6 bytes:

```csharp
await WakeOnLanHelper.SendMagicPacket(
    "01-23-45-67-89-AB",
    secureOnPassword: [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC]);
```

---

## 🔍 Wake Devices Found by the Scanner

The [IP scanner](ip-scanning.md) finds MAC addresses, so you can keep them and wake the devices later:

```csharp
var results = await scanner.ScanRange(
    IPAddress.Parse("192.168.1.1"),
    IPAddress.Parse("192.168.1.254"),
    CancellationToken.None);

foreach (var result in results.CollectedResults.Where(x => x.MacAddress is not null))
{
    await WakeOnLanHelper.SendMagicPacket(result.MacAddress!);
}
```

ARP only knows devices that are switched on, so scan while they are awake.

---

## 📦 The Magic Packet

`CreateMagicPacket` returns the packet without sending it, for example to send it another way:

```csharp
byte[] packet = WakeOnLanHelper.CreateMagicPacket("01-23-45-67-89-AB");
```

The packet is 6 bytes of `0xFF` followed by the MAC address 16 times: 102 bytes, plus the SecureOn password when one is given.

---

## 📈 Telemetry

Each packet sent is recorded on `atc.network.bytes.sent` and `atc.network.messages.sent`, with the component tag `wake_on_lan`, and as a `WakeOnLan.Send` span. See [Metrics & Tracing](telemetry.md).

---

## ⚠️ Requirements on the Device

- Wake-on-LAN must be enabled in the BIOS or UEFI and in the network card's settings.
- The device must be connected by cable; most Wi-Fi cards can't wake a powered-off device.
- The device must still have standby power.