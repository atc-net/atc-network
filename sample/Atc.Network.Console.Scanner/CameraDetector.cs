namespace Atc.Network.Console.Scanner;

/// <summary>
/// Decides which scanned addresses are IP cameras, from the signals the IP scanner collects.
/// Each signal adds to a score; the reasons say what was seen.
/// </summary>
public static class CameraDetector
{
    /// <summary>
    /// The score from which an address counts as a camera.
    /// </summary>
    public const int CameraScore = 50;

    /// <summary>
    /// The score from which an address may be a camera.
    /// </summary>
    public const int MaybeCameraScore = 25;

    /// <summary>
    /// mDNS service types that cameras announce: Hikvision (PSIA, CGI), Axis, and generic RTSP.
    /// </summary>
    private static readonly string[] CameraServiceTypes =
    [
        "_psia._tcp.local",
        "_cgi._tcp.local",
        "_axis-video._tcp.local",
        "_rtsp._tcp.local",
    ];

    /// <summary>
    /// Camera brands, matched in MAC vendors, mDNS names and SSDP servers. Some (such as TP-Link) also make routers,
    /// so a brand alone only says "maybe".
    /// </summary>
    private static readonly string[] CameraBrands =
    [
        "hikvision", "hilook", "ezviz", "dahua", "imou", "amcrest", "lorex", "axis", "hanwha", "samsung techwin",
        "uniview", "reolink", "tp-link", "tapo", "vivotek", "bosch", "mobotix", "ubiquiti", "foscam", "annke", "swann",
    ];

    private static readonly ushort[] RtspPorts = [554, 8554];

    /// <summary>
    /// Scores a scanned address.
    /// </summary>
    /// <param name="result">The scan result.</param>
    /// <returns>The candidate, with its score, reasons and best name.</returns>
    public static CameraCandidate Detect(IPScanResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var reasons = new List<string>();
        var score = 0;

        void Add(
            int points,
            string reason)
        {
            score += points;
            reasons.Add(reason);
        }

        foreach (var device in result.OnvifDevices)
        {
            if (device.IsNetworkVideoTransmitter)
            {
                Add(100, "ONVIF");
            }
            else
            {
                Add(40, "ONVIF (not a camera)");
            }
        }

        foreach (var serviceType in result.MdnsServices
                     .Select(x => x.ServiceType)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Where(x => CameraServiceTypes.Contains(x, StringComparer.OrdinalIgnoreCase)))
        {
            Add(60, $"mDNS {serviceType.Split('.')[0]}");
        }

        foreach (var openPort in result.OpenPortNumbers.Where(x => RtspPorts.Contains(x)))
        {
            Add(30, $"RTSP :{openPort}");
        }

        AddBrand(result.MacVendor, "vendor", Add);
        AddBrand(result.MdnsServices.Select(x => x.InstanceName).FirstOrDefault(ContainsBrand), "mDNS name", Add);
        AddBrand(result.SsdpDevices.Select(x => x.Server).FirstOrDefault(ContainsBrand), "SSDP server", Add);

        return new CameraCandidate(result, score, reasons, GetBestName(result));
    }

    private static void AddBrand(
        string? text,
        string source,
        Action<int, string> add)
    {
        if (ContainsBrand(text))
        {
            add(MaybeCameraScore, source);
        }
    }

    private static bool ContainsBrand(string? text)
        => text is not null && CameraBrands.Any(brand => text.Contains(brand, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The best name for the address: ONVIF, then the mDNS service and host names, NetBIOS, and reverse DNS.
    /// </summary>
    private static string? GetBestName(IPScanResult result)
        => new[]
            {
                result.OnvifDevices.Select(x => x.Name).FirstOrDefault(x => !string.IsNullOrEmpty(x)),
                result.MdnsServices
                    .Where(x => CameraServiceTypes.Contains(x.ServiceType, StringComparer.OrdinalIgnoreCase))
                    .Select(x => x.InstanceName)
                    .FirstOrDefault(),
                result.MdnsHostName,
                result.NetBiosStatus?.ComputerName,
                result.Hostname,
            }
            .FirstOrDefault(x => !string.IsNullOrEmpty(x));
}