namespace Atc.Network.Test.Helpers;

public sealed class NetBiosHelperTests
{
    private const ushort TransactionId = 0x1234;

    /// <summary>
    /// The wildcard name "*" padded with zero bytes, first-level encoded (each half-byte + 'A').
    /// </summary>
    private const string EncodedWildcard = "CKAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public void CreateRequest_IsANodeStatusQueryForTheWildcardName()
    {
        // Act
        var actual = NetBiosHelper.CreateRequest(TransactionId);

        // Assert - header (id, flags 0, 1 question), the encoded name as one 32-byte label, type NBSTAT (0x21), class IN.
        byte[] expected =
        [
            0x12, 0x34, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x20, .. Encoding.ASCII.GetBytes(EncodedWildcard), 0x00,
            0x00, 0x21, 0x00, 0x01,
        ];
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseResponse_ReadsTheNamesWorkgroupAndMacAddress()
    {
        // Arrange
        var response = NetBiosResponseBuilder.Create(
            TransactionId,
            [("DESKTOP-ABC", 0x00, false), ("WORKGROUP", 0x00, true), ("DESKTOP-ABC", 0x20, false)],
            [0x00, 0x15, 0x5D, 0x01, 0x02, 0x03]);

        // Act
        var actual = NetBiosHelper.ParseResponse(response, IPAddress.Parse("192.168.1.10"), TransactionId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(IPAddress.Parse("192.168.1.10"), actual.IPAddress);
        Assert.Equal("DESKTOP-ABC", actual.ComputerName);
        Assert.Equal("WORKGROUP", actual.Workgroup);
        Assert.Equal("00-15-5D-01-02-03", actual.MacAddress);
        Assert.Equal(3, actual.Names.Count);
        Assert.Equal(new NetBiosName("DESKTOP-ABC", 0x20, IsGroup: false), actual.Names[2]);
        Assert.True(actual.Names[1].IsGroup);
    }

    [Fact]
    public void ParseResponse_ZeroMacAddress_IsNull()
    {
        // Arrange - Samba reports a zero MAC address.
        var response = NetBiosResponseBuilder.Create(TransactionId, [("NAS", 0x00, false)], new byte[6]);

        // Act
        var actual = NetBiosHelper.ParseResponse(response, IPAddress.Loopback, TransactionId);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.MacAddress);
        Assert.Null(actual.Workgroup);
    }

    [Fact]
    public void ParseResponse_CompressedAnswerName_IsRead()
    {
        // Arrange - a response that repeats the question and points to it from the answer.
        var full = NetBiosResponseBuilder.Create(TransactionId, [("PC", 0x00, false)], [1, 2, 3, 4, 5, 6]);
        byte[] question = [0x20, .. Encoding.ASCII.GetBytes(EncodedWildcard), 0x00, 0x00, 0x21, 0x00, 0x01];
        byte[] response = [.. full.AsSpan(0, 12), .. question, 0xC0, 0x0C, .. full.AsSpan(12 + 34)];
        response[5] = 1;

        // Act
        var actual = NetBiosHelper.ParseResponse(response, IPAddress.Loopback, TransactionId);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("PC", actual.ComputerName);
        Assert.Equal("01-02-03-04-05-06", actual.MacAddress);
    }

    [Fact]
    public void ParseResponse_OtherTransactionId_IsIgnored()
    {
        // Arrange
        var response = NetBiosResponseBuilder.Create(0x9999, [("PC", 0x00, false)], new byte[6]);

        // Act & Assert
        Assert.Null(NetBiosHelper.ParseResponse(response, IPAddress.Loopback, TransactionId));
    }

    [Fact]
    public void ParseResponse_Query_IsIgnored()
    {
        // Arrange - the query flag (QR) is not set.
        var response = NetBiosResponseBuilder.Create(TransactionId, [("PC", 0x00, false)], new byte[6]);
        response[2] = 0x00;

        // Act & Assert
        Assert.Null(NetBiosHelper.ParseResponse(response, IPAddress.Loopback, TransactionId));
    }

    [Fact]
    public void ParseResponse_TruncatedAnywhere_ReturnsNullWithoutThrowing()
    {
        // Arrange
        var response = NetBiosResponseBuilder.Create(TransactionId, [("DESKTOP-ABC", 0x00, false), ("WORKGROUP", 0x00, true)], [0, 0x15, 0x5D, 1, 2, 3]);

        // Act & Assert - the statistics after the MAC address may be cut off, but not the names or the MAC address.
        var namesAndMacEnd = 12 + 34 + 10 + 1 + (2 * 18) + 6;
        for (var length = 0; length < namesAndMacEnd; length++)
        {
            Assert.Null(NetBiosHelper.ParseResponse(response.AsSpan(0, length), IPAddress.Loopback, TransactionId));
        }
    }

    [Fact]
    public void NetBiosName_ToString_ShowsTheSuffix()
        => Assert.Equal("DESKTOP-ABC<20>", new NetBiosName("DESKTOP-ABC", 0x20, IsGroup: false).ToString());

    [Fact]
    public async Task GetNodeStatus_FromAHost_ReturnsItsNames()
    {
        // Arrange - the fake host answers with the request's transaction id.
        using var responder = new FakeResponder(request =>
        [
            NetBiosResponseBuilder.Create(ReadTransactionId(request), [("DESKTOP-ABC", 0x00, false), ("WORKGROUP", 0x00, true)], [0, 0x15, 0x5D, 1, 2, 3]),
        ]);

        // Act
        var actual = await NetBiosHelper.GetNodeStatus(responder.EndPoint, TimeSpan.FromSeconds(5), CancellationToken.None);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("DESKTOP-ABC", actual.ComputerName);
        Assert.Equal("WORKGROUP", actual.Workgroup);
        Assert.Equal(IPAddress.Loopback, actual.IPAddress);
        Assert.Equal(50, Assert.Single(responder.Requests).Length);
    }

    [Fact]
    public async Task GetNodeStatus_ForeignReplyFirst_IsIgnored()
    {
        // Arrange
        using var responder = new FakeResponder(request =>
        [
            NetBiosResponseBuilder.Create((ushort)(ReadTransactionId(request) + 1), [("WRONG", 0x00, false)], new byte[6]),
            NetBiosResponseBuilder.Create(ReadTransactionId(request), [("RIGHT", 0x00, false)], new byte[6]),
        ]);

        // Act
        var actual = await NetBiosHelper.GetNodeStatus(responder.EndPoint, TimeSpan.FromSeconds(5), CancellationToken.None);

        // Assert
        Assert.Equal("RIGHT", actual?.ComputerName);
    }

    [Fact]
    public async Task GetNodeStatus_NoAnswer_ReturnsNull()
    {
        // Arrange - most hosts don't answer NetBIOS, so no answer is a normal result, not an error.
        using var responder = new FakeResponder(_ => []);

        // Act
        var actual = await NetBiosHelper.GetNodeStatus(responder.EndPoint, TimeSpan.FromMilliseconds(200), CancellationToken.None);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task GetNodeStatus_Cancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var responder = new FakeResponder(_ => []);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            NetBiosHelper.GetNodeStatus(responder.EndPoint, TimeSpan.FromSeconds(30), cancellationTokenSource.Token));
    }

    [Fact]
    public async Task GetNodeStatus_RecordsMessagesAndAnActivity()
    {
        // Arrange
        using var telemetry = new TelemetryCollector();
        using var responder = new FakeResponder(request => [NetBiosResponseBuilder.Create(ReadTransactionId(request), [("PC", 0x00, false)], new byte[6])]);

        // Act
        await NetBiosHelper.GetNodeStatus(responder.EndPoint, TimeSpan.FromSeconds(5), CancellationToken.None);

        // Assert
        var port = responder.EndPoint.Port;
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesSentInstrumentName, "netbios_client", port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesReceivedInstrumentName, "netbios_client", port));
        Assert.Single(telemetry.Activities("NetBios.GetNodeStatus", port));
    }

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task GetNodeStatus_LocalMachine_DoesNotThrow()
    {
        // Arrange - the local machine answers when NetBIOS over TCP/IP is on; otherwise the result is null.
        var localAddress = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(x => x.GetIPProperties().UnicastAddresses)
            .Select(x => x.Address)
            .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        Assert.SkipWhen(localAddress is null, "No IPv4 address.");

        // Act
        var exception = await Record.ExceptionAsync(async () => await NetBiosHelper.GetNodeStatus(localAddress ?? IPAddress.Loopback));

        // Assert
        Assert.Null(exception);
    }

    private static ushort ReadTransactionId(byte[] request)
        => NetBiosResponseBuilder.ReadTransactionId(request);
}