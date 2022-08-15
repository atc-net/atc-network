namespace Atc.Network.Models;

public class PingStatusResult
{
    public PingStatusResult(
        IPAddress ipAddress,
        IPStatus status,
        long pingInMs)
    {
        IPAddress = ipAddress;
        Exception = null;
        Status = status;
        PingInMs = pingInMs;
    }

    public PingStatusResult(
        IPAddress ipAddress,
        Exception? exception)
    {
        IPAddress = ipAddress;
        Exception = exception;
    }

    public IPAddress IPAddress { get; }

    public Exception? Exception { get; }

    public IPStatus Status { get; }

    public long PingInMs { get; }

    public NetworkQualityCategoryType QualityCategory
    {
        get
        {
            if (Status == IPStatus.TimedOut)
            {
                return NetworkQualityCategoryType.None;
            }

            return PingInMs switch
            {
                < 0 => NetworkQualityCategoryType.VeryPoor,
                < 10 => NetworkQualityCategoryType.Perfect,
                < 50 => NetworkQualityCategoryType.Excellent,
                < 100 => NetworkQualityCategoryType.VeryGood,
                < 250 => NetworkQualityCategoryType.Good,
                < 500 => NetworkQualityCategoryType.Fair,
                < 750 => NetworkQualityCategoryType.Poor,
                _ => NetworkQualityCategoryType.VeryPoor,
            };
        }
    }

    public override string ToString()
        => $"{nameof(IPAddress)}: {IPAddress}, {nameof(Exception)}: {Exception}, {nameof(Status)}: {Status}, {nameof(PingInMs)}: {PingInMs}, {nameof(QualityCategory)}: {QualityCategory}";
}