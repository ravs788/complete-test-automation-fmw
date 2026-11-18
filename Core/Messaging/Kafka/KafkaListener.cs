using System.Runtime.ExceptionServices;
using System.Text;
using Confluent.Kafka;

namespace Core.Messaging.Kafka;

/// <summary>
/// Listens to a topic for the duration of a test (R37). It assigns itself every partition directly (no consumer-group
/// rebalancing) at the current end offsets, so it sees exactly the messages produced after it started, and it never
/// commits offsets, so nothing is left on the broker when it is disposed. Start it before triggering the behaviour
/// under test, then wait with <see cref="WaitForAsync"/>.
/// </summary>
public sealed class KafkaListener : IAsyncDisposable
{
    private readonly IConsumer<string?, string> _consumer;
    private readonly CancellationTokenSource _stop = new();
    private readonly Task _loop;

    internal KafkaListener(IConsumer<string?, string> consumer, string topic)
    {
        _consumer = consumer;
        Topic = topic;
        _loop = Task.Factory.StartNew(ConsumeLoop, TaskCreationOptions.LongRunning);
    }

    public string Topic { get; }

    /// <summary>The last non-fatal consume error, for diagnosing a listener that receives nothing.</summary>
    public string? LastError { get; private set; }

    public MessageCollector<ReceivedMessage> Messages { get; } = new();

    /// <summary>Completes when the listener stops; faults when consumption fails fatally.</summary>
    public Task Completion => _loop;

    /// <param name="bootstrapServers">e.g. <c>localhost:9092</c>.</param>
    /// <param name="topic">Topic to listen to (must exist).</param>
    /// <param name="fromBeginning">Also read messages produced before the listener started.</param>
    /// <param name="timeout">Limit for reading the topic's metadata and offsets (default 30 s).</param>
    public static Task<KafkaListener> StartAsync(string bootstrapServers, string topic, bool fromBeginning = false,
        TimeSpan? timeout = null)
    {
        var limit = timeout ?? TimeSpan.FromSeconds(30);
        var consumer = new ConsumerBuilder<string?, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"test-listener-{Guid.NewGuid():N}", // required by the client; unused (manual assignment, no commits)
            EnableAutoCommit = false,
        }).Build();

        try
        {
            using var admin = new DependentAdminClientBuilder(consumer.Handle).Build();
            var partitions = admin.GetMetadata(topic, limit).Topics.Single().Partitions;
            if (partitions.Count == 0)
            {
                throw new InvalidOperationException($"Kafka topic '{topic}' doesn't exist or has no partitions.");
            }

            // Pin each partition's start position now, so nothing produced after StartAsync returns can be skipped.
            consumer.Assign(partitions.Select(p =>
            {
                var partition = new TopicPartition(topic, p.PartitionId);
                var start = fromBeginning ? Offset.Beginning : consumer.QueryWatermarkOffsets(partition, limit).High;
                return new TopicPartitionOffset(partition, start);
            }));
        }
        catch
        {
            consumer.Dispose();
            throw;
        }

        return Task.FromResult(new KafkaListener(consumer, topic));
    }

    public async Task<ReceivedMessage> WaitForAsync(Func<ReceivedMessage, bool> match, TimeSpan timeout,
        string description = "a matching message", CancellationToken cancellationToken = default)
    {
        using var waitCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var wait = Messages.WaitForAsync(match, timeout, description, waitCancellation.Token);
        if (await Task.WhenAny(wait, _loop) == _loop)
        {
            waitCancellation.Cancel();
            try { await wait; } catch { /* the worker failure takes precedence over the abandoned wait */ }
            await _loop; // Propagate the actual worker failure rather than a misleading message timeout.
            throw new InvalidOperationException("Kafka listener has stopped.");
        }
        return await wait;
    }

    internal static ReceivedMessage ToMessage(ConsumeResult<string?, string> result)
    {
        // Broker-neutral Headers is a dictionary: repeated Kafka keys use the last value; null maps to empty.
        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        if (result.Message.Headers is not null)
        {
            foreach (var header in result.Message.Headers)
            {
                var bytes = header.GetValueBytes();
                headers[header.Key] = bytes is null ? "" : Encoding.UTF8.GetString(bytes);
            }
        }
        return new ReceivedMessage
        {
            Source = $"{result.Topic}[{result.Partition.Value}]@{result.Offset.Value}",
            Key = result.Message.Key,
            Body = result.Message.Value ?? "",
            Headers = headers
        };
    }

    private void ConsumeLoop()
    {
        while (!_stop.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromMilliseconds(200));
                if (result?.Message is { } message)
                {
                    Messages.Add(ToMessage(result));
                }
            }
            catch (KafkaException ex) when (!ex.Error.IsFatal)
            {
                // Transient errors (broker hiccup, leader change) are retried by the next Consume call.
                LastError = ex.Error.Reason;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stop.Cancel();
        ExceptionDispatchInfo? failure = null;
        try { await _loop; }
        catch (Exception ex) { failure = ExceptionDispatchInfo.Capture(ex); }
        try { _consumer.Close(); }
        catch (Exception ex) { failure ??= ExceptionDispatchInfo.Capture(ex); }
        try { _consumer.Dispose(); }
        catch (Exception ex) { failure ??= ExceptionDispatchInfo.Capture(ex); }
        finally { _stop.Dispose(); }
        failure?.Throw();
    }
}
