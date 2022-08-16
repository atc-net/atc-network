namespace Atc.Network.Test.Internet;

public class IPScannerConfigTests
{
    [Theory]
    [InlineData(0, IPServicePortExaminationLevel.None)]
    [InlineData(9, IPServicePortExaminationLevel.WellKnown)]
    [InlineData(60, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(65535, IPServicePortExaminationLevel.All)]
    public void Constructor_PortNumbers_IPServicePortExaminationLevel(
        int expected,
        IPServicePortExaminationLevel ipServicePortExaminationLevel)
    {
        // Act
        var actual = new IPScannerConfig(ipServicePortExaminationLevel);

        // Assert
        Assert.Equal(expected, actual.PortNumbers.Count);
    }

    [Theory]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.None)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Unknown)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Https)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Http)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Ftps)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Ftp)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Ssh)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Telnet)]
    [InlineData(0, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.None)]
    [InlineData(0, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Unknown)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Https)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Http)]
    [InlineData(2, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Ftps)]
    [InlineData(2, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Ftp)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Ssh)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Telnet)]
    [InlineData(0, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.None)]
    [InlineData(0, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Unknown)]
    [InlineData(20, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Https)]
    [InlineData(31, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Http)]
    [InlineData(3, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Ftps)]
    [InlineData(2, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Ftp)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Ssh)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Telnet)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.None)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Unknown)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Https)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Http)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Ftps)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Ftp)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Ssh)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Telnet)]
    public void Constructor_PortNumbers_IPServicePortExaminationLevel_ServiceProtocolType(
        int expected,
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType serviceProtocolType)
    {
        // Act
        var actual = new IPScannerConfig(ipServicePortExaminationLevel, serviceProtocolType);

        // Assert
        Assert.Equal(expected, actual.PortNumbers.Count);
    }

    [Theory]
    [InlineData(2, IPServicePortExaminationLevel.WellKnown, new[] { ServiceProtocolType.Https, ServiceProtocolType.Http })]
    [InlineData(4, IPServicePortExaminationLevel.WellKnown, new[] { ServiceProtocolType.Ftps, ServiceProtocolType.Ftps })]
    [InlineData(51, IPServicePortExaminationLevel.WellKnownAndCommon, new[] { ServiceProtocolType.Https, ServiceProtocolType.Http })]
    [InlineData(6, IPServicePortExaminationLevel.WellKnownAndCommon, new[] { ServiceProtocolType.Ftps, ServiceProtocolType.Ftps })]
    public void Constructor_PortNumbers_IPServicePortExaminationLevel_ServiceProtocolTypes(
        int expected,
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType[] serviceProtocolTypes)
    {
        // Act
        var actual = new IPScannerConfig(ipServicePortExaminationLevel, serviceProtocolTypes);

        // Assert
        Assert.Equal(expected, actual.PortNumbers.Count);
    }

    [Theory]
    [InlineData(0, IPServicePortExaminationLevel.None)]
    [InlineData(9, IPServicePortExaminationLevel.WellKnown)]
    [InlineData(60, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(65535, IPServicePortExaminationLevel.All)]
    public void SetPortNumbers_IPServicePortExaminationLevel(
        int expected,
        IPServicePortExaminationLevel ipServicePortExaminationLevel)
    {
        // Arrange
        var sut = new IPScannerConfig();

        // Act
        sut.SetPortNumbers(ipServicePortExaminationLevel);

        // Assert
        Assert.Equal(expected, sut.PortNumbers.Count);
    }

    [Theory]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.None)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Unknown)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Https)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Http)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Ftps)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Ftp)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Ssh)]
    [InlineData(0, IPServicePortExaminationLevel.None, ServiceProtocolType.Telnet)]
    [InlineData(0, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.None)]
    [InlineData(0, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Unknown)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Https)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Http)]
    [InlineData(2, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Ftps)]
    [InlineData(2, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Ftp)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Ssh)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnown, ServiceProtocolType.Telnet)]
    [InlineData(0, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.None)]
    [InlineData(0, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Unknown)]
    [InlineData(20, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Https)]
    [InlineData(31, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Http)]
    [InlineData(3, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Ftps)]
    [InlineData(2, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Ftp)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Ssh)]
    [InlineData(1, IPServicePortExaminationLevel.WellKnownAndCommon, ServiceProtocolType.Telnet)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.None)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Unknown)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Https)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Http)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Ftps)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Ftp)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Ssh)]
    [InlineData(65535, IPServicePortExaminationLevel.All, ServiceProtocolType.Telnet)]
    public void SetPortNumbers_IPServicePortExaminationLevel_ServiceProtocolType(
        int expected,
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType serviceProtocolType)
    {
        // Arrange
        var sut = new IPScannerConfig();

        // Act
        sut.SetPortNumbers(ipServicePortExaminationLevel, serviceProtocolType);

        // Assert
        Assert.Equal(expected, sut.PortNumbers.Count);
    }

    [Theory]
    [InlineData(2, IPServicePortExaminationLevel.WellKnown, new[] { ServiceProtocolType.Https, ServiceProtocolType.Http })]
    [InlineData(4, IPServicePortExaminationLevel.WellKnown, new[] { ServiceProtocolType.Ftps, ServiceProtocolType.Ftps })]
    [InlineData(51, IPServicePortExaminationLevel.WellKnownAndCommon, new[] { ServiceProtocolType.Https, ServiceProtocolType.Http })]
    [InlineData(6, IPServicePortExaminationLevel.WellKnownAndCommon, new[] { ServiceProtocolType.Ftps, ServiceProtocolType.Ftps })]
    public void SetPortNumbers_IPServicePortExaminationLevel_ServiceProtocolTypes(
        int expected,
        IPServicePortExaminationLevel ipServicePortExaminationLevel,
        ServiceProtocolType[] serviceProtocolTypes)
    {
        // Arrange
        var sut = new IPScannerConfig();

        // Act
        sut.SetPortNumbers(ipServicePortExaminationLevel, serviceProtocolTypes);

        // Assert
        Assert.Equal(expected, sut.PortNumbers.Count);
    }
}