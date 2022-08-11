<div style='text-align: right'>
[References](Index.md)
</div>

# References extended

## [Atc.Network](Atc.Network.md)

- [AtcNetworkAssemblyTypeInitializer](Atc.Network.md#atcnetworkassemblytypeinitializer)
- [ConnectionState](Atc.Network.md#connectionstate)
- [ConnectionStateEventArgs](Atc.Network.md#connectionstateeventargs)
  -  Properties
     - ErrorMessage
     - State
  -  Methods
     - ToString()
- [ExceptionExtensions](Atc.Network.md#exceptionextensions)
  -  Static Methods
     - IsKnownExceptionForConsumerDisposed(this Exception exception)
     - IsKnownExceptionForNetworkCableUnplugged(this Exception exception)
- [IpAddressExtensions](Atc.Network.md#ipaddressextensions)
- [IPProtocolType](Atc.Network.md#ipprotocoltype)
- [IPScannerConfigExtensions](Atc.Network.md#ipscannerconfigextensions)
  -  Static Methods
     - GetTasksToProcessCount(this IPScannerConfig ipScannerConfig)
- [LoggingEventIdConstants](Atc.Network.md#loggingeventidconstants)
  -  Static Fields
     - int ClientNotConnected
     - int Connected
     - int Connecting
     - int ConnectionError
     - int DataReceivedByteLength
     - int DataReceiveError
     - int DataReceiveNoData
     - int DataReceiveTimeout
     - int DataSendingByteLength
     - int Disconnected
     - int Disconnecting
     - int Reconnected
     - int Reconnecting
- [NetworkQualityCategoryType](Atc.Network.md#networkqualitycategorytype)
- [TcpClientExtensions](Atc.Network.md#tcpclientextensions)
  -  Static Methods
     - SetBufferSizeAndTimeouts(this TcpClient tcpClient, int sendTimeout = 0, int sendBufferSize = 8192, int receiveTimeout = 0, int receiveBufferSize = 8192)
     - SetKeepAlive(this TcpClient tcpClient, int tcpKeepAliveTime = 2, int tcpKeepAliveInterval = 2, int tcpKeepAliveRetryCount = 5)

## [Atc.Network.Helpers](Atc.Network.Helpers.md)

- [ArpHelper](Atc.Network.Helpers.md#arphelper)
  -  Static Methods
     - GetArpResult()
- [DnsLookupHelper](Atc.Network.Helpers.md#dnslookuphelper)
  -  Static Methods
     - GetHostname(IPAddress ipAddress, CancellationToken cancellationToken)
- [IPAddressV4Helper](Atc.Network.Helpers.md#ipaddressv4helper)
  -  Static Methods
     - GetAddressesInRange(IPAddress ipAddress, int cidrMaskLength)
     - GetAddressesInRange(IPAddress startIpAddress, IPAddress endIpAddress)
     - GetLocalAddress()
     - GetStartAndEndAddressesInRange(IPAddress ipAddress, int cidrMaskLength)
     - IsInRange(IPAddress ipAddress, string cidrMask)
     - ValidateAddresses(IPAddress startIpAddress, IPAddress endIpAddress)
- [MacAddressVendorLookupHelper](Atc.Network.Helpers.md#macaddressvendorlookuphelper)
  -  Static Methods
     - LookupVendorNameFromMacAddress(string macAddress, CancellationToken cancellationToken = null)
- [PingHelper](Atc.Network.Helpers.md#pinghelper)
  -  Static Methods
     - GetStatus(IPAddress ipAddress, int timeoutInMs = 1000)

## [Atc.Network.Internet](Atc.Network.Internet.md)

- [IPPortScan](Atc.Network.Internet.md#ipportscan)
  -  Methods
     - CanConnectWithHttp(int portNumber = 80, CancellationToken cancellationToken = null)
     - CanConnectWithHttps(int portNumber = 80, CancellationToken cancellationToken = null)
     - CanConnectWithTcp(int portNumber, CancellationToken cancellationToken = null)
- [IPScanner](Atc.Network.Internet.md#ipscanner)
  -  Events
     - ProgressReporting
  -  Methods
     - Dispose()
     - Scan(IPAddress ipAddress, CancellationToken cancellationToken = null)
     - ScanCidrRange(IPAddress ipAddress, int cidrMaskLength, CancellationToken cancellationToken = null)
     - ScanRange(IPAddress startIpAddress, IPAddress endIpAddress, CancellationToken cancellationToken = null)
- [IPScannerConfig](Atc.Network.Internet.md#ipscannerconfig)
  -  Properties
     - Ping
     - PortNumbers
     - ResolveHostName
     - ResolveIPProtocolHttp
     - ResolveMacAddress
     - ResolveVendorFromMacAddress
     - Timeout
     - TimeoutHttp
     - TimeoutPing
     - TimeoutTcp
- [IPScannerConstants](Atc.Network.Internet.md#ipscannerconstants)
  -  Static Fields
     - int TimeoutHttpInMs
     - int TimeoutInMs
     - int TimeoutPingInMs
     - int TimeoutTcpInMs
- [IPScannerProgressReport](Atc.Network.Internet.md#ipscannerprogressreport)
  -  Properties
     - LatestUpdate
     - PercentageCompleted
     - TasksProcessedCount
     - TasksToProcessCount
  -  Methods
     - ToString()

## [Atc.Network.Models](Atc.Network.Models.md)

- [ArpEntity](Atc.Network.Models.md#arpentity)
  -  Properties
     - IPAddress
     - MacAddress
     - Type
  -  Methods
     - ToString()
- [IPScanPortResult](Atc.Network.Models.md#ipscanportresult)
  -  Properties
     - CanConnect
     - IPAddress
     - Port
     - Protocol
  -  Methods
     - ToString()
- [IPScanResult](Atc.Network.Models.md#ipscanresult)
  -  Properties
     - End
     - ErrorMessage
     - HasConnection
     - Hostname
     - IPAddress
     - IsCompleted
     - MacAddress
     - MacVendor
     - OpenPort
     - PingStatus
     - Ports
     - Start
     - TimeDiff
  -  Methods
     - ToString()
- [IPScanResults](Atc.Network.Models.md#ipscanresults)
  -  Properties
     - CollectedResults
     - End
     - ErrorMessage
     - IsCompleted
     - Start
     - TimeDiff
  -  Methods
     - ToString()
- [PingStatusResult](Atc.Network.Models.md#pingstatusresult)
  -  Properties
     - Exception
     - IPAddress
     - PingInMs
     - QualityCategory
     - Status
  -  Methods
     - ToString()

## [Atc.Network.Tcp](Atc.Network.Tcp.md)

- [TcpClient](Atc.Network.Tcp.md#tcpclient)
  -  Properties
     - IsConnected
  -  Events
     - Connected
     - ConnectionStateChanged
     - DataReceived
     - Disconnected
     - NoDataReceived
  -  Methods
     - Connect(CancellationToken cancellationToken = null)
     - Disconnect()
     - Dispose()
     - Send(byte[] data, CancellationToken cancellationToken = null)
     - Send(byte[] data, TcpTerminationType terminationType, CancellationToken cancellationToken = null)
     - Send(Encoding encoding, string data, CancellationToken cancellationToken = null)
     - Send(string data, CancellationToken cancellationToken = null)
- [TcpClientConfig](Atc.Network.Tcp.md#tcpclientconfig)
  -  Properties
     - ConnectTimeout
     - ReceiveBufferSize
     - ReceiveTimeout
     - SendBufferSize
     - SendTimeout
     - TerminationType
- [TcpClientKeepAliveConfig](Atc.Network.Tcp.md#tcpclientkeepaliveconfig)
  -  Properties
     - KeepAliveInterval
     - KeepAliveRetryCount
     - KeepAliveTime
     - ReconnectOnSenderSocketClosed
- [TcpConstants](Atc.Network.Tcp.md#tcpconstants)
  -  Static Fields
     - int DefaultBufferSize
     - int DefaultConnectTimeout
     - int DefaultSendReceiveTimeout
- [TcpTerminationType](Atc.Network.Tcp.md#tcpterminationtype)
- [TcpTerminationTypeHelper](Atc.Network.Tcp.md#tcpterminationtypehelper)
  -  Static Fields
     - byte CarriageReturn
     - byte LineFeed
  -  Static Methods
     - ConvertToBytes(TcpTerminationType tcpTerminationType)
     - ConvertToString(TcpTerminationType tcpTerminationType)

<hr /><div style='text-align: right'><i>Generated by MarkdownCodeDoc version 1.2</i></div>
