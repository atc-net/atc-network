namespace Atc.Network.Test.Helpers;

public sealed class NtpHelperTests
{
    private const ulong NtpSecondsAt2000 = 3_155_673_600; // 2000-01-01T00:00:00Z

    [Fact]
    public void ToNtpTimestamp_KnownTimes()
    {
        // Assert - seconds in the high 32 bits, the fraction of a second in the low 32 bits.
        Assert.Equal(NtpSecondsAt2000 << 32, NtpHelper.ToNtpTimestamp(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal((2_208_988_800UL << 32) | 0x8000_0000UL, NtpHelper.ToNtpTimestamp(DateTime.UnixEpoch.AddMilliseconds(500)));
    }

    [Fact]
    public void FromNtpTimestamp_KnownTimes()
    {
        // Assert
        Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), NtpHelper.FromNtpTimestamp(NtpSecondsAt2000 << 32));
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(500), NtpHelper.FromNtpTimestamp((2_208_988_800UL << 32) | 0x8000_0000UL));
    }

    [Fact]
    public void FromNtpTimestamp_After2036_IsInTheNextEra()
    {
        // The 32-bit seconds roll over on 2036-02-07T06:28:16Z. Seconds with the highest bit clear
        // are read as after the rollover (RFC 4330, section 3).
        var actual = NtpHelper.FromNtpTimestamp(0);

        // Assert
        Assert.Equal(new DateTime(2036, 2, 7, 6, 28, 16, DateTimeKind.Utc), actual);
        Assert.Equal(DateTimeKind.Utc, actual.Kind);
    }

    [Theory]
    [InlineData("2024-06-01T12:34:56.789Z")]
    [InlineData("2036-02-07T06:28:15Z")]
    [InlineData("2036-02-07T06:28:17Z")]
    [InlineData("2050-12-31T23:59:59.5Z")]
    public void NtpTimestamp_RoundTrips(string time)
    {
        // Arrange
        var expected = DateTime.Parse(time, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

        // Act
        var actual = NtpHelper.FromNtpTimestamp(NtpHelper.ToNtpTimestamp(expected));

        // Assert - within a tick, since the NTP fraction is finer than a tick but not an exact multiple.
        Assert.InRange((actual - expected).Duration(), TimeSpan.Zero, TimeSpan.FromTicks(1));
    }

    [Fact]
    public void CreateRequest_IsAnSntpVersion4ClientRequestWithTheTransmitTime()
    {
        // Arrange
        var transmitTime = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var actual = NtpHelper.CreateRequest(transmitTime);

        // Assert - LI 0, version 4, mode 3 (client); the transmit timestamp in the last 8 bytes.
        Assert.Equal(48, actual.Length);
        Assert.Equal(0x23, actual[0]);
        Assert.All(actual.Skip(1).Take(39), x => Assert.Equal(0, x));
        Assert.Equal(NtpHelper.ToNtpTimestamp(transmitTime), BinaryPrimitives.ReadUInt64BigEndian(actual.AsSpan(40)));
    }

    [Fact]
    public void CalculateResult_OffsetAndRoundTripDelay()
    {
        // Arrange - the server is 5 s ahead, and each direction takes 100 ms; the server needs 100 ms to answer.
        var t1 = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var t2 = t1.AddMilliseconds(5_100);
        var t3 = t1.AddMilliseconds(5_200);
        var t4 = t1.AddMilliseconds(300);
        var server = new IPEndPoint(IPAddress.Loopback, 123);

        // Act
        var actual = NtpHelper.CalculateResult(server, stratum: 2, t1, t2, t3, t4);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(5), actual.Offset);
        Assert.Equal(TimeSpan.FromMilliseconds(200), actual.RoundTripDelay);
        Assert.Equal(t4.AddSeconds(5), actual.UtcTime);
        Assert.Equal(DateTimeKind.Utc, actual.UtcTime.Kind);
        Assert.Equal(2, actual.Stratum);
        Assert.Equal(server, actual.Server);
    }

    [Fact]
    public void ParseReply_ValidReply_ReturnsTheServerTimes()
    {
        // Arrange
        var originate = NtpHelper.ToNtpTimestamp(new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc));
        var receive = new DateTime(2024, 6, 1, 10, 0, 5, DateTimeKind.Utc);
        var transmit = receive.AddMilliseconds(1);
        var reply = CreateReply(originate, stratum: 3, receive, transmit);

        // Act
        var actual = NtpHelper.ParseReply(reply, originate);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(3, actual.Stratum);
        Assert.Equal(receive, actual.ReceiveTime);
        Assert.Equal(transmit, actual.TransmitTime);
    }

    [Fact]
    public void ParseReply_ForAnotherRequest_IsIgnored()
    {
        // Arrange - a reply must echo our transmit timestamp; anything else is old or spoofed.
        var reply = CreateReply(originate: 1, stratum: 2, DateTime.UtcNow, DateTime.UtcNow);

        // Act & Assert
        Assert.Null(NtpHelper.ParseReply(reply, expectedOriginate: 2));
    }

    [Fact]
    public void ParseReply_TooShort_IsIgnored()
        => Assert.Null(NtpHelper.ParseReply(new byte[47], expectedOriginate: 0));

    [Fact]
    public void ParseReply_KissOfDeath_ThrowsWithTheKissCode()
    {
        // Arrange - stratum 0 is a "kiss-o'-death": the server refuses, with a code in the reference id.
        var reply = CreateReply(originate: 7, stratum: 0, DateTime.UtcNow, DateTime.UtcNow);
        "RATE"u8.CopyTo(reply.AsSpan(12));

        // Act
        var exception = Assert.Throws<InvalidDataException>(() => NtpHelper.ParseReply(reply, expectedOriginate: 7));

        // Assert
        Assert.Contains("RATE", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseReply_ServerClockNotSynchronized_Throws()
    {
        // Arrange - leap indicator 3 means the server's own clock is not synchronized (RFC 4330: discard the reply).
        var reply = CreateReply(originate: 7, stratum: 2, DateTime.UtcNow, DateTime.UtcNow);
        reply[0] = 0xE4;

        // Act
        var exception = Assert.Throws<InvalidDataException>(() => NtpHelper.ParseReply(reply, expectedOriginate: 7));

        // Assert
        Assert.Contains("not synchronized", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseReply_NotAServerReply_Throws()
    {
        // Arrange - mode 3 is a client request, not a server reply.
        var reply = CreateReply(originate: 7, stratum: 2, DateTime.UtcNow, DateTime.UtcNow);
        reply[0] = 0x23;

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => NtpHelper.ParseReply(reply, expectedOriginate: 7));
    }

    [Fact]
    public async Task GetTime_FromAServer_ReturnsItsOffset()
    {
        // Arrange - the fake server is one hour ahead.
        using var server = new FakeNtpServer(TimeSpan.FromHours(1));

        // Act
        var actual = await NtpHelper.GetTime(server.EndPoint, TimeSpan.FromSeconds(5), CancellationToken.None);

        // Assert
        Assert.InRange(actual.Offset, TimeSpan.FromHours(1) - TimeSpan.FromSeconds(1), TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1));
        Assert.InRange(actual.RoundTripDelay, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        Assert.InRange(actual.UtcTime - DateTime.UtcNow, TimeSpan.FromHours(1) - TimeSpan.FromSeconds(2), TimeSpan.FromHours(1) + TimeSpan.FromSeconds(2));
        Assert.Equal(2, actual.Stratum);
        Assert.Equal(server.EndPoint, actual.Server);
    }

    [Fact]
    public async Task GetTime_IgnoresARepliesForAnotherRequest()
    {
        // Arrange - the server first sends a reply that does not echo our transmit timestamp.
        using var server = new FakeNtpServer(TimeSpan.FromHours(1), sendForeignReplyFirst: true);

        // Act
        var actual = await NtpHelper.GetTime(server.EndPoint, TimeSpan.FromSeconds(5), CancellationToken.None);

        // Assert
        Assert.InRange(actual.Offset, TimeSpan.FromMinutes(59), TimeSpan.FromMinutes(61));
    }

    [Fact]
    public async Task GetTime_NoReply_ThrowsTimeoutException()
    {
        // Arrange - a socket that never answers.
        using var silent = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var endPoint = (IPEndPoint)silent.Client.LocalEndPoint;

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => NtpHelper.GetTime(endPoint, TimeSpan.FromMilliseconds(200), CancellationToken.None));
    }

    [Fact]
    public async Task GetTime_Cancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var silent = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var endPoint = (IPEndPoint)silent.Client.LocalEndPoint;
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => NtpHelper.GetTime(endPoint, TimeSpan.FromSeconds(30), cancellationTokenSource.Token));
    }

    [Fact]
    public async Task GetTime_RecordsBytesAndAnActivity()
    {
        // Arrange
        using var telemetry = new TelemetryCollector();
        using var server = new FakeNtpServer(TimeSpan.Zero);

        // Act
        await NtpHelper.GetTime(server.EndPoint, TimeSpan.FromSeconds(5), CancellationToken.None);

        // Assert
        var port = server.EndPoint.Port;
        Assert.Equal(48, telemetry.Sum(NetworkTelemetry.BytesSentInstrumentName, "ntp_client", port));
        Assert.Equal(48, telemetry.Sum(NetworkTelemetry.BytesReceivedInstrumentName, "ntp_client", port));
        Assert.Equal(1, telemetry.Sum(NetworkTelemetry.MessagesReceivedInstrumentName, "ntp_client", port));
        Assert.Single(telemetry.Activities("Ntp.GetTime", port));
    }

    [Fact]
    public void NtpResult_ToString_ListsTheValues()
    {
        // Arrange
        var t1 = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var result = NtpHelper.CalculateResult(new IPEndPoint(IPAddress.Loopback, 123), 2, t1, t1, t1, t1);

        // Act
        var actual = result.ToString();

        // Assert
        Assert.Contains("Server: 127.0.0.1:123", actual, StringComparison.Ordinal);
        Assert.Contains("Stratum: 2", actual, StringComparison.Ordinal);
        Assert.Contains("Offset: ", actual, StringComparison.Ordinal);
        Assert.Contains("RoundTripDelay: ", actual, StringComparison.Ordinal);
        Assert.Contains("UtcTime: ", actual, StringComparison.Ordinal);
    }

    [Fact]
    [Trait(Traits.Category, Traits.Categories.Integration)]
    public async Task GetTime_PoolNtpOrg_IsCloseToTheLocalClock()
    {
        // Act
        var actual = await NtpHelper.GetTime();

        // Assert
        Assert.InRange(actual.Stratum, 1, 15);
        Assert.InRange(actual.Offset.Duration(), TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    private static byte[] CreateReply(
        ulong originate,
        int stratum,
        DateTime receive,
        DateTime transmit)
    {
        // LI 0, version 4, mode 4 (server).
        var reply = new byte[48];
        reply[0] = 0x24;
        reply[1] = (byte)stratum;
        BinaryPrimitives.WriteUInt64BigEndian(reply.AsSpan(24), originate);
        BinaryPrimitives.WriteUInt64BigEndian(reply.AsSpan(32), NtpHelper.ToNtpTimestamp(receive));
        BinaryPrimitives.WriteUInt64BigEndian(reply.AsSpan(40), NtpHelper.ToNtpTimestamp(transmit));
        return reply;
    }

    /// <summary>
    /// Answers one NTP request on loopback, with a clock that is <c>ahead</c> of the local one.
    /// </summary>
    private sealed class FakeNtpServer : IDisposable
    {
        private readonly System.Net.Sockets.UdpClient socket = new(new IPEndPoint(IPAddress.Loopback, 0));
        private readonly CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(10));
        private readonly Task serveTask;

        public FakeNtpServer(
            TimeSpan ahead,
            bool sendForeignReplyFirst = false)
        {
            EndPoint = (IPEndPoint)socket.Client.LocalEndPoint;
            serveTask = Serve(ahead, sendForeignReplyFirst);
        }

        public IPEndPoint EndPoint { get; }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            socket.Dispose();
            cancellationTokenSource.Dispose();
            _ = serveTask.Exception;
        }

        private async Task Serve(
            TimeSpan ahead,
            bool sendForeignReplyFirst)
        {
            var request = await socket.ReceiveAsync(cancellationTokenSource.Token);
            var originate = BinaryPrimitives.ReadUInt64BigEndian(request.Buffer.AsSpan(40));
            var now = DateTime.UtcNow + ahead;

            if (sendForeignReplyFirst)
            {
                await socket.SendAsync(CreateReply(originate + 1, 2, now, now), request.RemoteEndPoint, cancellationTokenSource.Token);
            }

            await socket.SendAsync(CreateReply(originate, 2, now, now), request.RemoteEndPoint, cancellationTokenSource.Token);
        }
    }
}