namespace Atc.Network.Test.Extensions;

public class IPScannerConfigExtensionsTests
{
    [Theory]
    [InlineData(4, true, true, true, true, 0, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(3, false, true, true, true, 0, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(2, false, false, true, true, 0, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(0, false, false, false, true, 0, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(0, false, false, false, false, 0, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(1, false, false, true, false, 0, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(10, true, true, true, true, 3, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(9, false, true, true, true, 3, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(8, false, false, true, true, 3, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(6, false, false, false, true, 3, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(6, false, false, false, false, 3, IPServicePortExaminationLevel.WellKnownAndCommon)]
    [InlineData(7, false, false, true, false, 3, IPServicePortExaminationLevel.WellKnownAndCommon)]
    public void GetTasksToProcessCount(
        int expected,
        bool resolvePing,
        bool resolveHostName,
        bool resolveMacAddress,
        bool resolveVendorFromMacAddress,
        ushort numberOfPorts,
        IPServicePortExaminationLevel treatOpenPortsAsWebServices)
    {
        // Arrange
        var ipScannerConfig = new IPScannerConfig
        {
            IcmpPing = resolvePing,
            ResolveHostName = resolveHostName,
            ResolveMacAddress = resolveMacAddress,
            ResolveVendorFromMacAddress = resolveVendorFromMacAddress,
            TreatOpenPortsAsWebServices = treatOpenPortsAsWebServices,
        };

        for (var i = 0; i < numberOfPorts; i++)
        {
            ipScannerConfig.PortNumbers.Add((ushort)(i + 10));
        }

        // Act
        var actual = ipScannerConfig.GetTasksToProcessCount();

        // Assert
        Assert.Equal(expected, actual);
    }
}