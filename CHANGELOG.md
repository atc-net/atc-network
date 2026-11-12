# Changelog

## [2.0.0](https://github.com/atc-net/atc-network/compare/v1.0.236...v2.0.0) (2026-11-12)


### ⚠ BREAKING CHANGES

* ITcpClient and IUdpClient have a new ReadAllAsync method, which custom implementations must add.
* ITcpClient, IUdpClient, ITcpServer and IUdpServer have a new ErrorOccurred event, which custom implementations must add.
* ITcpServer has new members (ClientConnected, ClientDisconnected, ClientDataReceived and two Send overloads), so custom ITcpServer implementations must add them. TcpServer now handles clients concurrently, so DataReceived can be raised from several clients at the same time.
* IUdpServer has a new DatagramReceived event, so custom IUdpServer implementations must add it.
* TcpServer.StartAsync throws a SocketException when the listener cannot be started (for example when the port is in use), so a host fails to start instead of running without the server.
* UdpServer no longer replies "pong" to "ping" by default. Set UdpServerConfig.RespondToPing = true to keep the old behaviour.
* Atc.Network now targets net10.0 only. Consumers on .NET 8 must move to .NET 10 to use this version.

### build

* target net10.0 and update NuGet packages to latest ([8727dd8](https://github.com/atc-net/atc-network/commit/8727dd871a2de2a13f2037812d04eec09e10396f))


### New features

* add optional TLS to TcpClient and TcpServer ([258106f](https://github.com/atc-net/atc-network/commit/258106f81e59eb85bb7f640040a59934ae669c5e))
* add UdpServer.DatagramReceived with the sender's endpoint ([939b39b](https://github.com/atc-net/atc-network/commit/939b39bf60e755d68d5c8f04546a12461bef952f))
* ask hosts for their NetBIOS names with NetBiosHelper ([44ce290](https://github.com/atc-net/atc-network/commit/44ce290a905708fa064b0eb5557d3ca7143f2fd7))
* emit OpenTelemetry metrics and traces ([70b67b0](https://github.com/atc-net/atc-network/commit/70b67b011bfa2480f74a60a19d006d945a10e74a))
* find ONVIF cameras with WS-Discovery ([ca64e7d](https://github.com/atc-net/atc-network/commit/ca64e7d03cbb35e5af71e650c518294a1bf9a4c7))
* find services on the LAN with mDNS and SSDP discovery ([d17f43a](https://github.com/atc-net/atc-network/commit/d17f43a137f4b524336f4cabaa57b185fd41f1f0))
* get the time from an NTP server with NtpHelper ([662d49a](https://github.com/atc-net/atc-network/commit/662d49ab4333ebcc82f6e7ce36db184cc16b5db3))
* look up MAC address vendors offline from the IEEE registry ([c035a70](https://github.com/atc-net/atc-network/commit/c035a70170a3f995f9be99df7f17b63a2a14eb47))
* raise ErrorOccurred for errors the clients and servers handle ([6c395d8](https://github.com/atc-net/atc-network/commit/6c395d82a56eb79050ea1d91e46c91877e39cef8))
* read ONVIF camera models and stream URLs with OnvifClient ([5efe889](https://github.com/atc-net/atc-network/commit/5efe8892b2dd7c5cd5cf7f95a82d75e0fd84d26f))
* read received data as an async stream with ReadAllAsync ([d6ef48e](https://github.com/atc-net/atc-network/commit/d6ef48e42ba1ca7727995a6158e5c011c53bb3ff))
* serve several TcpServer clients at once and reply to one client ([ad07051](https://github.com/atc-net/atc-network/commit/ad0705172171f91a08616986ee7dbb7d8cd2ed0c))
* split received data into messages with TcpClientConfig.MessageFraming ([aed8ce4](https://github.com/atc-net/atc-network/commit/aed8ce4ac2c89a62341d057fc0ee599c90b5c977))
* support IPv6 and dual-mode servers ([a051e7c](https://github.com/atc-net/atc-network/commit/a051e7cc5a685b4b2d76d83f0a18c8e3d6e25d54))
* trace the route to a host with TracerouteHelper ([42d1adf](https://github.com/atc-net/atc-network/commit/42d1adf122709d46b38a2b4c495347f76104f92d))
* wake devices with Wake-on-LAN magic packets ([e5e85ba](https://github.com/atc-net/atc-network/commit/e5e85bae7d19bd24795172694604e3f9b9886c5b))


### Bug fixes

* create UdpServer without a config ([00e84d1](https://github.com/atc-net/atc-network/commit/00e84d180116ab4d77ccbde6bebb3cde4f80d820))
* depend on Microsoft.Extensions.Hosting.Abstractions instead of the full hosting package ([8e927dd](https://github.com/atc-net/atc-network/commit/8e927ddd443803cbcdf5cea951721970474340a5))
* don't follow redirects in the HTTP reachability check ([776f164](https://github.com/atc-net/atc-network/commit/776f164f6ae5bae3ac415cdb18a35e21c3db3f72))
* hide only link-local networks in the scanner sample ([1650093](https://github.com/atc-net/atc-network/commit/1650093ef8750575001d66ec7d6afb8d8530ac61))
* keep receiving when a DataReceived handler throws ([b973896](https://github.com/atc-net/atc-network/commit/b973896f894d65369a850ec429a9cc17715b5bc2))
* keep TcpServer running when a handler or client connection fails ([83b138d](https://github.com/atc-net/atc-network/commit/83b138de8d79c191953f774d9077597cc2b3632a))
* keep the clients working when a connection event handler throws ([f1c3139](https://github.com/atc-net/atc-network/commit/f1c31390de0e31624ecba86c43ef5fa11a08cd8d))
* keep the VNC session when an event handler throws ([900eaec](https://github.com/atc-net/atc-network/commit/900eaecc19563f1480f3b4fcc00b2c0ed1239b13))
* keep UdpServer receiving when a handler or a receive fails ([bc20478](https://github.com/atc-net/atc-network/commit/bc2047811956ac96031ff36228d93db93b081657))
* let the ARP cache expire after 90 seconds ([6e689cf](https://github.com/atc-net/atc-network/commit/6e689cfa3a6a08281722c794f29977d2dcd66f96))
* list every config property in ToString ([bf9895e](https://github.com/atc-net/atc-network/commit/bf9895e7de204d2b39ed9a3541fb135691bbfb71))
* make the UdpServer ping reply opt-in ([a2bf59d](https://github.com/atc-net/atc-network/commit/a2bf59dc57f76f19904eacf96b08cea8acf75efa))
* receive on dual-mode UdpServer sockets on macOS ([1c1211d](https://github.com/atc-net/atc-network/commit/1c1211d04d2f55cbd67c490383f47aebdcbec708))
* report HTTP ports as reachable on any HTTP response ([bf0a76b](https://github.com/atc-net/atc-network/commit/bf0a76bb2950f6ec6557377e7312919076f811f0))
* report the round-trip time of a ping reply instead of the call's elapsed time ([6269475](https://github.com/atc-net/atc-network/commit/62694753363df7bb2c6551eb6e8e3a136e7094a5))
* resolve findings from the updated analyzers ([3a1eeb1](https://github.com/atc-net/atc-network/commit/3a1eeb131ff5fdd9869be6be0d43abbef46b4935))
* send CR LF in the right order for CarriageReturnLineFeed ([12cd2f1](https://github.com/atc-net/atc-network/commit/12cd2f15b076e34f27ca27f7891140cbfb8d3cb5))
* send on the connected UdpClient socket so it works on macOS ([11787cf](https://github.com/atc-net/atc-network/commit/11787cffc78c8e10fd1ff9bfd362c57b640285f1))
* spell TCP correctly in TcpServer log messages ([78574df](https://github.com/atc-net/atc-network/commit/78574df5742ee6533cfa9f2049e0b4230b6c36e4))
* start the TcpServer listener in StartAsync ([14af16c](https://github.com/atc-net/atc-network/commit/14af16c0273364e8d1122b8249c6366b7811c78d))
* wake queued TcpServer sends when a client disconnects ([39c4b25](https://github.com/atc-net/atc-network/commit/39c4b25be27e2c7fd1d9332fe8ab0d26dfa9934c))


### Performance improvements

* assemble TcpServer messages without recopying them on every read ([0002e56](https://github.com/atc-net/atc-network/commit/0002e569c69b3e6b6f4aec540d2cc45606ffbf6c))
* copy only the received bytes in TcpClient, UdpClient and UdpServer ([0ffc729](https://github.com/atc-net/atc-network/commit/0ffc7299eceff1c2fb6b8e124cb4301525fbe7a8))
* reuse shared HttpClient instances ([8735ff3](https://github.com/atc-net/atc-network/commit/8735ff35b30f4fb408d1bd788e2ada22616d87da))
* scan the ports of an IP address concurrently ([dc62e62](https://github.com/atc-net/atc-network/commit/dc62e6240e59532ca2a482a20a6e619d6a2baa74))

## Changelog

All notable changes to this project will be documented in this file.

From 2.0.0 onwards this file is maintained by [release-please](https://github.com/googleapis/release-please) from [Conventional Commits](https://www.conventionalcommits.org/), and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html). Earlier versions (up to 1.0.236) were released without a changelog.
