// Scans a network range and finds the IP cameras on it, with every signal Atc.Network has:
// ONVIF WS-Discovery, mDNS and SSDP, NetBIOS names, MAC vendors, and the camera ports (RTSP and ONVIF).
//
//   dotnet run --project sample/Atc.Network.Console.Scanner [startIp] [endIp]
//
// Without arguments it asks which network to scan (the default is 192.168.1.1 - 192.168.1.255).
// To read the model and stream URLs of ONVIF cameras, it asks for their ONVIF account, or set
// ATC_ONVIF_USERNAME and ATC_ONVIF_PASSWORD first.
// Box drawing characters and ✔ need UTF-8, which older Windows consoles don't use by default.
Console.OutputEncoding = System.Text.Encoding.UTF8;
AnsiConsole.Write(new Rule("[bold]Atc.Network camera scanner[/]").LeftJustified());

var (startIpAddress, endIpAddress) = RangeSelector.Select(args);

using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

var logger = new CollectingLogger();
using var ipScanner = new IPScanner(
    logger,
    new IPScannerConfig
    {
        IcmpPing = true,

        // Reverse DNS has no timeout yet, so a /24 would take minutes; mDNS, NetBIOS and ONVIF give the names instead.
        ResolveHostName = false,
        ResolveNetBiosName = true,
        ResolveMacAddress = true,

        // The online vendor lookup is rate-limited to about one device per second.
        ResolveVendorFromMacAddress = true,
        DiscoverServices = true,
        DiscoverOnvif = true,

        // RTSP, HTTP(S), Hikvision SDK / ONVIF, Tapo ONVIF and RTSP alternatives.
        PortNumbers = [80, 443, 554, 2020, 8000, 8080, 8554],
        TreatOpenPortsAsWebServices = IPServicePortExaminationLevel.None,
    });

var results = await AnsiConsole
    .Progress()
    .AutoClear(true)
    .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new ElapsedTimeColumn(), new SpinnerColumn())
    .StartAsync(async context =>
    {
        var scanTask = context.AddTask($"Scanning {startIpAddress} - {endIpAddress}");
        var answered = 0;
        ipScanner.ProgressReporting += (_, report) =>
        {
            if (report.Type == IPScannerProgressReportingType.IPAddressDone && report.LatestUpdate?.HasConnection == true)
            {
                Interlocked.Increment(ref answered);
            }

            scanTask.Value = report.PercentageCompleted;
            scanTask.Description = $"Scanning {startIpAddress} - {endIpAddress}  [green]{answered}[/] answered";
        };

        var scanResults = await ipScanner.ScanRange(startIpAddress, endIpAddress, cancellationTokenSource.Token);
        scanTask.Value = 100;
        return scanResults;
    });

if (results.ErrorMessage is not null)
{
    AnsiConsole.MarkupLine($"[red]Scan failed:[/] {Markup.Escape(results.ErrorMessage)}");
    return;
}

var candidates = results.CollectedResults
    .Select(CameraDetector.Detect)
    .Where(x => x.Score >= CameraDetector.MaybeCameraScore)
    .OrderByDescending(x => x.IsCamera)
    .ThenBy(x => x.Result.IPAddress.GetAddressBytes(), Comparer<byte[]>.Create((a, b) => a.AsSpan().SequenceCompareTo(b)))
    .ToList();
var cameras = candidates.Where(x => x.IsCamera).ToList();

ScanReport.WriteCameras(candidates, results);

var onvifCameras = cameras.Where(x => x.OnvifDeviceServiceUrl is not null).ToList();
var credential = onvifCameras.Count > 0
    ? OnvifCameraReader.GetCredential(onvifCameras.Count)
    : null;

if (onvifCameras.Count > 0)
{
    var details = await AnsiConsole
        .Status()
        .StartAsync("Asking the ONVIF cameras ...", _ =>
            Task.WhenAll(onvifCameras.Select(x => OnvifCameraReader.Read(x, credential, cancellationTokenSource.Token))));
    ScanReport.WriteOnvif(details);
}

ScanReport.WriteHints(cameras, credential is not null);

foreach (var message in logger.Messages)
{
    AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
}