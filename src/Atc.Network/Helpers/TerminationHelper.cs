namespace Atc.Network.Helpers;

public static class TerminationHelper
{
    public static void AppendTerminationBytesIfNeeded(
        ref byte[] data,
        TerminationType terminationType)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (terminationType == TerminationType.None)
        {
            return;
        }

        var terminationTypeAsBytes = TerminationTypeHelper.ConvertToBytes(terminationType);
        if (data.Length < terminationTypeAsBytes.Length)
        {
            return;
        }

        var x = data[^terminationTypeAsBytes.Length..];
        if (!x.SequenceEqual(terminationTypeAsBytes))
        {
            data = data
                .Concat(terminationTypeAsBytes)
                .ToArray();
        }
    }
}