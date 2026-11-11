namespace Atc.Network.Helpers;

/// <summary>
/// Wakes a sleeping or powered-off device on the network by sending it a Wake-on-LAN "magic packet".
/// </summary>
/// <remarks>
/// The magic packet is 6 bytes of 0xFF followed by the device's MAC address 16 times (102 bytes),
/// optionally followed by a 4- or 6-byte SecureOn password. It is sent as a UDP datagram,
/// by default as a broadcast to port 9. The device must have Wake-on-LAN enabled.
/// </remarks>
public static class WakeOnLanHelper
{
    /// <summary>
    /// The default port for Wake-on-LAN (the discard port). Port 7 is also common.
    /// </summary>
    public const int DefaultPort = 9;

    private const int SynchronizationStreamLength = 6;
    private const int MacAddressLength = 6;
    private const int MacAddressRepetitions = 16;
    private const int MagicPacketLength = SynchronizationStreamLength + (MacAddressLength * MacAddressRepetitions);

    /// <summary>
    /// Gets the default endpoint: the global broadcast address (255.255.255.255) on port 9.
    /// </summary>
    /// <remarks>
    /// Routers do not forward the global broadcast. To wake a device on another subnet,
    /// send to that subnet's broadcast address instead, such as 192.168.1.255.
    /// </remarks>
    public static IPEndPoint DefaultEndPoint
        => new(IPAddress.Broadcast, DefaultPort);

    /// <summary>
    /// Creates the magic packet for a MAC address.
    /// </summary>
    /// <param name="macAddress">
    /// The MAC address, such as "01-23-45-67-89-AB", "01:23:45:67:89:AB", "0123456789AB" or "0123.4567.89AB".
    /// </param>
    /// <param name="secureOnPassword">An optional SecureOn password of 4 or 6 bytes, appended to the packet.</param>
    /// <returns>The magic packet.</returns>
    /// <exception cref="ArgumentException">The MAC address or the password is not valid.</exception>
    public static byte[] CreateMagicPacket(
        string macAddress,
        byte[]? secureOnPassword = null)
        => CreateMagicPacket(ParseMacAddress(macAddress), secureOnPassword);

    /// <summary>
    /// Creates the magic packet for a MAC address.
    /// </summary>
    /// <param name="macAddress">The MAC address.</param>
    /// <param name="secureOnPassword">An optional SecureOn password of 4 or 6 bytes, appended to the packet.</param>
    /// <returns>The magic packet.</returns>
    /// <exception cref="ArgumentException">The MAC address is not 6 bytes, or the password is not 4 or 6 bytes.</exception>
    public static byte[] CreateMagicPacket(
        PhysicalAddress macAddress,
        byte[]? secureOnPassword = null)
    {
        ArgumentNullException.ThrowIfNull(macAddress);

        var macBytes = macAddress.GetAddressBytes();
        if (macBytes.Length != MacAddressLength)
        {
            throw new ArgumentException($"A MAC address must be {MacAddressLength} bytes, but got {macBytes.Length}.", nameof(macAddress));
        }

        if (secureOnPassword is not null &&
            secureOnPassword.Length != 4 &&
            secureOnPassword.Length != 6)
        {
            throw new ArgumentException($"A SecureOn password must be 4 or 6 bytes, but got {secureOnPassword.Length}.", nameof(secureOnPassword));
        }

        var packet = new byte[MagicPacketLength + (secureOnPassword?.Length ?? 0)];
        packet.AsSpan(0, SynchronizationStreamLength).Fill(0xFF);
        for (var i = 0; i < MacAddressRepetitions; i++)
        {
            macBytes.CopyTo(packet, SynchronizationStreamLength + (i * MacAddressLength));
        }

        secureOnPassword?.CopyTo(packet, MagicPacketLength);
        return packet;
    }

    /// <summary>
    /// Sends the magic packet for a MAC address.
    /// </summary>
    /// <param name="macAddress">
    /// The MAC address, such as "01-23-45-67-89-AB", "01:23:45:67:89:AB", "0123456789AB" or "0123.4567.89AB".
    /// </param>
    /// <param name="endPoint">
    /// Where to send the packet. The default is <see cref="DefaultEndPoint"/> (255.255.255.255:9).
    /// </param>
    /// <param name="secureOnPassword">An optional SecureOn password of 4 or 6 bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the packet has been sent.</returns>
    /// <exception cref="ArgumentException">The MAC address or the password is not valid.</exception>
    /// <exception cref="SocketException">The packet could not be sent.</exception>
    public static async Task SendMagicPacket(
        string macAddress,
        IPEndPoint? endPoint = null,
        byte[]? secureOnPassword = null,
        CancellationToken cancellationToken = default)
    {
        var packet = CreateMagicPacket(macAddress, secureOnPassword);
        endPoint ??= DefaultEndPoint;

        var instrumentation = new NetworkInstrumentation("wake_on_lan", endPoint.Address.ToString(), endPoint.Port);
        using var activity = instrumentation.StartActivity("WakeOnLan.Send", ActivityKind.Producer);

        using var socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        if (endPoint.AddressFamily == AddressFamily.InterNetwork)
        {
            socket.EnableBroadcast = true;
        }

        try
        {
            await socket.SendToAsync(packet, SocketFlags.None, endPoint, cancellationToken);
        }
        catch (SocketException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        instrumentation.Sent(packet.Length);
    }

    private static PhysicalAddress ParseMacAddress(string macAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(macAddress);

        if (!PhysicalAddress.TryParse(macAddress.Trim(), out var physicalAddress) ||
            physicalAddress.GetAddressBytes().Length != MacAddressLength)
        {
            throw new ArgumentException(
                $"'{macAddress}' is not a MAC address. Use a form such as 01-23-45-67-89-AB, 01:23:45:67:89:AB, 0123456789AB or 0123.4567.89AB.",
                nameof(macAddress));
        }

        return physicalAddress;
    }
}