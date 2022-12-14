<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network.Udp

<br />

## IUdpClient

>```csharp
>public interface IUdpClient : IDisposable
>```

### Properties

#### IsConnected
>```csharp
>IsConnected
>```
><b>Summary:</b> Is client connected.
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
>Task Send(string data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(Encoding encoding, string data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(byte[] data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(byte[] data, TerminationType terminationType, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />

<br />

## IUdpServer

>```csharp
>public interface IUdpServer : IHostedService, IDisposable
>```

### Properties

#### IsRunning
>```csharp
>IsRunning
>```
><b>Summary:</b> Is running.
### Events

#### DataReceived
>```csharp
>DataReceived
>```
><b>Summary:</b> Event to raise when data has become available from the server.
### Methods

#### Send
>```csharp
>Task Send(EndPoint recipient, string data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`recipient`&nbsp;&nbsp;-&nbsp;&nbsp;The recipient endpoint.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(EndPoint recipient, Encoding encoding, string data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`recipient`&nbsp;&nbsp;-&nbsp;&nbsp;The recipient endpoint.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(EndPoint recipient, byte[] data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`recipient`&nbsp;&nbsp;-&nbsp;&nbsp;The recipient endpoint.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(EndPoint recipient, byte[] data, TerminationType terminationType, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`recipient`&nbsp;&nbsp;-&nbsp;&nbsp;The recipient endpoint.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />

<br />

## UdpClient
The main UdpClient - Handles call execution.

>```csharp
>public class UdpClient : IUdpClient, IDisposable
>```

### Properties

#### IsConnected
>```csharp
>IsConnected
>```
><b>Summary:</b> Is client connected.
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
>Task Send(string data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(Encoding encoding, string data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(byte[] data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(byte[] data, TerminationType terminationType, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />

<br />

## UdpClientConfig

>```csharp
>public class UdpClientConfig : UdpConfigBase
>```


<br />

## UdpConfigBase

>```csharp
>public abstract class UdpConfigBase
>```

### Properties

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
#### SendBufferSize
>```csharp
>SendBufferSize
>```
><b>Summary:</b> Gets or sets the size of the send buffer in bytes.
>
><b>Returns:</b> The size of the send buffer, in bytes. The default value is 8192 bytes.
#### TerminationType
>```csharp
>TerminationType
>```
><b>Summary:</b> Gets or sets the TerminationType.

<br />

## UdpServer
The main UdpServer - Handles call execution.

>```csharp
>public class UdpServer : IUdpServer, IHostedService, IDisposable
>```

### Properties

#### IsRunning
>```csharp
>IsRunning
>```
><b>Summary:</b> Is running.
### Events

#### DataReceived
>```csharp
>DataReceived
>```
><b>Summary:</b> Event to raise when data has become available from the server.
### Methods

#### Dispose
>```csharp
>void Dispose()
>```
#### Send
>```csharp
>Task Send(EndPoint recipient, string data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`recipient`&nbsp;&nbsp;-&nbsp;&nbsp;The recipient endpoint.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(EndPoint recipient, Encoding encoding, string data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`recipient`&nbsp;&nbsp;-&nbsp;&nbsp;The recipient endpoint.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(EndPoint recipient, byte[] data, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`recipient`&nbsp;&nbsp;-&nbsp;&nbsp;The recipient endpoint.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### Send
>```csharp
>Task Send(EndPoint recipient, byte[] data, TerminationType terminationType, CancellationToken cancellationToken)
>```
><b>Summary:</b> Send data.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`recipient`&nbsp;&nbsp;-&nbsp;&nbsp;The recipient endpoint.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`data`&nbsp;&nbsp;-&nbsp;&nbsp;The data to send.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`cancellationToken`&nbsp;&nbsp;-&nbsp;&nbsp;The cancellationToken.<br />
#### StartAsync
>```csharp
>Task StartAsync(CancellationToken cancellationToken)
>```
#### StopAsync
>```csharp
>Task StopAsync(CancellationToken cancellationToken)
>```

<br />

## UdpServerConfig

>```csharp
>public class UdpServerConfig : UdpConfigBase
>```

### Properties

#### EchoOnReceivedData
>```csharp
>EchoOnReceivedData
>```
><b>Summary:</b> Gets or sets the echo on received data.

<br />

## UpdConstants

>```csharp
>public static class UpdConstants
>```

### Static Fields

#### DefaultBufferSize
>```csharp
>int DefaultBufferSize
>```
><b>Summary:</b> The send/receive buffer value, in bytes. The default is 8192 (8 Kb);
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
