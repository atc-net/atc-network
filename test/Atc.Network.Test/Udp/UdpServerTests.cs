namespace Atc.Network.Test.Udp;

[Trait(Traits.Category, Traits.Categories.SkipWhenLiveUnitTesting)]
public sealed class UdpServerTests
{
    [Fact]
    public void UdpServer_WithoutConfig_UsesDefaultConfig()
    {
        // Arrange
        var port = GetFreePort();

        // Act
        using var udpServer = new UdpServer(NullLogger.Instance, IPAddress.Loopback, port);

        // Assert
        Assert.False(udpServer.IsRunning);
    }

    private static int GetFreePort()
    {
        using var probe = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint).Port;
    }
}