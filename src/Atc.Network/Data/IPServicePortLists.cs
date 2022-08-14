namespace Atc.Network.Data;

public static class IPServicePortLists
{
    public static IReadOnlyCollection<ushort> GetWellKnown()
    {
        var serviceProtocolTypes = Enum.GetValues(typeof(ServiceProtocolType));
        var portNumbers = new List<ushort>();
        foreach (var item in serviceProtocolTypes)
        {
            var serviceProtocolType = (ServiceProtocolType)item;
            if (serviceProtocolType is ServiceProtocolType.None or ServiceProtocolType.Unknown)
            {
                continue;
            }

            portNumbers.AddRange(GetWellKnown(serviceProtocolType));
        }

        return portNumbers
            .OrderBy(x => x)
            .ToList();
    }

    public static IReadOnlyCollection<ushort> GetWellKnown(
        ServiceProtocolType serviceProtocolType)
    {
        return serviceProtocolType switch
        {
            ServiceProtocolType.None => new List<ushort>(),
            ServiceProtocolType.Unknown => new List<ushort>(),
            ServiceProtocolType.Ftp => WellKnownForFtp,
            ServiceProtocolType.Ftps => WellKnownForFtps,
            ServiceProtocolType.Http => WellKnownForHttp,
            ServiceProtocolType.Https => WellKnownForHttps,
            ServiceProtocolType.Rtsp => WellKnownForRtsp,
            ServiceProtocolType.Ssh => WellKnownForSsh,
            ServiceProtocolType.Telnet => WellKnownForTelnet,
            _ => throw new SwitchCaseDefaultException(serviceProtocolType),
        };
    }

    public static IReadOnlyCollection<ushort> GetWellKnownOrCommon()
    {
        var serviceProtocolTypes = Enum.GetValues(typeof(ServiceProtocolType));
        var portNumbers = new List<ushort>();
        foreach (var item in serviceProtocolTypes)
        {
            var serviceProtocolType = (ServiceProtocolType)item;
            if (serviceProtocolType is ServiceProtocolType.None or ServiceProtocolType.Unknown)
            {
                continue;
            }

            portNumbers.AddRange(GetWellKnownOrCommon(serviceProtocolType));
        }

        return portNumbers
            .OrderBy(x => x)
            .ToList();
    }

    public static IReadOnlyCollection<ushort> GetWellKnownOrCommon(
        ServiceProtocolType serviceProtocolType)
    {
        return serviceProtocolType switch
        {
            ServiceProtocolType.None => new List<ushort>(),
            ServiceProtocolType.Unknown => new List<ushort>(),
            ServiceProtocolType.Ftp => WellKnownOrCommonForFtp,
            ServiceProtocolType.Ftps => WellKnownOrCommonForFtps,
            ServiceProtocolType.Http => WellKnownOrCommonForHttp,
            ServiceProtocolType.Https => WellKnownOrCommonForHttps,
            ServiceProtocolType.Rtsp => WellKnownOrCommonForRtsp,
            ServiceProtocolType.Ssh => WellKnownOrCommonForSsh,
            ServiceProtocolType.Telnet => WellKnownOrCommonForTelnet,
            _ => throw new SwitchCaseDefaultException(serviceProtocolType),
        };
    }

    public static readonly IReadOnlyCollection<ushort> WellKnownForFtp = new List<ushort>
    {
        20,
        21,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownOrCommonForFtp = new List<ushort>
    {
        20,
        21,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownForFtps = new List<ushort>
    {
        989,
        990,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownOrCommonForFtps = new List<ushort>
    {
        989,
        990,
        6619,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownForHttp = new List<ushort>
    {
        80,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownOrCommonForHttp = new List<ushort>
    {
        80,
        280,
        591,
        1965,
        2480,
        4444,
        4445,
        4567,
        5000,
        5104,
        5800,
        5985,
        7001,
        8000,
        8005,
        8008,
        8042,
        8080,
        8088,
        8096,
        8222,
        8280,
        8281,
        8403,
        8530,
        8887,
        9080,
        9981,
        11371,
        12046,
        16080,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownForHttps = new List<ushort>
    {
        443,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownOrCommonForHttps = new List<ushort>
    {
        443,
        832,
        981,
        1311,
        4444,
        4445,
        5001,
        5986,
        7000,
        7002,
        8243,
        8333,
        8403,
        8448,
        8531,
        8888,
        9443,
        12043,
        18091,
        18092,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownForRtsp = new List<ushort>
    {
        554,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownOrCommonForRtsp = new List<ushort>
    {
        554,
        7070,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownForSsh = new List<ushort>
    {
        22,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownOrCommonForSsh = new List<ushort>
    {
        22,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownForTelnet = new List<ushort>
    {
        23,
    };

    public static readonly IReadOnlyCollection<ushort> WellKnownOrCommonForTelnet = new List<ushort>
    {
        23,
    };
}