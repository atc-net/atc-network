// ReSharper disable LocalizableElement
if (args.Length < 1)
{
    Console.WriteLine("Usage: dotnet run -- <host> [port] [password]");
    return;
}

var host = args[0];
var port = args.Length > 1 ? int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture) : VncConstants.DefaultPort;
var password = args.Length > 2 ? args[2] : null;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);
    builder.AddConsole();
});

var logger = loggerFactory.CreateLogger<VncClient>();

var vncClient = new VncClient(
    logger,
    host,
    port,
    new VncClientConfig
    {
        BitsPerPixel = 32,
        Depth = 24,
        SharedDesktop = true,
    });

vncClient.Connected += () => Console.WriteLine("Connected");
vncClient.Disconnected += () => Console.WriteLine("Disconnected");
vncClient.ConnectionStateChanged += (_, e) => Console.WriteLine($"Connection: {e.State}");
vncClient.FramebufferUpdated += (_, e) => Console.WriteLine($"Framebuffer updated: {e.Rectangle}");
vncClient.BellReceived += () => Console.WriteLine("Bell!");
vncClient.ServerCutText += text => Console.WriteLine($"Server clipboard: {text}");
vncClient.ConnectionLost += () => Console.WriteLine("Connection lost!");

Console.WriteLine($"Connecting to {host}:{port}...");
if (!await vncClient.Connect())
{
    Console.WriteLine("Cannot connect");
    vncClient.Dispose();
    return;
}

if (password is not null)
{
    Console.WriteLine("Authenticating...");
    if (!await vncClient.Authenticate(password))
    {
        Console.WriteLine("Authentication failed");
        vncClient.Dispose();
        return;
    }
}
else
{
    // Attempt no-auth
    if (!await vncClient.Authenticate(string.Empty))
    {
        Console.WriteLine("Authentication failed (no password provided)");
        vncClient.Dispose();
        return;
    }
}

Console.WriteLine("Initializing...");
await vncClient.Initialize();

var fb = vncClient.Framebuffer!;
Console.WriteLine($"Desktop: {fb.DesktopName}");
Console.WriteLine($"Resolution: {fb.Width}x{fb.Height}");
Console.WriteLine($"Pixel Format: {fb.PixelFormat}");

Console.WriteLine("Starting updates (press Enter to disconnect)...");
await vncClient.StartUpdates();

Console.ReadLine();

await vncClient.Disconnect();
vncClient.Dispose();

Console.WriteLine("Done.");