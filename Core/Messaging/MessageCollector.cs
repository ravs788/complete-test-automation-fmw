using System.Collections.Concurrent;
using Core.Resilience;

namespace Core.Messaging;

/// <summary>
/// Collects messages delivered by a listener on a background thread and lets a test wait for one that matches
/// (R37). Thread-safe. Broker add-ons (<c>Core.Messaging.RabbitMq</c>, <c>Core.Messaging.Kafka</c>) feed it.
/// </summary>
public sealed class MessageCollector<T>
{
    private readonly ConcurrentQueue<T> _messages = new();

    /// <summary>Everything received so far, oldest first.</summary>
    public IReadOnlyList<T> Messages => _messages.ToArray();

    public void Add(T message) => _messages.Enqueue(message);

    /// <summary>
    /// Waits until a received message matches <paramref name="match"/> (including ones that arrived earlier) and
    /// returns the first match; throws <see cref="TimeoutException"/> after <paramref name="timeout"/>.
    /// </summary>
    public async Task<T> WaitForAsync(Func<T, bool> match, TimeSpan timeout, string description = "a matching message",
        CancellationToken cancellationToken = default)
    {
        T found = default!;
        await Poll.UntilAsync(() =>
        {
            foreach (var message in _messages)
            {
                if (match(message))
                {
                    found = message;
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }, timeout, TimeSpan.FromMilliseconds(100), description, cancellationToken);
        return found;
    }
}
