namespace Atc.Network.Console.Scanner;

/// <summary>
/// Shows the results: a table of the cameras found, and a tree with what the ONVIF cameras told.
/// </summary>
public static class ScanReport
{
    /// <summary>
    /// Writes the table of cameras and maybe-cameras.
    /// </summary>
    /// <param name="candidates">The candidates, cameras first.</param>
    /// <param name="results">The scan results.</param>
    public static void WriteCameras(
        IReadOnlyList<CameraCandidate> candidates,
        IPScanResults results)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(results);

        var cameraCount = candidates.Count(x => x.IsCamera);
        var table = new Table()
            .RoundedBorder()
            .Title($"[bold]{cameraCount} camera(s)[/] and {candidates.Count - cameraCount} maybe, among {results.CollectedResults.Count(x => x.HasConnection)} devices")
            .Caption($"[grey]Scanned in {Markup.Escape(results.TimeDiff ?? string.Empty)}[/]")
            .AddColumn(new TableColumn(string.Empty).Centered())
            .AddColumn(new TableColumn("IP address").NoWrap())
            .AddColumn("Name")
            .AddColumn("Vendor")
            .AddColumn("Open ports")
            .AddColumn("Found by");

        foreach (var candidate in candidates)
        {
            var result = candidate.Result;
            table.AddRow(
                candidate.IsCamera ? "[green]✔[/]" : "[yellow]?[/]",
                Markup.Escape(result.IPAddress.ToString()),
                candidate.IsCamera ? $"[bold]{Markup.Escape(candidate.Name ?? "-")}[/]" : Markup.Escape(candidate.Name ?? "-"),
                Markup.Escape(result.MacVendor ?? "-"),
                Markup.Escape(string.Join(", ", result.OpenPortNumbers)),
                Markup.Escape(string.Join(", ", candidate.Reasons)));
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Writes a tree with each ONVIF camera's clock, model and streams.
    /// </summary>
    /// <param name="cameras">What was read from each camera.</param>
    public static void WriteOnvif(IReadOnlyList<OnvifCameraDetails> cameras)
    {
        ArgumentNullException.ThrowIfNull(cameras);

        var tree = new Tree("[bold]ONVIF cameras[/]");
        foreach (var camera in cameras)
        {
            var node = tree.AddNode($"[bold]{Markup.Escape(camera.Camera.Name ?? "-")}[/] {Markup.Escape(camera.Camera.Result.IPAddress.ToString())} [grey]{Markup.Escape(camera.DeviceServiceUrl.ToString())}[/]");
            if (camera.Clock is { } clock)
            {
                node.AddNode($"Clock {clock:yyyy-MM-dd HH:mm:ss} UTC [grey]({camera.ClockOffset.TotalSeconds:+0.0;-0.0} s from this machine)[/]");
            }

            if (camera.Information is { } information)
            {
                node.AddNode(Markup.Escape($"{information.Manufacturer} {information.Model}, firmware {information.FirmwareVersion}"));
            }

            foreach (var stream in camera.Streams)
            {
                var profile = stream.Profile;
                node.AddNode($"[green]{Markup.Escape(profile.Name ?? profile.Token)}[/] {Markup.Escape(stream.StreamUri.ToString())} [grey]{profile.Width}x{profile.Height} {Markup.Escape(profile.Encoding ?? string.Empty)} {profile.FrameRate} fps[/]");
            }

            if (camera.Error is not null)
            {
                node.AddNode($"[red]{Markup.Escape(camera.Error)}[/]");
            }
        }

        AnsiConsole.Write(tree);
    }

    /// <summary>
    /// Writes hints: cameras without ONVIF, and how to read stream URLs.
    /// </summary>
    /// <param name="cameras">The cameras found.</param>
    /// <param name="hasCredential">Whether an ONVIF account was used.</param>
    public static void WriteHints(
        IReadOnlyList<CameraCandidate> cameras,
        bool hasCredential)
    {
        ArgumentNullException.ThrowIfNull(cameras);

        var hints = new List<string>();
        var withoutOnvif = cameras.Where(x => x.OnvifDeviceServiceUrl is null).Select(x => x.Result.IPAddress.ToString()).ToList();
        if (withoutOnvif.Count > 0)
        {
            hints.Add($"No ONVIF on {Markup.Escape(string.Join(", ", withoutOnvif))}: turn ONVIF on in the camera's settings to read its stream URLs (often off on Hikvision).");
        }

        if (!hasCredential && cameras.Any(x => x.OnvifDeviceServiceUrl is not null))
        {
            hints.Add($"Set [bold]{OnvifCameraReader.UserNameVariable}[/] and [bold]{OnvifCameraReader.PasswordVariable}[/] (the cameras' ONVIF account) to read their model and stream URLs without being asked.");
        }

        if (hints.Count > 0)
        {
            AnsiConsole.Write(new Panel(string.Join("\n", hints)).Header("Hints").BorderColor(Color.Grey));
        }
    }
}