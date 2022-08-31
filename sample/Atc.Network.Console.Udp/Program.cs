void OnServerDataReceived(
    byte[] data)
{
    Console.WriteLine($"Server received Data Length: {data.Length}");
    var dataStr = Encoding.ASCII
        .GetString(data)
        .RemoveNonPrintableCharacter();
    Console.WriteLine($"Server received Data: {dataStr}");
}

void OnClientDataReceived(
    byte[] data)
{
    Console.WriteLine($"Client received Data Length: {data.Length}");
    var dataStr = Encoding.ASCII
        .GetString(data)
        .RemoveNonPrintableCharacter();
    Console.WriteLine($"Client received Data: {dataStr}");
}

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);
    builder.AddConsole();
});

var loggerServer = loggerFactory.CreateLogger<UdpServer>();
var loggerClient = loggerFactory.CreateLogger<UdpClient>();

var udpServer = new UdpServer(
    loggerServer,
    new IPEndPoint(IPAddress.Loopback, 27001),
    new UdpServerConfig
    {
        EchoOnReceivedData = true,
    });

udpServer.DataReceived += OnServerDataReceived;

await udpServer.StartAsync(CancellationToken.None);

var udpClient = new UdpClient(
    loggerClient,
    new IPEndPoint(IPAddress.Loopback, 27001),
    27002);

udpClient.Connected += () => Console.WriteLine("Connected");
udpClient.Disconnected += () => Console.WriteLine("Disconnected");
udpClient.ConnectionStateChanged += (_, args) => Console.WriteLine($"Connection state: {args.State}");
udpClient.DataReceived += OnClientDataReceived;
if (!await udpClient.Connect())
{
    Console.WriteLine("Cannot connect");
    udpClient.Dispose();
    return;
}

await udpClient.Send("Hallo", CancellationToken.None);
await udpClient.Send("World..", CancellationToken.None);
await udpClient.Send("ping", CancellationToken.None);
await udpServer.Send(new IPEndPoint(IPAddress.Loopback, 27002), "The UdpServer say hallo", CancellationToken.None);

await Task.Delay(TimeSpan.FromMilliseconds(500));

await udpClient.Disconnect();
udpClient.Dispose();

await udpServer.StopAsync(CancellationToken.None);
udpServer.Dispose();

Console.WriteLine("Press any key for quit");
Console.ReadLine();