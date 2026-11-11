namespace Atc.Network.Test.TestHelpers;

/// <summary>
/// Builds NetBIOS node status responses for fake hosts.
/// </summary>
internal static class NetBiosResponseBuilder
{
    /// <summary>
    /// The wildcard name "*" padded with zero bytes, first-level encoded (each half-byte + 'A').
    /// </summary>
    public const string EncodedWildcard = "CKAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    public static ushort ReadTransactionId(byte[] request)
        => BinaryPrimitives.ReadUInt16BigEndian(request);

    /// <summary>
    /// Builds a node status response: header, the answer name in full (34 bytes), type NBSTAT, class IN,
    /// TTL 0, the name table, and the statistics, which start with the MAC address.
    /// </summary>
    /// <param name="transactionId">The transaction id of the request.</param>
    /// <param name="names">The names, with their suffix and whether each is a group name.</param>
    /// <param name="macAddress">The 6-byte MAC address.</param>
    /// <returns>The response.</returns>
    public static byte[] Create(
        ushort transactionId,
        (string Name, byte Suffix, bool IsGroup)[] names,
        byte[] macAddress)
    {
        var data = new List<byte> { (byte)names.Length };
        foreach (var (name, suffix, isGroup) in names)
        {
            data.AddRange(Encoding.ASCII.GetBytes(name.PadRight(15)));
            data.Add(suffix);
            data.AddRange([isGroup ? (byte)0x84 : (byte)0x04, 0x00]);
        }

        data.AddRange(macAddress);
        data.AddRange(new byte[40]);

        var response = new List<byte>
        {
            (byte)(transactionId >> 8), (byte)transactionId, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
            0x20,
        };
        response.AddRange(Encoding.ASCII.GetBytes(EncodedWildcard));
        response.AddRange([0x00, 0x00, 0x21, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, (byte)(data.Count >> 8), (byte)data.Count]);
        response.AddRange(data);
        return response.ToArray();
    }
}