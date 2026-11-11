namespace Atc.Network.Test.Helpers;

public class IeeeOuiMacAddressVendorLookupTests
{
    // The same layout as the IEEE files oui.csv (MA-L), mam.csv (MA-M) and oui36.csv (MA-S).
    private const string MaLargeCsv = """
        Registry,Assignment,Organization Name,Organization Address
        MA-L,00000C,"Cisco Systems, Inc",170 WEST TASMAN DRIVE SAN JOSE CA US 95134
        MA-L,001BC5,IEEE Registration Authority,445 Hoes Lane Piscataway NJ US 08554
        MA-L,3C5AB4,Google, Inc.,"1600 Amphitheatre Parkway Mountain View CA US 94043 "
        """;

    private const string MaSmallCsv = """
        Registry,Assignment,Organization Name,Organization Address
        MA-S,001BC5009,"Small ""Quoted"" Vendor",Somewhere
        """;

    [Theory]
    [InlineData("00:00:0C:12:34:56")]
    [InlineData("00-00-0C-12-34-56")]
    [InlineData("00000c123456")]
    [InlineData("0000.0c12.3456")]
    public async Task LookupVendorName_KnownAssignment_ReturnsOrganizationName(
        string macAddress)
    {
        // Arrange
        var lookup = IeeeOuiMacAddressVendorLookup.FromCsv(new StringReader(MaLargeCsv));

        // Act
        var actual = await lookup.LookupVendorName(macAddress);

        // Assert
        Assert.Equal("Cisco Systems, Inc", actual);
    }

    [Fact]
    public async Task LookupVendorName_UnquotedNameWithComma_ReturnsTheFirstField()
    {
        // Arrange - IEEE rows are not always quoted; the name then ends at the first comma.
        var lookup = IeeeOuiMacAddressVendorLookup.FromCsv(new StringReader(MaLargeCsv));

        // Act
        var actual = await lookup.LookupVendorName("3C:5A:B4:00:00:01");

        // Assert
        Assert.Equal("Google", actual);
    }

    [Fact]
    public async Task LookupVendorName_SmallerBlockInsideLargerBlock_ReturnsTheSmallerBlocksVendor()
    {
        // Arrange
        var lookup = IeeeOuiMacAddressVendorLookup.FromCsv(new StringReader(MaLargeCsv), new StringReader(MaSmallCsv));

        // Act
        var insideSmallBlock = await lookup.LookupVendorName("00:1B:C5:00:9F:FF");
        var outsideSmallBlock = await lookup.LookupVendorName("00:1B:C5:01:00:00");

        // Assert
        Assert.Equal("Small \"Quoted\" Vendor", insideSmallBlock);
        Assert.Equal("IEEE Registration Authority", outsideSmallBlock);
        Assert.Equal(4, lookup.Count);
    }

    [Theory]
    [InlineData("AA:BB:CC:DD:EE:FF")]
    [InlineData("not a mac")]
    [InlineData("")]
    public async Task LookupVendorName_UnknownOrInvalid_ReturnsNull(
        string macAddress)
    {
        // Arrange
        var lookup = IeeeOuiMacAddressVendorLookup.FromCsv(new StringReader(MaLargeCsv));

        // Act
        var actual = await lookup.LookupVendorName(macAddress);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task FromFiles_ReadsTheFiles()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), $"oui-{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(path, MaLargeCsv);

        try
        {
            // Act
            var lookup = IeeeOuiMacAddressVendorLookup.FromFiles(path);

            // Assert
            Assert.Equal(3, lookup.Count);
            Assert.Equal("Cisco Systems, Inc", await lookup.LookupVendorName("00:00:0C:00:00:01"));
        }
        finally
        {
            File.Delete(path);
        }
    }
}