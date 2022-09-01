// ReSharper disable LocalizableElement

void OnDataReceived(
    byte[] data)
{
    Console.WriteLine($"Received Data Length: {data.Length}");
    var dataStr = Encoding.ASCII
        .GetString(data)
        .RemoveNonPrintableCharacter();
    Console.WriteLine($"Received Data: {dataStr}");
}

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);
    builder.AddConsole();
});

var logger = loggerFactory.CreateLogger<TcpClient>();

var tcpClient = new TcpClient(
    logger,
    "tcpbin.com",
    4242,
    new TcpClientConfig
    {
        TerminationType = TerminationType.LineFeed,
        ConnectTimeout = 1000,
    });

tcpClient.Connected += () => Console.WriteLine("Connected");
tcpClient.Disconnected += () => Console.WriteLine("Disconnected");
tcpClient.ConnectionStateChanged += (_, args) => Console.WriteLine($"Connection: {args.State}");
tcpClient.DataReceived += OnDataReceived;
if (!await tcpClient.Connect())
{
    Console.WriteLine("Cannot connect");
    tcpClient.Dispose();
    return;
}

await tcpClient.Send("ping", CancellationToken.None);

await Task.Delay(TimeSpan.FromMilliseconds(400));

await tcpClient.Disconnect();
tcpClient.Dispose();

Console.WriteLine("Press any key for quit");
Console.ReadLine();