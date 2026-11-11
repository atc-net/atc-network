namespace Atc.Network.Helpers;

/// <summary>
/// Looks up MAC address vendors offline, from the IEEE registry CSV files. Nothing is sent over the network.
/// </summary>
/// <remarks>
/// <para>
/// Download the files from the IEEE Registration Authority (https://regauth.standards.ieee.org/standards-ra-web/pub/view.html):
/// <c>oui.csv</c> (MA-L, 24-bit prefixes), <c>mam.csv</c> (MA-M, 28-bit) and <c>oui36.csv</c> (MA-S, 36-bit).
/// <c>oui.csv</c> alone covers most devices.
/// </para>
/// <para>
/// When several files are loaded, the longest matching prefix wins, so a small block inside a larger one
/// returns the small block's owner.
/// </para>
/// </remarks>
public sealed class IeeeOuiMacAddressVendorLookup : IMacAddressVendorLookup
{
    private static readonly int[] PrefixLengths = [9, 7, 6];

    private readonly Dictionary<string, string> vendorsByAssignment;

    private IeeeOuiMacAddressVendorLookup(
        Dictionary<string, string> vendorsByAssignment)
    {
        this.vendorsByAssignment = vendorsByAssignment;
    }

    /// <summary>
    /// Gets the number of assignments loaded.
    /// </summary>
    public int Count => vendorsByAssignment.Count;

    /// <summary>
    /// Loads IEEE registry CSV files, such as <c>oui.csv</c>, <c>mam.csv</c> and <c>oui36.csv</c>.
    /// </summary>
    /// <param name="csvFilePaths">The files.</param>
    /// <returns>The lookup.</returns>
    public static IeeeOuiMacAddressVendorLookup FromFiles(
        params string[] csvFilePaths)
    {
        ArgumentNullException.ThrowIfNull(csvFilePaths);

        var readers = csvFilePaths
            .Select(path => (TextReader)new StreamReader(path, Encoding.UTF8))
            .ToArray();

        try
        {
            return FromCsv(readers);
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader.Dispose();
            }
        }
    }

    /// <summary>
    /// Loads IEEE registry CSV content, with the columns Registry, Assignment, Organization Name and Organization Address.
    /// </summary>
    /// <param name="csvReaders">The CSV content. The readers are read to the end but not disposed.</param>
    /// <returns>The lookup.</returns>
    public static IeeeOuiMacAddressVendorLookup FromCsv(
        params TextReader[] csvReaders)
    {
        ArgumentNullException.ThrowIfNull(csvReaders);

        var assignments = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var reader in csvReaders)
        {
            while (reader.ReadLine() is { } line)
            {
                AddLine(assignments, line);
            }
        }

        return new IeeeOuiMacAddressVendorLookup(assignments);
    }

    /// <inheritdoc />
    public Task<string?> LookupVendorName(
        string macAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(macAddress);

        var hexDigits = ToHexDigits(macAddress);
        if (hexDigits is null)
        {
            return Task.FromResult<string?>(null);
        }

        foreach (var prefixLength in PrefixLengths)
        {
            if (hexDigits.Length >= prefixLength &&
                vendorsByAssignment.TryGetValue(hexDigits[..prefixLength], out var vendor))
            {
                return Task.FromResult<string?>(vendor);
            }
        }

        return Task.FromResult<string?>(null);
    }

    private static void AddLine(
        Dictionary<string, string> assignments,
        string line)
    {
        var fields = ParseCsvLine(line);
        if (fields.Count < 3)
        {
            return;
        }

        var assignment = fields[1].Trim().ToUpperInvariant();
        var organizationName = fields[2].Trim();
        if (!PrefixLengths.Contains(assignment.Length) ||
            !assignment.All(char.IsAsciiHexDigit) ||
            organizationName.Length == 0)
        {
            // The header row, or a line that is not an assignment.
            return;
        }

        assignments[assignment] = organizationName;
    }

    /// <summary>
    /// Returns the MAC address as upper-case hex digits without separators, or null when it is not a MAC address.
    /// </summary>
    private static string? ToHexDigits(string macAddress)
    {
        var builder = new StringBuilder(12);
        foreach (var c in macAddress)
        {
            if (char.IsAsciiHexDigit(c))
            {
                builder.Append(char.ToUpperInvariant(c));
            }
            else if (c is not (':' or '-' or '.'))
            {
                return null;
            }
        }

        return builder.Length == 12 ? builder.ToString() : null;
    }

    /// <summary>
    /// Splits one CSV line into fields. Quoted fields may contain commas and doubled quotes.
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        var i = 0;
        while (i < line.Length)
        {
            var c = line[i];
            i++;
            if (inQuotes)
            {
                if (c != '"')
                {
                    field.Append(c);
                }
                else if (i < line.Length && line[i] == '"')
                {
                    // A doubled quote inside a quoted field is one quote.
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = false;
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else
            {
                field.Append(c);
            }
        }

        fields.Add(field.ToString());
        return fields;
    }
}