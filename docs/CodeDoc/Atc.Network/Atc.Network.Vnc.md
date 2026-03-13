<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Vnc

<br />

## IVncClient
This is a interface for `Atc.Network.Vnc.VncClient`.

>```csharp
>public interface IVncClient : IDisposable
>```

### Properties

#### Framebuffer
>```csharp
>Framebuffer
>```
><b>Summary:</b> Gets the framebuffer after initialization.
#### IPAddressOrHostname
>```csharp
>IPAddressOrHostname
>```
><b>Summary:</b> IPAddress or hostname for server connection.
#### IsConnected
>```csharp
>IsConnected
>```
><b>Summary:</b> Is client connected.
#### Port
>```csharp
>Port
>```
><b>Summary:</b> Port number for server connection.
#### ViewOnly
>```csharp
>ViewOnly
>```
><b>Summary:</b> Gets a value indicating whether the client is in view-only mode.
### Events

#### BellReceived
>```csharp
>BellReceived
>```
><b>Summary:</b> Event to raise when the server sends a bell notification.
#### Connected
>```csharp
>Connected
>```
><b>Summary:</b> Event to raise when connection is established.
#### ConnectionLost
>```csharp
>ConnectionLost
>```
><b>Summary:</b> Event to raise when the connection is lost unexpectedly.
#### ConnectionStateChanged
>```csharp
>ConnectionStateChanged
>```
><b>Summary:</b> Event to raise when connection state is changed.
#### Disconnected
>```csharp
>Disconnected
>```
><b>Summary:</b> Event to raise when connection is destroyed.
#### FramebufferUpdated
>```csharp
>FramebufferUpdated
>```
><b>Summary:</b> Event to raise when a framebuffer update is received.
#### ServerCutText
>```csharp
>ServerCutText
>```
><b>Summary:</b> Event to raise when the server sends clipboard text.
### Methods

#### Authenticate
>```csharp
>Task<bool> Authenticate(string password, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Authenticate with the VNC server using a password.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`password`&nbsp;&nbsp;-&nbsp;&nbsp;The VNC password.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Connect
>```csharp
>Task<bool> Connect(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Connect to the VNC server.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Disconnect
>```csharp
>Task Disconnect()
>```
><b>Summary:</b> Disconnect from the VNC server.
#### Initialize
>```csharp
>Task Initialize(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Initialize the VNC session after authentication.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### RequestFullScreenUpdate
>```csharp
>Task RequestFullScreenUpdate(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Request a full screen update.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### SendClientCutText
>```csharp
>Task SendClientCutText(string text, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send clipboard text to the server.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`text`&nbsp;&nbsp;-&nbsp;&nbsp;The clipboard text.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### SendKeyEvent
>```csharp
>Task SendKeyEvent(uint keysym, bool pressed, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send a key event to the server.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`keysym`&nbsp;&nbsp;-&nbsp;&nbsp;The X11 keysym value.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`pressed`&nbsp;&nbsp;-&nbsp;&nbsp;True if the key is pressed, false if released.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### SendPointerEvent
>```csharp
>Task SendPointerEvent(byte buttonMask, int x, int y, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send a pointer (mouse) event to the server.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`buttonMask`&nbsp;&nbsp;-&nbsp;&nbsp;The button state mask.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`x`&nbsp;&nbsp;-&nbsp;&nbsp;The X position.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`y`&nbsp;&nbsp;-&nbsp;&nbsp;The Y position.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### StartUpdates
>```csharp
>Task StartUpdates(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Start receiving framebuffer updates.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />

<br />

## IVncInputPolicy
Defines input handling policy for a VNC client. Controls whether keyboard and pointer events are sent to the server.

>```csharp
>public interface IVncInputPolicy
>```

### Properties

#### AllowClipboardTransfer
>```csharp
>AllowClipboardTransfer
>```
><b>Summary:</b> Determines whether clipboard text should be sent to the server.
#### AllowKeyboardInput
>```csharp
>AllowKeyboardInput
>```
><b>Summary:</b> Determines whether keyboard events should be forwarded to the server.
#### AllowPointerInput
>```csharp
>AllowPointerInput
>```
><b>Summary:</b> Determines whether pointer (mouse) events should be forwarded to the server.

<br />

## VncClient
The main VncClient - Handles VNC/RFB protocol communication.

>```csharp
>public class VncClient : IVncClient, IDisposable
>```

### Properties

#### Framebuffer
>```csharp
>Framebuffer
>```
><b>Summary:</b> Gets the framebuffer after initialization.
#### IPAddressOrHostname
>```csharp
>IPAddressOrHostname
>```
><b>Summary:</b> IPAddress or hostname for server connection.
#### IsConnected
>```csharp
>IsConnected
>```
><b>Summary:</b> Is client connected.
#### Port
>```csharp
>Port
>```
><b>Summary:</b> Port number for server connection.
#### ViewOnly
>```csharp
>ViewOnly
>```
><b>Summary:</b> Gets a value indicating whether the client is in view-only mode.
### Events

#### BellReceived
>```csharp
>BellReceived
>```
><b>Summary:</b> Event to raise when the server sends a bell notification.
#### Connected
>```csharp
>Connected
>```
><b>Summary:</b> Event to raise when connection is established.
#### ConnectionLost
>```csharp
>ConnectionLost
>```
><b>Summary:</b> Event to raise when the connection is lost unexpectedly.
#### ConnectionStateChanged
>```csharp
>ConnectionStateChanged
>```
><b>Summary:</b> Event to raise when connection state is changed.
#### Disconnected
>```csharp
>Disconnected
>```
><b>Summary:</b> Event to raise when connection is destroyed.
#### FramebufferUpdated
>```csharp
>FramebufferUpdated
>```
><b>Summary:</b> Event to raise when a framebuffer update is received.
#### ServerCutText
>```csharp
>ServerCutText
>```
><b>Summary:</b> Event to raise when the server sends clipboard text.
### Methods

#### Authenticate
>```csharp
>Task<bool> Authenticate(string password, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Authenticate with the VNC server using a password.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`password`&nbsp;&nbsp;-&nbsp;&nbsp;The VNC password.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Connect
>```csharp
>Task<bool> Connect(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Connect to the VNC server.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Disconnect
>```csharp
>Task Disconnect()
>```
><b>Summary:</b> Disconnect from the VNC server.
#### Dispose
>```csharp
>void Dispose()
>```
#### Initialize
>```csharp
>Task Initialize(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Initialize the VNC session after authentication.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### RequestFullScreenUpdate
>```csharp
>Task RequestFullScreenUpdate(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Request a full screen update.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### SendClientCutText
>```csharp
>Task SendClientCutText(string text, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send clipboard text to the server.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`text`&nbsp;&nbsp;-&nbsp;&nbsp;The clipboard text.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### SendKeyEvent
>```csharp
>Task SendKeyEvent(uint keysym, bool pressed, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send a key event to the server.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`keysym`&nbsp;&nbsp;-&nbsp;&nbsp;The X11 keysym value.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`pressed`&nbsp;&nbsp;-&nbsp;&nbsp;True if the key is pressed, false if released.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### SendPointerEvent
>```csharp
>Task SendPointerEvent(byte buttonMask, int x, int y, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send a pointer (mouse) event to the server.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`buttonMask`&nbsp;&nbsp;-&nbsp;&nbsp;The button state mask.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`x`&nbsp;&nbsp;-&nbsp;&nbsp;The X position.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`y`&nbsp;&nbsp;-&nbsp;&nbsp;The Y position.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### StartUpdates
>```csharp
>Task StartUpdates(CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Start receiving framebuffer updates.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />

<br />

## VncClientConfig
Base configurations for `Atc.Network.Vnc.VncClient`.

>```csharp
>public class VncClientConfig
>```

### Properties

#### BitsPerPixel
>```csharp
>BitsPerPixel
>```
><b>Summary:</b> Gets or sets the bits per pixel for the requested pixel format.
>
><b>Returns:</b> The bits per pixel. The default is 32.
#### ConnectTimeout
>```csharp
>ConnectTimeout
>```
><b>Summary:</b> Gets or sets the connect timeout value of the connection in milliseconds.
>
><b>Returns:</b> The connect time-out value, in milliseconds. The default is 10000 (10 sec).
#### Depth
>```csharp
>Depth
>```
><b>Summary:</b> Gets or sets the colour depth.
>
><b>Returns:</b> The colour depth. The default is 24.
#### Port
>```csharp
>Port
>```
><b>Summary:</b> Gets or sets the VNC server port.
>
><b>Returns:</b> The port number. The default is 5900.
#### ReceiveTimeout
>```csharp
>ReceiveTimeout
>```
><b>Summary:</b> Gets or sets the receive timeout value in milliseconds.
>
><b>Returns:</b> The receive time-out value, in milliseconds. The default is 30000 (30 sec).
#### SendTimeout
>```csharp
>SendTimeout
>```
><b>Summary:</b> Gets or sets the send timeout value in milliseconds.
>
><b>Returns:</b> The send time-out value, in milliseconds. The default is 30000 (30 sec).
#### SharedDesktop
>```csharp
>SharedDesktop
>```
><b>Summary:</b> Gets or sets a value indicating whether the desktop should be shared with other clients.
#### ViewOnly
>```csharp
>ViewOnly
>```
><b>Summary:</b> Gets or sets a value indicating whether the client is view-only (no input events sent).
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## VncClientKeepAliveConfig
KeepAlive configurations for `Atc.Network.Vnc.VncClient`.

>```csharp
>public class VncClientKeepAliveConfig
>```

### Properties

#### Enable
>```csharp
>Enable
>```
><b>Summary:</b> Keep alive enable/disable on the socket option `System.Net.Sockets.SocketOptionName.KeepAlive`.
#### Interval
>```csharp
>Interval
>```
><b>Summary:</b> Keep alive interval on the socket option `System.Net.Sockets.SocketOptionName.TcpKeepAliveInterval`.
#### RetryCount
>```csharp
>RetryCount
>```
><b>Summary:</b> Keep alive retry count on the socket option `System.Net.Sockets.SocketOptionName.TcpKeepAliveRetryCount`.
#### Time
>```csharp
>Time
>```
><b>Summary:</b> Keep alive time on the socket option `System.Net.Sockets.SocketOptionName.TcpKeepAliveTime`.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## VncClientReconnectConfig
Reconnect configurations for `Atc.Network.Vnc.VncClient`.

>```csharp
>public class VncClientReconnectConfig
>```

### Properties

#### Enable
>```csharp
>Enable
>```
><b>Summary:</b> Enable auto-reconnect on unexpected disconnect.
>
><b>Remarks:</b> Reconnect triggers when the connection is lost unexpectedly (e.g., server closes the socket). The full VNC sequence (TCP connect, RFB handshake, authenticate, initialize) is replayed.
#### RetryInterval
>```csharp
>RetryInterval
>```
><b>Summary:</b> Gets or sets the retry interval in milliseconds.
>
><b>Returns:</b> The retry interval value, in milliseconds. The default is 2000 (2 sec).
>
><b>Remarks:</b> If the `Atc.Network.Vnc.VncClientReconnectConfig.RetryInterval` and the `Atc.Network.Vnc.VncClientReconnectConfig.RetryMaxAttempts` is set to the defaults as a calculation example: 2sec * 1800 attempts, then the `Atc.Network.Vnc.VncClient` will try auto-reconnect within 1 hour, before it gives up on auto reconnection.
#### RetryMaxAttempts
>```csharp
>RetryMaxAttempts
>```
><b>Summary:</b> Gets or sets the retry max attempts.
>
><b>Returns:</b> The retry max attempts value.
>
><b>Remarks:</b> If the `Atc.Network.Vnc.VncClientReconnectConfig.RetryInterval` and the `Atc.Network.Vnc.VncClientReconnectConfig.RetryMaxAttempts` is set to the defaults as a calculation example: 2sec * 1800 attempts, then the `Atc.Network.Vnc.VncClient` will try auto-reconnect within 1 hour, before it gives up on auto reconnection.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## VncConstants
Constants for the VNC/RFB protocol.

>```csharp
>public static class VncConstants
>```

### Static Fields

#### ChallengeLength
>```csharp
>int ChallengeLength
>```
><b>Summary:</b> VNC authentication challenge length in bytes.
#### DefaultBitsPerPixel
>```csharp
>int DefaultBitsPerPixel
>```
><b>Summary:</b> Default bits per pixel.
#### DefaultBufferSize
>```csharp
>int DefaultBufferSize
>```
><b>Summary:</b> Default buffer size in bytes (32 KB).
#### DefaultConnectTimeout
>```csharp
>int DefaultConnectTimeout
>```
><b>Summary:</b> Default connect timeout in milliseconds (10 seconds).
#### DefaultDepth
>```csharp
>int DefaultDepth
>```
><b>Summary:</b> Default colour depth.
#### DefaultPort
>```csharp
>int DefaultPort
>```
><b>Summary:</b> Default VNC server port.
#### DefaultReceiveTimeout
>```csharp
>int DefaultReceiveTimeout
>```
><b>Summary:</b> Default receive timeout in milliseconds (30 seconds).
#### DefaultReconnectRetryInterval
>```csharp
>int DefaultReconnectRetryInterval
>```
><b>Summary:</b> Default reconnect retry interval in milliseconds (2 seconds).
#### DefaultReconnectRetryMaxAttempts
>```csharp
>int DefaultReconnectRetryMaxAttempts
>```
><b>Summary:</b> Default reconnect retry max attempts (1800 × 2s = 1 hour).
#### DefaultSendTimeout
>```csharp
>int DefaultSendTimeout
>```
><b>Summary:</b> Default send timeout in milliseconds (30 seconds).
#### GracePeriodTimeout
>```csharp
>int GracePeriodTimeout
>```
><b>Summary:</b> Grace period timeout in milliseconds.
#### RfbVersion
>```csharp
>string RfbVersion
>```
><b>Summary:</b> RFB protocol version string sent by client (3.8).

<br />

## VncDefaultInputPolicy
Default input policy that allows all input types.

>```csharp
>public class VncDefaultInputPolicy : IVncInputPolicy
>```

### Properties

#### AllowClipboardTransfer
>```csharp
>AllowClipboardTransfer
>```
#### AllowKeyboardInput
>```csharp
>AllowKeyboardInput
>```
#### AllowPointerInput
>```csharp
>AllowPointerInput
>```

<br />

## VncFramebuffer
Represents the VNC server's framebuffer, holding pixel data and format information.

>```csharp
>public class VncFramebuffer
>```

### Properties

#### DesktopName
>```csharp
>DesktopName
>```
><b>Summary:</b> Gets the name of the remote desktop.
#### Height
>```csharp
>Height
>```
><b>Summary:</b> Gets the height of the framebuffer in pixels.
#### Item
>```csharp
>Item
>```
><b>Summary:</b> Gets or sets the pixel value at the specified coordinates.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`x`&nbsp;&nbsp;-&nbsp;&nbsp;The x coordinate.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`y`&nbsp;&nbsp;-&nbsp;&nbsp;The y coordinate.<br />
>
><b>Returns:</b> The 32-bit ARGB pixel value.
#### PixelData
>```csharp
>PixelData
>```
><b>Summary:</b> Gets the pixel data as an array of 32-bit ARGB values.
#### PixelFormat
>```csharp
>PixelFormat
>```
><b>Summary:</b> Gets the pixel format of the framebuffer.
#### Width
>```csharp
>Width
>```
><b>Summary:</b> Gets the width of the framebuffer in pixels.
### Methods

#### FillRectangle
>```csharp
>void FillRectangle(VncRectangle rectangle, int pixel)
>```
><b>Summary:</b> Fills a rectangle region with the specified pixel value.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`rectangle`&nbsp;&nbsp;-&nbsp;&nbsp;The rectangle region to fill.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`pixel`&nbsp;&nbsp;-&nbsp;&nbsp;The pixel value to fill with.<br />
#### ToString
>```csharp
>string ToString()
>```

<br />

## VncFramebufferUpdateEventArgs
Event arguments for a framebuffer update.

>```csharp
>public class VncFramebufferUpdateEventArgs : EventArgs
>```

### Properties

#### Framebuffer
>```csharp
>Framebuffer
>```
><b>Summary:</b> Gets the framebuffer containing the updated pixel data.
#### Rectangle
>```csharp
>Rectangle
>```
><b>Summary:</b> Gets the updated rectangle region.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## VncPixelFormat
Describes the pixel format used by the VNC framebuffer.

>```csharp
>public class VncPixelFormat
>```

### Static Methods

#### Create
>```csharp
>VncPixelFormat Create(int bitsPerPixel, int depth)
>```
><b>Summary:</b> Creates a pixel format for the specified bits per pixel and depth.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`bitsPerPixel`&nbsp;&nbsp;-&nbsp;&nbsp;Bits per pixel (8, 16, or 32).<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`depth`&nbsp;&nbsp;-&nbsp;&nbsp;Colour depth.<br />
>
><b>Returns:</b> A configured `Atc.Network.Vnc.VncPixelFormat`.
### Properties

#### BigEndian
>```csharp
>BigEndian
>```
><b>Summary:</b> Gets or sets a value indicating whether the pixel values are in big-endian byte order.
#### BitsPerPixel
>```csharp
>BitsPerPixel
>```
><b>Summary:</b> Gets or sets the number of bits per pixel (8, 16, or 32).
#### BlueMax
>```csharp
>BlueMax
>```
><b>Summary:</b> Gets or sets the maximum blue value.
#### BlueShift
>```csharp
>BlueShift
>```
><b>Summary:</b> Gets or sets the blue colour shift.
#### Depth
>```csharp
>Depth
>```
><b>Summary:</b> Gets or sets the colour depth.
#### GreenMax
>```csharp
>GreenMax
>```
><b>Summary:</b> Gets or sets the maximum green value.
#### GreenShift
>```csharp
>GreenShift
>```
><b>Summary:</b> Gets or sets the green colour shift.
#### RedMax
>```csharp
>RedMax
>```
><b>Summary:</b> Gets or sets the maximum red value.
#### RedShift
>```csharp
>RedShift
>```
><b>Summary:</b> Gets or sets the red colour shift.
#### TrueColour
>```csharp
>TrueColour
>```
><b>Summary:</b> Gets or sets a value indicating whether true colour is used (as opposed to colour map).
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## VncRectangle
Represents a rectangle region in the VNC framebuffer.

>```csharp
>public struct VncRectangle : IEquatable<VncRectangle>
>```

### Properties

#### Height
>```csharp
>Height
>```
#### Width
>```csharp
>Width
>```
#### X
>```csharp
>X
>```
#### Y
>```csharp
>Y
>```
### Methods

#### Deconstruct
>```csharp
>void Deconstruct(out int X, out int Y, out int Width, out int Height)
>```
#### Equals
>```csharp
>bool Equals(object obj)
>```
#### Equals
>```csharp
>bool Equals(VncRectangle other)
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

## VncViewInputPolicy
View-only input policy that blocks all input.

>```csharp
>public class VncViewInputPolicy : IVncInputPolicy
>```

### Properties

#### AllowClipboardTransfer
>```csharp
>AllowClipboardTransfer
>```
#### AllowKeyboardInput
>```csharp
>AllowKeyboardInput
>```
#### AllowPointerInput
>```csharp
>AllowPointerInput
>```
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
