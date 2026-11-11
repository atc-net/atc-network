var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

// Aspire passes the port in TCP_PORT; 9100 when the app runs on its own.
var port = builder.Configuration.GetValue("TCP_PORT", 9100);

builder.Services.AddSingleton(serviceProvider => new TcpServer(
    serviceProvider.GetRequiredService<ILogger<TcpServer>>(),
    IPAddress.Loopback,
    port,
    new TcpServerConfig
    {
        DefaultEncoding = Encoding.UTF8,
        TerminationType = TerminationType.LineFeed,
    }));

// Hosted services start in the order they are added: subscribe to the events first, then start the server.
builder.Services.AddHostedService<TcpServerEventHandler>();
builder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<TcpServer>());

await builder
    .Build()
    .RunAsync();