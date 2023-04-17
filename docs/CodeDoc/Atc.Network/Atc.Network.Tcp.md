<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Tcp

<br />

## ITcpClient
This is a interface for `Atc.Network.Tcp.TcpClient`.

>```csharp
>public interface ITcpClient
>```

### Properties

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
### Events

#### Connected
>```csharp
>Connected
>```
><b>Summary:</b> Event to raise when connection is established.
#### ConnectionStateChanged
>```csharp
>ConnectionStateChanged
>```
><b>Summary:</b> Event to raise when connection state is changed.
#### DataReceived
>```csharp
>DataReceived
>```
><b>Summary:</b> Event to raise when data has become available from the server.
#### Disconnected
>```csharp
>Disconnected
>```
><b>Summary:</b> Event to raise when connection is destroyed.
#### NoDataReceived
>```csharp
>NoDataReceived
>```
><b>Summary:</b> Event to raise when no data received.
### Methods

#### Connect
>```csharp
>Task<bool> Connect(CancellationToken cancellationToken = null)
>```
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Disconnect
>```csharp
>Task Disconnect()
>```
#### Send
>```csharp
>Task Send(string data, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> Data will be encoded as client-config default encoding.
#### Send
>```csharp
>Task Send(Encoding encoding, string data, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> Data will be encoded as client-config default encoding.
#### Send
>```csharp
>Task Send(byte[] data, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> Data will be encoded as client-config default encoding.
#### Send
>```csharp
>Task Send(byte[] data, TerminationType terminationType, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> Data will be encoded as client-config default encoding.

<br />

## TcpClient
The main TcpClient - Handles call execution.

>```csharp
>public class TcpClient : IDisposable
>```

### Properties

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
### Events

#### Connected
>```csharp
>Connected
>```
><b>Summary:</b> Event to raise when connection is established.
#### ConnectionStateChanged
>```csharp
>ConnectionStateChanged
>```
><b>Summary:</b> Event to raise when connection state is changed.
#### DataReceived
>```csharp
>DataReceived
>```
><b>Summary:</b> Event to raise when data has become available from the server.
#### Disconnected
>```csharp
>Disconnected
>```
><b>Summary:</b> Event to raise when connection is destroyed.
#### NoDataReceived
>```csharp
>NoDataReceived
>```
><b>Summary:</b> Event to raise when no data received.
### Methods

#### Connect
>```csharp
>Task<bool> Connect(CancellationToken cancellationToken = null)
>```
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Disconnect
>```csharp
>Task Disconnect()
>```
#### Dispose
>```csharp
>void Dispose()
>```
#### Send
>```csharp
>Task Send(string data, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> Data will be encoded as client-config default encoding.
#### Send
>```csharp
>Task Send(Encoding encoding, string data, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> Data will be encoded as client-config default encoding.
#### Send
>```csharp
>Task Send(byte[] data, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> Data will be encoded as client-config default encoding.
#### Send
>```csharp
>Task Send(byte[] data, TerminationType terminationType, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> Data will be encoded as client-config default encoding.

<br />

## TcpClientConfig
Base configurations for `Atc.Network.Tcp.TcpClient`.

>```csharp
>public class TcpClientConfig
>```

### Properties

#### ConnectTimeout
>```csharp
>ConnectTimeout
>```
><b>Summary:</b> Gets or sets the connect timeout value of the connection in milliseconds.
>
><b>Returns:</b> The connect time-out value, in milliseconds. The default is 10000 (10 sec);
#### DefaultEncoding
>```csharp
>DefaultEncoding
>```
><b>Summary:</b> Gets or sets the default encoding.
#### ReceiveBufferSize
>```csharp
>ReceiveBufferSize
>```
><b>Summary:</b> Gets or sets the size of the receive buffer in bytes.
>
><b>Returns:</b> The size of the receive buffer, in bytes. The default value is 8192 bytes.
#### ReceiveTimeout
>```csharp
>ReceiveTimeout
>```
><b>Summary:</b> Gets or sets the receive timeout value in milliseconds.
>
><b>Returns:</b> The receive time-out value, in milliseconds. The default is 0;
#### SendBufferSize
>```csharp
>SendBufferSize
>```
><b>Summary:</b> Gets or sets the size of the send buffer in bytes.
>
><b>Returns:</b> The size of the send buffer, in bytes. The default value is 8192 bytes.
#### SendTimeout
>```csharp
>SendTimeout
>```
><b>Summary:</b> Gets or sets the send timeout value in milliseconds.
>
><b>Returns:</b> The send time-out value, in milliseconds. The default is 0;
#### TerminationType
>```csharp
>TerminationType
>```
><b>Summary:</b> Gets or sets the TerminationType.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## TcpClientKeepAliveConfig
KeepAlive configurations for `Atc.Network.Tcp.TcpClient`.

>```csharp
>public class TcpClientKeepAliveConfig
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

## TcpClientReconnectConfig
Reconnect configurations for `Atc.Network.Tcp.TcpClient`.

>```csharp
>public class TcpClientReconnectConfig
>```

### Properties

#### Enable
>```csharp
>Enable
>```
><b>Summary:</b> Enable auto-reconnect then disconnect.
>
><b>Remarks:</b> Disconnect happens 'on sender socket closed'.
#### RetryInterval
>```csharp
>RetryInterval
>```
><b>Summary:</b> Gets or sets the retry interval in milliseconds.
>
><b>Returns:</b> The retry interval value, in milliseconds. The default is 1000 (1 sec).
>
><b>Remarks:</b> If the `Atc.Network.Tcp.TcpClientReconnectConfig.RetryInterval` and the `Atc.Network.Tcp.TcpClientReconnectConfig.RetryMaxAttempts` is set to the defaults as a calculation example: 1sec * 3600 attempts, then the `Atc.Network.Tcp.TcpClient` will try auto-reconnect within 1hour, before it gives up on auto reconnection.
#### RetryMaxAttempts
>```csharp
>RetryMaxAttempts
>```
><b>Summary:</b> Gets or sets the retry max attempts.
>
><b>Returns:</b> The retry max attempts value.
>
><b>Remarks:</b> If the `Atc.Network.Tcp.TcpClientReconnectConfig.RetryInterval` and the `Atc.Network.Tcp.TcpClientReconnectConfig.RetryMaxAttempts` is set to the defaults as a calculation example: 1sec * 3600 attempts, then the `Atc.Network.Tcp.TcpClient` will try auto-reconnect within 1hour, before it gives up on auto reconnection.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## TcpConstants
This class contains default constant for `Atc.Network.Tcp.TcpClient` and `Atc.Network.Tcp.TcpClientReconnectConfig`.

>```csharp
>public static class TcpConstants
>```

### Static Fields

#### DefaultBufferSize
>```csharp
>int DefaultBufferSize
>```
><b>Summary:</b> The send/receive buffer value, in bytes (8 Kb).
#### DefaultConnectTimeout
>```csharp
>int DefaultConnectTimeout
>```
><b>Summary:</b> The connect time-out value, in milliseconds (10 sec).
#### DefaultReconnectRetryInterval
>```csharp
>int DefaultReconnectRetryInterval
>```
><b>Summary:</b> The reconnect retry interval value, in milliseconds (1 sec).
#### DefaultReconnectRetryMaxAttempts
>```csharp
>int DefaultReconnectRetryMaxAttempts
>```
><b>Summary:</b> The reconnect retry max attempts value.
#### DefaultSendReceiveTimeout
>```csharp
>int DefaultSendReceiveTimeout
>```
><b>Summary:</b> The send/receive time-out value, in milliseconds.
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
