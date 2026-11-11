<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Onvif

<br />

## OnvifCapabilities
The service URLs of an ONVIF device (GetCapabilities). A device may serve several services on one URL.

>```csharp
>public class OnvifCapabilities : IEquatable<OnvifCapabilities>
>```

### Properties

#### EventsServiceUrl
>```csharp
>EventsServiceUrl
>```
><b>Summary:</b> The events service.
#### ImagingServiceUrl
>```csharp
>ImagingServiceUrl
>```
><b>Summary:</b> The imaging service.
#### MediaServiceUrl
>```csharp
>MediaServiceUrl
>```
><b>Summary:</b> The media service: profiles and stream URIs.
#### PtzServiceUrl
>```csharp
>PtzServiceUrl
>```
><b>Summary:</b> The pan/tilt/zoom service, when the device has one.
### Methods

#### <Clone>$
>```csharp
>OnvifCapabilities <Clone>$()
>```
#### Deconstruct
>```csharp
>void Deconstruct(out Uri MediaServiceUrl, out Uri PtzServiceUrl, out Uri ImagingServiceUrl, out Uri EventsServiceUrl)
>```
#### Equals
>```csharp
>bool Equals(object obj)
>```
#### Equals
>```csharp
>bool Equals(OnvifCapabilities other)
>```
#### GetHashCode
>```csharp
>int GetHashCode()
>```
#### ToString
>```csharp
>string ToString()
>```

<br />

## OnvifClient
A small ONVIF client: device information, service URLs, media profiles, stream URIs and network interfaces.
><b>Remarks:</b> Requests are SOAP 1.2 over HTTP, authenticated with a WS-Security UsernameToken (PasswordDigest). Before the first authenticated request the client reads the device's clock (which needs no credentials), so the token's time matches the device: a camera with a wrong clock rejects a token with the local time.<br>Limits: Media service version 1 only (which nearly all cameras offer), and no HTTP digest authentication; a device that only accepts HTTP digest answers 401, which is reported as an `Atc.Network.Onvif.OnvifException`. The password is never logged or included in exceptions.

>```csharp
>public class OnvifClient
>```

### Properties

#### ClockOffset
>```csharp
>ClockOffset
>```
><b>Summary:</b> Gets how far the device's clock is ahead of the local clock, as read by `Atc.Network.Onvif.OnvifClient.GetSystemDateAndTime(System.Threading.CancellationToken)`.
#### DeviceServiceUrl
>```csharp
>DeviceServiceUrl
>```
><b>Summary:</b> Gets the device service URL.
#### Timeout
>```csharp
>Timeout
>```
><b>Summary:</b> Gets the time to wait for each response. The default is 5 seconds.
### Methods

#### GetCapabilities
>```csharp
>Task<OnvifCapabilities> GetCapabilities(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Reads the URLs of the device's services.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The service URLs.
#### GetDeviceInformation
>```csharp
>Task<OnvifDeviceInformation> GetDeviceInformation(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Reads the manufacturer, model, firmware version and serial number.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The device information.
#### GetNetworkInterfaces
>```csharp
>Task<IReadOnlyList<OnvifNetworkInterface>> GetNetworkInterfaces(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Reads the device's network interfaces, including whether each is wired or Wi-Fi.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The network interfaces.
#### GetProfiles
>```csharp
>Task<IReadOnlyList<OnvifMediaProfile>> GetProfiles(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Reads the media profiles: one per stream setup, such as the main and the sub stream.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The profiles.
#### GetStreamUri
>```csharp
>Task<Uri> GetStreamUri(string profileToken, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Reads the RTSP URI of a media profile.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`profileToken`&nbsp;&nbsp;-&nbsp;&nbsp;The profile token ().<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The RTSP URI as the device reports it, without credentials.
#### GetStreams
>```csharp
>Task<IReadOnlyList<OnvifStreamInfo>> GetStreams(bool replaceHostWithDeviceHost = True, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Reads every media profile with its RTSP URI.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`replaceHostWithDeviceHost`&nbsp;&nbsp;-&nbsp;&nbsp;
            Whether to replace the host in each stream URI with the host of . Cameras often report
            an old or internal address in their stream URIs. The default is .
            <br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The profiles with their stream URIs, without credentials.
#### GetStreams
>```csharp
>Task<IReadOnlyList<OnvifStreamInfo>> GetStreams(CancellationToken cancellationToken)
>```
><b>Summary:</b> Reads every media profile with its RTSP URI.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`replaceHostWithDeviceHost`&nbsp;&nbsp;-&nbsp;&nbsp;
            Whether to replace the host in each stream URI with the host of . Cameras often report
            an old or internal address in their stream URIs. The default is .
            <br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The profiles with their stream URIs, without credentials.
#### GetSystemDateAndTime
>```csharp
>Task<DateTime> GetSystemDateAndTime(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Reads the device's clock (UTC). Needs no credentials, so it also tells whether ONVIF answers at the URL.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellation token.<br />
>
><b>Returns:</b> The device's time, in UTC.

<br />

## OnvifDeviceInformation
What an ONVIF device says about itself (GetDeviceInformation).

>```csharp
>public class OnvifDeviceInformation : IEquatable<OnvifDeviceInformation>
>```

### Properties

#### FirmwareVersion
>```csharp
>FirmwareVersion
>```
><b>Summary:</b> The firmware version.
#### HardwareId
>```csharp
>HardwareId
>```
><b>Summary:</b> The hardware id.
#### Manufacturer
>```csharp
>Manufacturer
>```
><b>Summary:</b> The manufacturer, such as "TP-Link" or "HIKVISION".
#### Model
>```csharp
>Model
>```
><b>Summary:</b> The model, such as "C210" or "DS-2CD2183G0-IU".
#### SerialNumber
>```csharp
>SerialNumber
>```
><b>Summary:</b> The serial number.
### Methods

#### <Clone>$
>```csharp
>OnvifDeviceInformation <Clone>$()
>```
#### Deconstruct
>```csharp
>void Deconstruct(out string Manufacturer, out string Model, out string FirmwareVersion, out string SerialNumber, out string HardwareId)
>```
#### Equals
>```csharp
>bool Equals(object obj)
>```
#### Equals
>```csharp
>bool Equals(OnvifDeviceInformation other)
>```
#### GetHashCode
>```csharp
>int GetHashCode()
>```
#### ToString
>```csharp
>string ToString()
>```

<br />

## OnvifException
An ONVIF request failed: the device answered with a SOAP fault, an HTTP error, or a response that could not be read.

>```csharp
>public class OnvifException : Exception, ISerializable
>```

### Properties

#### Code
>```csharp
>Code
>```
><b>Summary:</b> Gets the SOAP fault code, such as "SOAP-ENV:Sender".
#### IsNotAuthorized
>```csharp
>IsNotAuthorized
>```
><b>Summary:</b> Gets a value indicating whether the device rejected the credentials.
#### Reason
>```csharp
>Reason
>```
><b>Summary:</b> Gets the fault reason, such as "Authority failure".
#### StatusCode
>```csharp
>StatusCode
>```
><b>Summary:</b> Gets the HTTP status code, when the device answered.
#### Subcode
>```csharp
>Subcode
>```
><b>Summary:</b> Gets the SOAP fault subcode, such as "ter:NotAuthorized".

<br />

## OnvifMediaProfile
A media profile of an ONVIF device: one video stream setup, such as the main or the sub stream.

>```csharp
>public class OnvifMediaProfile : IEquatable<OnvifMediaProfile>
>```

### Properties

#### Encoding
>```csharp
>Encoding
>```
><b>Summary:</b> The video encoding, such as "H264" or "H265".
#### FrameRate
>```csharp
>FrameRate
>```
><b>Summary:</b> The frame rate limit, or 0 when unknown.
#### Height
>```csharp
>Height
>```
><b>Summary:</b> The video height in pixels, or 0 when unknown.
#### Name
>```csharp
>Name
>```
><b>Summary:</b> The profile name, such as "mainStream".
#### Token
>```csharp
>Token
>```
><b>Summary:</b> The profile token, which `GetStreamUri` takes.
#### Width
>```csharp
>Width
>```
><b>Summary:</b> The video width in pixels, or 0 when unknown.
### Methods

#### <Clone>$
>```csharp
>OnvifMediaProfile <Clone>$()
>```
#### Deconstruct
>```csharp
>void Deconstruct(out string Token, out string Name, out string Encoding, out int Width, out int Height, out int FrameRate)
>```
#### Equals
>```csharp
>bool Equals(object obj)
>```
#### Equals
>```csharp
>bool Equals(OnvifMediaProfile other)
>```
#### GetHashCode
>```csharp
>int GetHashCode()
>```
#### ToString
>```csharp
>string ToString()
>```

<br />

## OnvifNetworkInterface
A network interface of an ONVIF device (GetNetworkInterfaces).

>```csharp
>public class OnvifNetworkInterface : IEquatable<OnvifNetworkInterface>
>```

### Properties

#### Enabled
>```csharp
>Enabled
>```
><b>Summary:</b> Whether the interface is enabled.
#### IPv4Addresses
>```csharp
>IPv4Addresses
>```
><b>Summary:</b> The IPv4 addresses, manual or from DHCP.
#### IsWireless
>```csharp
>IsWireless
>```
><b>Summary:</b> Whether it is a Wi-Fi interface (IEEE 802.11).
#### MacAddress
>```csharp
>MacAddress
>```
><b>Summary:</b> The MAC address, as the device wrote it.
#### Name
>```csharp
>Name
>```
><b>Summary:</b> The interface name, such as "eth0".
#### Token
>```csharp
>Token
>```
><b>Summary:</b> The interface token.
### Methods

#### <Clone>$
>```csharp
>OnvifNetworkInterface <Clone>$()
>```
#### Deconstruct
>```csharp
>void Deconstruct(out string Token, out string Name, out bool Enabled, out string MacAddress, out IReadOnlyList`1 IPv4Addresses, out bool IsWireless)
>```
#### Equals
>```csharp
>bool Equals(object obj)
>```
#### Equals
>```csharp
>bool Equals(OnvifNetworkInterface other)
>```
#### GetHashCode
>```csharp
>int GetHashCode()
>```
#### ToString
>```csharp
>string ToString()
>```

<br />

## OnvifStreamInfo
A media profile with its RTSP stream URI.

>```csharp
>public class OnvifStreamInfo : IEquatable<OnvifStreamInfo>
>```

### Properties

#### Profile
>```csharp
>Profile
>```
><b>Summary:</b> The media profile.
#### StreamUri
>```csharp
>StreamUri
>```
><b>Summary:</b> The RTSP URI, without credentials; add the user name and password to play it.
### Methods

#### <Clone>$
>```csharp
>OnvifStreamInfo <Clone>$()
>```
#### Deconstruct
>```csharp
>void Deconstruct(out OnvifMediaProfile Profile, out Uri StreamUri)
>```
#### Equals
>```csharp
>bool Equals(object obj)
>```
#### Equals
>```csharp
>bool Equals(OnvifStreamInfo other)
>```
#### GetHashCode
>```csharp
>int GetHashCode()
>```
#### ToString
>```csharp
>string ToString()
>```
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
