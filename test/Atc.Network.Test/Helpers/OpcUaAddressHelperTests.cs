// ReSharper disable StringLiteralTypo
namespace Atc.Network.Test.Helpers;

[SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = "OK")]
[SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "OK")]
public class OpcUaAddressHelperTests
{
    [Theory]
    [InlineData(true, "opc.tcp://milo.digitalpetri.com:62541", false)]
    [InlineData(true, "opc.tcp://milo.digitalpetri.com:62541/", false)]
    [InlineData(true, "opc.tcp://milo.digitalpetri.com:62541/milo", false)]
    [InlineData(true, "opc.tcp://192.168.1.1:62541", true)]
    [InlineData(true, "opc.tcp://192.168.1.1:62541/", true)]
    [InlineData(true, "opc.tcp://192.168.1.1:62541/milo", true)]
    [InlineData(false, "", false)]
    [InlineData(false, "milo.digitalpetri.com:62541/milo", false)]
    [InlineData(false, "opc.tcp://62541/milo", false)]
    [InlineData(false, "opc.tcp://milo.digitalpetri.com:62541/milo", true)]
    [InlineData(false, "opc.tcp://192.168.1:62541", true)]
    [InlineData(false, "opc.tcp://192.168.1:62541/", true)]
    [InlineData(false, "opc.tcp://192.168.1:62541/milo", true)]
    public void IsValid(
        bool expected,
        string url,
        bool restrictToIp4Address)
    {
        // Act
        var isValid = OpcUaAddressHelper.IsValid(url, restrictToIp4Address);

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData(true, "opc.tcp://milo.digitalpetri.com:62541", false)]
    [InlineData(true, "opc.tcp://milo.digitalpetri.com:62541/", false)]
    [InlineData(true, "opc.tcp://milo.digitalpetri.com:62541/milo", false)]
    [InlineData(true, "opc.tcp://192.168.1.1:62541", true)]
    [InlineData(true, "opc.tcp://192.168.1.1:62541/", true)]
    [InlineData(true, "opc.tcp://192.168.1.1:62541/milo", true)]
    [InlineData(false, "milo.digitalpetri.com:62541/milo", false)]
    [InlineData(false, "opc.tcp://62541/milo", false)]
    [InlineData(false, "opc.tcp://milo.digitalpetri.com:62541/milo", true)]
    [InlineData(false, "opc.tcp://192.168.1:62541", true)]
    [InlineData(false, "opc.tcp://192.168.1:62541/", true)]
    [InlineData(false, "opc.tcp://192.168.1:62541/milo", true)]

    public void IsValid_AsUri(
        bool expected,
        string url,
        bool restrictToIp4Address)
    {
        // Arrange
        var uri = new Uri(url);

        // Act
        var isValid = OpcUaAddressHelper.IsValid(uri, restrictToIp4Address);

        // Assert
        Assert.Equal(expected, isValid);
    }
}