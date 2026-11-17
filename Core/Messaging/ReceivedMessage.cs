using System.Text.Json;

namespace Core.Messaging;

/// <summary>A message seen by a broker listener (RabbitMQ, Kafka, …), in a broker-neutral shape.</summary>
public sealed record ReceivedMessage
{
    /// <summary>Queue, exchange/routing key or topic the message came from.</summary>
    public string Source { get; init; } = "";

    /// <summary>Routing key (RabbitMQ) or message key (Kafka), if any.</summary>
    public string? Key { get; init; }

    /// <summary>The body decoded as UTF-8.</summary>
    public string Body { get; init; } = "";

    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>The body deserialized from JSON (case-insensitive property names).</summary>
    public T? BodyAs<T>() => JsonSerializer.Deserialize<T>(Body, JsonOptions);
}
