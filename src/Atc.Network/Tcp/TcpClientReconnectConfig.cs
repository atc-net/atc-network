namespace Atc.Network.Tcp;

/// <summary>
/// Reconnect configurations for <see cref="TcpClient"/>.
/// </summary>
public class TcpClientReconnectConfig
{
    /// <summary>
    /// Enable auto-reconnect then disconnect.
    /// </summary>
    /// <remarks>
    /// Disconnect happens 'on sender socket closed'.
    /// </remarks>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Gets or sets the retry interval in milliseconds.
    /// </summary>
    /// <remarks>
    /// If the <see cref="RetryInterval"/> and the <see cref="RetryMaxAttempts"/> is set to the
    /// defaults as a calculation example: 1sec * 3600 attempts, then the <see cref="TcpClient"/> will
    /// try auto-reconnect within 1hour, before it gives up on auto reconnection.
    /// </remarks>
    /// <returns>
    /// The retry interval value, in milliseconds. The default is 1000 (1 sec).
    /// </returns>
    public int RetryInterval { get; set; } = TcpConstants.DefaultReconnectRetryInterval;

    /// <summary>
    /// Gets or sets the retry max attempts.
    /// </summary>
    /// <remarks>
    /// If the <see cref="RetryInterval"/> and the <see cref="RetryMaxAttempts"/> is set to the
    /// defaults as a calculation example: 1sec * 3600 attempts, then the <see cref="TcpClient"/> will
    /// try auto-reconnect within 1hour, before it gives up on auto reconnection.
    /// </remarks>
    /// <returns>
    /// The retry max attempts value.
    /// </returns>
    public int RetryMaxAttempts { get; set; } = TcpConstants.DefaultReconnectRetryMaxAttempts;

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Enable)}: {Enable}, {nameof(RetryInterval)}: {RetryInterval}";
}