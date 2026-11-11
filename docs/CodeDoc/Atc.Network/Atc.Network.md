<div style='text-align: right'>

[References](Index.md)&nbsp;&nbsp;-&nbsp;&nbsp;[References extended](IndexExtended.md)
</div>

# Atc.Network

<br />

## AtcNetworkAssemblyTypeInitializer

>```csharp
>public static class AtcNetworkAssemblyTypeInitializer
>```


<br />

## ConnectionState
Enumeration: ConnectionType.

>```csharp
>public enum ConnectionState
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None | Default None. | 
| 1 | Connecting | Connecting | Connecting. | 
| 2 | Connected | Connected | Connected. | 
| 3 | Disconnecting | Disconnecting | Disconnecting. | 
| 4 | Disconnected | Disconnected | Disconnected. | 
| 5 | ConnectionFailed | Connection failed | The connection failed. | 
| 6 | ReconnectionFailed | Reconnection failed | The reconnection failed. | 
| 7 | Reconnecting | Reconnecting | Reconnecting. | 
| 8 | Reconnected | Reconnected | Reconnected. | 
| 9 | Pulse | Pulse | Pulse. | 



<br />

## ConnectionStateEventArgs
ConnectionStateEventArgs.

>```csharp
>public class ConnectionStateEventArgs : EventArgs
>```

### Properties

#### ErrorMessage
>```csharp
>ErrorMessage
>```
><b>Summary:</b> Gets the error message.
#### State
>```csharp
>State
>```
><b>Summary:</b> Gets the state.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## ExceptionExtensions

>```csharp
>public static class ExceptionExtensions
>```

### Static Methods

#### IsKnownExceptionForConsumerDisposed
>```csharp
>ValueTuple<bool, SocketError?> IsKnownExceptionForConsumerDisposed(this Exception exception)
>```
#### IsKnownExceptionForNetworkCableUnplugged
>```csharp
>ValueTuple<bool, SocketError?> IsKnownExceptionForNetworkCableUnplugged(this Exception exception)
>```

<br />

## IPAddressExtensions

>```csharp
>public static class IPAddressExtensions
>```

### Static Methods

#### IsInRange
>```csharp
>bool IsInRange(this IPAddress ipAddress, string cidrNotation)
>```
#### IsPrivate
>```csharp
>bool IsPrivate(this IPAddress ipAddress)
>```
><b>Summary:</b> Is IP address in private network scope.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipAddress`&nbsp;&nbsp;-&nbsp;&nbsp;The ip address.<br />
>
><b>Remarks:</b> https://en.wikipedia.org/wiki/Reserved_IP_addresses
#### IsPublic
>```csharp
>bool IsPublic(this IPAddress ipAddress)
>```
#### ToUnsignedInt
>```csharp
>uint ToUnsignedInt(this IPAddress ipAddress)
>```

<br />

## IPScannerConfigExtensions

>```csharp
>public static class IPScannerConfigExtensions
>```

### Static Methods

#### GetTasksToProcessCount
>```csharp
>int GetTasksToProcessCount(this IPScannerConfig ipScannerConfig)
>```

<br />

## IPScannerProgressReportingType

>```csharp
>public enum IPScannerProgressReportingType
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None |  | 
| 1 | IPAddressStart | IPAddress Start |  | 
| 2 | IPAddressDone | IPAddress Done |  | 
| 3 | Counters | Counters |  | 
| 4 | Ping | Ping |  | 
| 5 | HostName | Host Name |  | 
| 6 | MacAddress | Mac Address |  | 
| 7 | MacVendor | Mac Vendor |  | 
| 8 | Tcp | Tcp |  | 
| 9 | ServiceHttp | Service Http |  | 
| 10 | NetBiosName | Net Bios Name |  | 



<br />

## IPServicePortExaminationLevel

>```csharp
>public enum IPServicePortExaminationLevel
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None |  | 
| 1 | WellKnown | Well Known |  | 
| 2 | WellKnownAndCommon | Well Known And Common |  | 
| 3 | All | All |  | 



<br />

## IPStatusExtensions
Provides extension methods for the `System.Net.NetworkInformation.IPStatus` enumeration.
><b>Remarks:</b> This static class extends the `System.Net.NetworkInformation.IPStatus` enumeration, allowing for easy retrieval of localized string descriptions for each IP status value. This can be particularly useful for displaying user-friendly status messages in applications that perform network operations and diagnostics.

>```csharp
>public static class IPStatusExtensions
>```

### Static Methods

#### GetLocalizedDescription
>```csharp
>string GetLocalizedDescription(this IPStatus ipStatus)
>```
><b>Summary:</b> Retrieves a localized description for a specified `System.Net.NetworkInformation.IPStatus` value.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ipStatus`&nbsp;&nbsp;-&nbsp;&nbsp;The  value for which a localized description is needed.<br />
>
><b>Returns:</b> A localized string representation of the specified `System.Net.NetworkInformation.IPStatus` value.
>
><b>Code example:</b>
>```csharp
>This example shows how to call the  method on an instance of the  enumeration:
>
>var status = IPStatus.TimedOut;
>var description = status.GetLocalizedDescription();
>Console.WriteLine(description); // Outputs the localized description for the IPStatus.TimedOut
>```

<br />

## LoggingEventIdConstants

>```csharp
>public static class LoggingEventIdConstants
>```


<br />

## NetworkErrorEventArgs
Event data for an error that a client or server handled and logged, so it did not stop.

>```csharp
>public class NetworkErrorEventArgs : EventArgs
>```

### Properties

#### EventName
>```csharp
>EventName
>```
><b>Summary:</b> Gets the event whose handler threw, such as "DataReceived", for `Atc.Network.NetworkErrorKind.EventHandler`; otherwise <see langword="null" />.
#### Exception
>```csharp
>Exception
>```
><b>Summary:</b> Gets the exception.
#### Kind
>```csharp
>Kind
>```
><b>Summary:</b> Gets where the error came from.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## NetworkErrorKind
Where an error reported by an `ErrorOccurred` event came from.

>```csharp
>public enum NetworkErrorKind
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | EventHandler | Event Handler | An event handler, or an `On…` override, threw. `Atc.Network.NetworkErrorEventArgs.EventName` names the event. The client or server keeps running. | 
| 1 | Receive | Receive | Receiving failed, for example because the connection was reset. | 
| 2 | ClientConnection | Client Connection | A server could not serve one client, for example because its TLS handshake failed. The server keeps serving the other clients. | 
| 3 | Listener | Listener | A server's listener stopped unexpectedly. The server no longer accepts clients. | 



<br />

## NetworkQualityCategoryType
Enumeration: NetworkQualityCategoryType.

>```csharp
>public enum NetworkQualityCategoryType
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None |  | 
| 1 | VeryPoor | Very poor | The very poor | 
| 2 | Poor | Poor | The poor | 
| 3 | Fair | Fair | The fair | 
| 4 | Good | Good | The good | 
| 5 | VeryGood | Good | The very good | 
| 6 | Excellent | Excellent | The excellent | 
| 7 | Perfect | Perfect | The perfect | 



<br />

## NetworkTelemetry
The names of the metrics and traces that Atc.Network emits, for use with OpenTelemetry, for example `AddMeter(NetworkTelemetry.MeterName)` and `AddSource(NetworkTelemetry.ActivitySourceName)`.
><b>Remarks:</b> Every measurement and activity is tagged with `Atc.Network.NetworkTelemetry.ComponentTag` (for example "tcp_client"), `Atc.Network.NetworkTelemetry.ServerAddressTag` and `Atc.Network.NetworkTelemetry.ServerPortTag`: the server a client connects to, or the address and port a server listens on.

>```csharp
>public static class NetworkTelemetry
>```

### Static Fields

#### ActiveConnectionsInstrumentName
>```csharp
>string ActiveConnectionsInstrumentName
>```
><b>Summary:</b> Connections that are open now. A server counts each connected client.
#### ActivitySourceName
>```csharp
>string ActivitySourceName
>```
><b>Summary:</b> The name of the `System.Diagnostics.ActivitySource`.
#### BytesReceivedInstrumentName
>```csharp
>string BytesReceivedInstrumentName
>```
><b>Summary:</b> Bytes received.
#### BytesSentInstrumentName
>```csharp
>string BytesSentInstrumentName
>```
><b>Summary:</b> Bytes sent.
#### ComponentTag
>```csharp
>string ComponentTag
>```
><b>Summary:</b> The tag with the component: "tcp_client", "tcp_server", "udp_client", "udp_server" or "vnc_client".
#### ConnectionFailuresInstrumentName
>```csharp
>string ConnectionFailuresInstrumentName
>```
><b>Summary:</b> Connect attempts that failed, and server-side TLS handshakes that failed.
#### EventTag
>```csharp
>string EventTag
>```
><b>Summary:</b> The tag with the event whose handler threw.
#### HandlerErrorsInstrumentName
>```csharp
>string HandlerErrorsInstrumentName
>```
><b>Summary:</b> Event handlers that threw, tagged with `Atc.Network.NetworkTelemetry.EventTag`.
#### MessagesReceivedInstrumentName
>```csharp
>string MessagesReceivedInstrumentName
>```
><b>Summary:</b> Messages received: one per DataReceived event (a TCP client read, a TCP server message or a UDP datagram).
#### MessagesSentInstrumentName
>```csharp
>string MessagesSentInstrumentName
>```
><b>Summary:</b> Messages sent: one per send call.
#### MeterName
>```csharp
>string MeterName
>```
><b>Summary:</b> The name of the `System.Diagnostics.Metrics.Meter`.
#### OutcomeTag
>```csharp
>string OutcomeTag
>```
><b>Summary:</b> The tag with the outcome of a reconnect attempt: "success" or "failure".
#### ReconnectsInstrumentName
>```csharp
>string ReconnectsInstrumentName
>```
><b>Summary:</b> Reconnect attempts, tagged with `Atc.Network.NetworkTelemetry.OutcomeTag` "success" or "failure".
#### ServerAddressTag
>```csharp
>string ServerAddressTag
>```
><b>Summary:</b> The tag with the server address.
#### ServerPortTag
>```csharp
>string ServerPortTag
>```
><b>Summary:</b> The tag with the server port.

<br />

## ServiceProtocolType

>```csharp
>public enum ServiceProtocolType
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None |  | 
| 1 | Unknown | Unknown |  | 
| 2 | Ftp | Ftp |  | 
| 3 | Ftps | Ftps |  | 
| 4 | Http | Http |  | 
| 5 | Https | Https |  | 
| 6 | Rtsp | Rtsp |  | 
| 7 | Ssh | Ssh |  | 
| 8 | Telnet | Telnet |  | 



<br />

## TcpClientExtensions

>```csharp
>public static class TcpClientExtensions
>```

### Static Methods

#### DisableKeepAlive
>```csharp
>void DisableKeepAlive(this TcpClient tcpClient)
>```
><b>Summary:</b> Disables the keep alive.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tcpClient`&nbsp;&nbsp;-&nbsp;&nbsp;The TCP client.<br />
#### SetBufferSizeAndTimeouts
>```csharp
>void SetBufferSizeAndTimeouts(this TcpClient tcpClient, int sendTimeout = 600000, int sendBufferSize = 8192, int receiveTimeout = 600000, int receiveBufferSize = 8192)
>```
><b>Summary:</b> Sets the buffer size and timeouts.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tcpClient`&nbsp;&nbsp;-&nbsp;&nbsp;The TCP client.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`sendTimeout`&nbsp;&nbsp;-&nbsp;&nbsp;The send timeout value of the connection in milliseconds.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`sendBufferSize`&nbsp;&nbsp;-&nbsp;&nbsp;Size of the send buffer in bytes.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`receiveTimeout`&nbsp;&nbsp;-&nbsp;&nbsp;The receive timeout value of the connection in milliseconds.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`receiveBufferSize`&nbsp;&nbsp;-&nbsp;&nbsp;Size of the receive buffer in bytes.<br />
#### SetKeepAlive
>```csharp
>void SetKeepAlive(this TcpClient tcpClient, int tcpKeepAliveTime = 2, int tcpKeepAliveInterval = 2, int tcpKeepAliveRetryCount = 5)
>```
><b>Summary:</b> Sets the KeepAlive options.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tcpClient`&nbsp;&nbsp;-&nbsp;&nbsp;The TcpClient.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tcpKeepAliveTime`&nbsp;&nbsp;-&nbsp;&nbsp;Specifies how often TCP sends keep-alive transmissions (milliseconds).<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tcpKeepAliveInterval`&nbsp;&nbsp;-&nbsp;&nbsp;Specifies how often TCP repeats keep-alive transmissions when no response is received.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tcpKeepAliveRetryCount`&nbsp;&nbsp;-&nbsp;&nbsp;The number of TCP keep alive probes that will be sent before the connection is terminated.<br />

<br />

## TcpMessageFraming
How a `Atc.Network.Tcp.TcpClient` splits the TCP byte stream into messages.

>```csharp
>public enum TcpMessageFraming
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None | No framing: `DataReceived` is raised once per read from the socket, which can hold part of a message, a whole message or several messages. Sends append the `Atc.Network.TcpMessageFraming.TerminationType`. | 
| 1 | TerminationType | Termination Type | Received data is collected until the configured `Atc.Network.TcpMessageFraming.TerminationType`, and `DataReceived` is raised once per message, including the terminator. Sends append the `Atc.Network.TcpMessageFraming.TerminationType`. | 
| 2 | LengthPrefix | Length Prefix | Every message starts with its length as a 4-byte big-endian integer. Sends add the length, and `DataReceived` is raised once per message, without the length. The `Atc.Network.TcpMessageFraming.TerminationType` is not used. | 



<br />

## TcpServerClientEventArgs
Event data that identifies one client connected to a TCP server.

>```csharp
>public class TcpServerClientEventArgs : EventArgs
>```

### Properties

#### ClientId
>```csharp
>ClientId
>```
><b>Summary:</b> Gets the id the server gave the client when it connected. Use it to send to this client.
#### RemoteEndPoint
>```csharp
>RemoteEndPoint
>```
><b>Summary:</b> Gets the client's endpoint.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## TcpServerDataReceivedEventArgs
Event data for a message a TCP server received from one client.

>```csharp
>public class TcpServerDataReceivedEventArgs : TcpServerClientEventArgs
>```

### Properties

#### Data
>```csharp
>Data
>```
><b>Summary:</b> Gets the received data.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## TerminationType

>```csharp
>public enum TerminationType
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None | The none - known as '\0'. | 
| 1 | LineFeed | Line Feed | The line feed - know as '\n' (0x0A). | 
| 2 | CarriageReturn | Carriage Return | The carriage return - know as '\r' (0x0D). | 
| 3 | CarriageReturnLineFeed | Carriage Return Line Feed | The carriage return line feed - know as '\r\n' (0x0D, 0x0A). | 
| 4 | EndOfText | End Of Text | The end of transmission - known as ETX (0x03). | 
| 5 | EndOfTransmission | End Of Transmission | The end of transmission - known as EOT (0x04). | 



<br />

## TransportProtocolType

>```csharp
>public enum TransportProtocolType
>```


| Value | Name | Description | Summary | 
| --- | --- | --- | --- | 
| 0 | None | None |  | 
| 1 | Udp | Udp |  | 
| 2 | Tcp | Tcp |  | 



<br />

## UdpDatagramReceivedEventArgs
Event data for a datagram received by a UDP server, including who sent it.

>```csharp
>public class UdpDatagramReceivedEventArgs : EventArgs
>```

### Properties

#### Data
>```csharp
>Data
>```
><b>Summary:</b> Gets the received data.
#### RemoteEndPoint
>```csharp
>RemoteEndPoint
>```
><b>Summary:</b> Gets the endpoint that sent the datagram. Use it as the recipient to reply.
### Methods

#### ToString
>```csharp
>string ToString()
>```

<br />

## UshortExtensions
Extension methods for <see langword="ushort" />

>```csharp
>public static class UshortExtensions
>```

### Static Methods

#### IsPortForIPService
>```csharp
>bool IsPortForIPService(this ushort portNumber, ServiceProtocolType serviceProtocolType, IPServicePortExaminationLevel matchLevel)
>```
#### IsWellKnownIPServicePort
>```csharp
>bool IsWellKnownIPServicePort(this ushort portNumber, ServiceProtocolType serviceProtocolType)
>```
><b>Summary:</b> Returns <see langword="true" /> if the value is a well known port for a IP service.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`serviceProtocolType`&nbsp;&nbsp;-&nbsp;&nbsp;Type of the service protocol.<br />
>
><b>Remarks:</b> See <see href="https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers" /> for a complete list of well known port numbers.
#### IsWellKnownOrCommonIPServicePort
>```csharp
>bool IsWellKnownOrCommonIPServicePort(this ushort portNumber, ServiceProtocolType serviceProtocolType)
>```
><b>Summary:</b> Returns <see langword="true" /> if the value is a well known port or a common substitute for a IP service.
>
><b>Parameters:</b><br>
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`portNumber`&nbsp;&nbsp;-&nbsp;&nbsp;The port number.<br />
>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`serviceProtocolType`&nbsp;&nbsp;-&nbsp;&nbsp;Type of the service protocol.<br />
>
><b>Remarks:</b> See <see href="https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers" /> for a complete list of well known port numbers.
<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
