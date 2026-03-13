namespace Atc.Network.Vnc;

/// <summary>
/// Reconnect configurations for <see cref="VncClient"/>.
/// </summary>
public class VncClientReconnectConfig
{
    /// <summary>
    /// Enable auto-reconnect on unexpected disconnect.
    /// </summary>
    /// <remarks>
    /// Reconnect triggers when the connection is lost unexpectedly (e.g., server closes the socket).
    /// The full VNC sequence (TCP connect, RFB handshake, authenticate, initialize) is replayed.
    /// </remarks>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Gets or sets the retry interval in milliseconds.
    /// </summary>
    /// <remarks>
    /// If the <see cref="RetryInterval"/> and the <see cref="RetryMaxAttempts"/> is set to the
    /// defaults as a calculation example: 2sec * 1800 attempts, then the <see cref="VncClient"/> will
    /// try auto-reconnect within 1 hour, before it gives up on auto reconnection.
    /// </remarks>
    /// <returns>
    /// The retry interval value, in milliseconds. The default is 2000 (2 sec).
    /// </returns>
    public int RetryInterval { get; set; } = VncConstants.DefaultReconnectRetryInterval;

    /// <summary>
    /// Gets or sets the retry max attempts.
    /// </summary>
    /// <remarks>
    /// If the <see cref="RetryInterval"/> and the <see cref="RetryMaxAttempts"/> is set to the
    /// defaults as a calculation example: 2sec * 1800 attempts, then the <see cref="VncClient"/> will
    /// try auto-reconnect within 1 hour, before it gives up on auto reconnection.
    /// </remarks>
    /// <returns>
    /// The retry max attempts value.
    /// </returns>
    public int RetryMaxAttempts { get; set; } = VncConstants.DefaultReconnectRetryMaxAttempts;

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Enable)}: {Enable}, {nameof(RetryInterval)}: {RetryInterval}, {nameof(RetryMaxAttempts)}: {RetryMaxAttempts}";
}