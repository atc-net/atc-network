var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

// Aspire passes the port in UDP_PORT; 9101 when the app runs on its own.
var port = builder.Configuration.GetValue("UDP_PORT", 9101);

// UdpServer binds the port when it is created, and StartAsync marks it running.
// The UDP pair runs over IPv6 (::1); the TCP pair runs over IPv4.
builder.Services.AddSingleton(serviceProvider => new UdpServer(
    serviceProvider.GetRequiredService<ILogger<UdpServer>>(),
    IPAddress.IPv6Loopback,
    port,
    new UdpServerConfig
    {
        DefaultEncoding = Encoding.UTF8,
    }));

// Hosted services start in the order they are added: subscribe to the events first, then start the server.
builder.Services.AddHostedService<UdpServerEventHandler>();
builder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<UdpServer>());

await builder
    .Build()
    .RunAsync();