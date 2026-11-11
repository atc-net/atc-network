var builder = DistributedApplication.CreateBuilder(args);

// The servers listen on fixed ports, without the Aspire proxy: the proxy handles TCP only, not UDP,
// and the same setup for both keeps them easy to compare. The TCP pair uses IPv4 (127.0.0.1),
// the UDP pair IPv6 (::1). Each port is passed to its server
// in an environment variable, and to its client with WithReference.
var tcpServer = builder
    .AddProject<Projects.Showcase_TcpTestServer>("tcp-test-server")
    .WithEndpoint(port: 9100, scheme: "tcp", name: "tcp", env: "TCP_PORT", isProxied: false);

builder
    .AddProject<Projects.Showcase_TcpTestClient>("tcp-test-client")
    .WithReference(tcpServer.GetEndpoint("tcp"))
    .WaitFor(tcpServer);

var udpServer = builder
    .AddProject<Projects.Showcase_UdpTestServer>("udp-test-server")
    .WithEndpoint(port: 9101, scheme: "udp", name: "udp", env: "UDP_PORT", isProxied: false, protocol: ProtocolType.Udp);

builder
    .AddProject<Projects.Showcase_UdpTestClient>("udp-test-client")
    .WithReference(udpServer.GetEndpoint("udp"))
    .WaitFor(udpServer);

await builder
    .Build()
    .RunAsync();