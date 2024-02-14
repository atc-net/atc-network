namespace Atc.Network.Helpers;

/// <summary>
/// Provides utilities for appending termination sequences to data arrays.
/// </summary>
public static class TerminationHelper
{
    /// <summary>
    /// Appends termination bytes to a data array if the specified termination type is not already present at the end of the array.
    /// </summary>
    /// <param name="data">The data array to append the termination bytes to, if necessary.</param>
    /// <param name="terminationType">The type of termination sequence to append.</param>
    /// <remarks>
    /// This method first checks if the termination type is None, in which case it does nothing. If the termination type is specified,
    /// it converts the termination type to its byte array representation and checks if the data array already ends with this sequence.
    /// If not, it appends the termination bytes to the end of the data array.
    /// </remarks>
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