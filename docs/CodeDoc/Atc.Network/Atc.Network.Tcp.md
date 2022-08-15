<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Tcp

<br />

## TcpClient
The main TcpClient - Handles call execution.

>```csharp
>public class TcpClient : IDisposable
>```

### Properties

#### IsConnected
>```csharp
>IsConnected
>```
><b>Summary:</b> Is client connected
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
>Task Send(byte[] data, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> TerminationType is resolved from TcpClientConfig.
#### Send
>```csharp
>Task Send(byte[] data, TcpTerminationType terminationType, CancellationToken cancellationToken = null)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
>
><b>Remarks:</b> TerminationType is resolved from TcpClientConfig.
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
><b>Remarks:</b> TerminationType is resolved from TcpClientConfig.
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
><b>Remarks:</b> TerminationType is resolved from TcpClientConfig.

<br />

## TcpClientConfig

>```csharp
>public class TcpClientConfig
>```

### Properties

#### ConnectTimeout
>```csharp
>ConnectTimeout
>```
><b>Summary:</b> Gets or sets the send timeout value of the connection in milliseconds.
>
><b>Returns:</b> The connect time-out value, in milliseconds. The default is 10000 (10 sec);
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
><b>Summary:</b> Gets or sets the receive timeout value of the connection in milliseconds.
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
><b>Summary:</b> Gets or sets the send timeout value of the connection in milliseconds.
>
><b>Returns:</b> The send time-out value, in milliseconds. The default is 0;
#### TerminationType
>```csharp
>TerminationType
>```
><b>Summary:</b> The TerminationType.

<br />

## TcpClientKeepAliveConfig
TcpClient KeepAlive Config

>```csharp
>public class TcpClientKeepAliveConfig
>```

### Properties

#### KeepAliveInterval
>```csharp
>KeepAliveInterval
>```
><b>Summary:</b> KeepAliveInterval
#### KeepAliveRetryCount
>```csharp
>KeepAliveRetryCount
>```
><b>Summary:</b> KeepAliveRetryCount
#### KeepAliveTime
>```csharp
>KeepAliveTime
>```
><b>Summary:</b> KeepAliveTime
#### ReconnectOnSenderSocketClosed
>```csharp
>ReconnectOnSenderSocketClosed
>```
><b>Summary:</b> ReconnectOnSenderSocketClosed

<br />

## TcpConstants

>```csharp
>public static class TcpConstants
>```

### Static Fields

#### DefaultBufferSize
>```csharp
>int DefaultBufferSize
>```
><b>Summary:</b> The send/receive buffer value, in bytes. The default is 8192 (8 Kb);
#### DefaultConnectTimeout
>```csharp
>int DefaultConnectTimeout
>```
><b>Summary:</b> The connect time-out value, in milliseconds. The default is 10000 (10 sec);
#### DefaultSendReceiveTimeout
>```csharp
>int DefaultSendReceiveTimeout
>```
><b>Summary:</b> The send/receive time-out value, in milliseconds. The default is 0;

<br />

## TcpTerminationType

>```csharp
>public enum TcpTerminationType
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None |  | 
| 1 | LineFeed | Line Feed |  | 
| 2 | CarriageReturn | Carriage Return |  | 
| 3 | CarriageReturnLineFeed | Carriage Return Line Feed |  | 



<br />

## TcpTerminationTypeHelper

>```csharp
>public static class TcpTerminationTypeHelper
>```

### Static Fields

#### CarriageReturn
>```csharp
>byte CarriageReturn
>```
#### LineFeed
>```csharp
>byte LineFeed
>```
### Static Methods

#### ConvertToBytes
>```csharp
>byte[] ConvertToBytes(TcpTerminationType tcpTerminationType)
>```
#### ConvertToString
>```csharp
>string ConvertToString(TcpTerminationType tcpTerminationType)
>```
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
