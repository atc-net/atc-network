namespace Atc.Network;

/// <summary>
/// The async-stream readers of a client's received data (ReadAllAsync). Each reader has its own bounded channel;
/// the receive loop waits while a reader's channel is full, which gives back-pressure.
/// </summary>
internal sealed class ReceivedDataReaders
{
    /// <summary>
    /// The number of items each reader buffers before the receive loop waits for it.
    /// </summary>
    public const int Capacity = 256;

    private readonly ConcurrentDictionary<Channel<ReadOnlyMemory<byte>>, bool> channels = new();
    private volatile bool isCompleted;

    public async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<ReadOnlyMemory<byte>>(new BoundedChannelOptions(Capacity)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait,
        });

        channels.TryAdd(channel, value: true);
        if (isCompleted)
        {
            channel.Writer.TryComplete();
        }

        try
        {
            await foreach (var data in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return data;
            }
        }
        finally
        {
            // Completing the channel releases a receive loop that waits for this reader.
            channels.TryRemove(channel, out _);
            channel.Writer.TryComplete();
        }
    }

    /// <summary>
    /// Passes received data to every reader, waiting while a reader's buffer is full.
    /// </summary>
    /// <param name="data">The received data.</param>
    /// <param name="cancellationToken">The receive loop's cancellationToken.</param>
    public async Task WriteAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken)
    {
        foreach (var channel in channels.Keys)
        {
            try
            {
                await channel.Writer.WriteAsync(data, cancellationToken);
            }
            catch (ChannelClosedException)
            {
                // The reader stopped reading.
            }
        }
    }

    /// <summary>
    /// Ends every stream, for example when the client is disposed. Items already buffered are still read.
    /// </summary>
    public void CompleteAll()
    {
        isCompleted = true;
        foreach (var channel in channels.Keys)
        {
            channel.Writer.TryComplete();
        }
    }
}