namespace Atc.Network.Test.Helpers;

public sealed class WakeOnLanHelperTests
{
    private static readonly byte[] Mac = [0x01, 0x23, 0x45, 0x67, 0x89, 0xAB];

    [Fact]
    public void CreateMagicPacket_IsSixFFBytesThenTheMacSixteenTimes()
    {
        // Act
        var actual = WakeOnLanHelper.CreateMagicPacket("01-23-45-67-89-AB");

        // Assert
        Assert.Equal(102, actual.Length);
        Assert.Equal(Enumerable.Repeat((byte)0xFF, 6), actual.Take(6));
        for (var i = 0; i < 16; i++)
        {
            Assert.Equal(Mac, actual.Skip(6 + (i * 6)).Take(6));
        }
    }

    [Theory]
    [InlineData("01-23-45-67-89-AB")]
    [InlineData("01:23:45:67:89:AB")]
    [InlineData("0123456789AB")]
    [InlineData("0123.4567.89AB")]
    [InlineData("01-23-45-67-89-ab")]
    [InlineData(" 01:23:45:67:89:ab ")]
    public void CreateMagicPacket_AcceptsCommonMacFormats(string macAddress)
    {
        // Act
        var actual = WakeOnLanHelper.CreateMagicPacket(macAddress);

        // Assert
        Assert.Equal(Mac, actual.Skip(6).Take(6));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("01-23-45-67-89")]
    [InlineData("01-23-45-67-89-AB-CD-EF")]
    [InlineData("01-23-45-67-89-GG")]
    [InlineData("01-23:45-67:89-AB")]
    [InlineData("not a mac")]
    public void CreateMagicPacket_InvalidMac_Throws(string macAddress)
        => Assert.ThrowsAny<ArgumentException>(() => WakeOnLanHelper.CreateMagicPacket(macAddress));

    [Fact]
    public void CreateMagicPacket_PhysicalAddress_IsTheSameAsTheString()
    {
        // Act
        var actual = WakeOnLanHelper.CreateMagicPacket(new PhysicalAddress(Mac));

        // Assert
        Assert.Equal(WakeOnLanHelper.CreateMagicPacket("01-23-45-67-89-AB"), actual);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    public void CreateMagicPacket_SecureOnPassword_IsAppended(
        int passwordLength)
    {
        // Arrange
        var password = Enumerable.Range(1, passwordLength).Select(x => (byte)x).ToArray();

        // Act
        var actual = WakeOnLanHelper.CreateMagicPacket("01-23-45-67-89-AB", password);

        // Assert
        Assert.Equal(102 + passwordLength, actual.Length);
        Assert.Equal(password, actual.Skip(102));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(7)]
    public void CreateMagicPacket_SecureOnPasswordOfWrongLength_Throws(
        int passwordLength)
        => Assert.Throws<ArgumentException>(() => WakeOnLanHelper.CreateMagicPacket("01-23-45-67-89-AB", new byte[passwordLength]));

    [Fact]
    public async Task SendMagicPacket_ToAnEndPoint_SendsTheMagicPacket()
    {
        // Arrange
        using var receiver = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var endPoint = (IPEndPoint)receiver.Client.LocalEndPoint;
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        await WakeOnLanHelper.SendMagicPacket("01-23-45-67-89-AB", endPoint, cancellationToken: cancellationTokenSource.Token);
        var received = await receiver.ReceiveAsync(cancellationTokenSource.Token);

        // Assert
        Assert.Equal(WakeOnLanHelper.CreateMagicPacket("01-23-45-67-89-AB"), received.Buffer);
    }

    [Fact]
    public async Task SendMagicPacket_RecordsBytesSentAndAnActivity()
    {
        // Arrange
        using var telemetry = new TelemetryCollector();
        using var receiver = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var endPoint = (IPEndPoint)receiver.Client.LocalEndPoint;

        // Act
        await WakeOnLanHelper.SendMagicPacket("01-23-45-67-89-AB", endPoint);

        // Assert
        Assert.Equal(102, telemetry.Sum(NetworkTelemetry.BytesSentInstrumentName, "wake_on_lan", endPoint.Port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesSentInstrumentName, "wake_on_lan", endPoint.Port));
        Assert.Single(telemetry.Activities("WakeOnLan.Send", endPoint.Port));
    }

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task SendMagicPacket_ToTheDefaultBroadcast_IsSent()
    {
        // Act - a broadcast needs EnableBroadcast on the socket; without it the send fails.
        // 00-00-5E-00-53-00 is a MAC address reserved for documentation, so no device wakes.
        var exception = await Record.ExceptionAsync(() => WakeOnLanHelper.SendMagicPacket("00-00-5E-00-53-00"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Defaults_AreTheGlobalBroadcastOnPort9()
    {
        // Assert
        Assert.Equal(9, WakeOnLanHelper.DefaultPort);
        Assert.Equal(new IPEndPoint(IPAddress.Broadcast, 9), WakeOnLanHelper.DefaultEndPoint);
    }
}