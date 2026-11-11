var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHostedService<UdpTestClientWorker>();

await builder
    .Build()
    .RunAsync();