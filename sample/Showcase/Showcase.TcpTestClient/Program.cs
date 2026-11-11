var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHostedService<TcpTestClientWorker>();

await builder
    .Build()
    .RunAsync();