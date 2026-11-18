using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Core.Messaging.Kafka;

namespace Tests.Unit.Messaging;

/// <summary>Runs against a real broker when KAFKA_BOOTSTRAP is set (e.g. localhost:9092).</summary>
[Trait("Category", "integration")]
public class KafkaListenerIntegrationTests
{
    private static string Bootstrap => Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP")!;

    [RequiresEnvironmentFact("KAFKA_BOOTSTRAP")]
    public async Task Listener_Should_ReceiveMessagesProducedAfterItStarted()
    {
        // Arrange
        var topic = $"test-topic-{Guid.NewGuid():N}";
        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = Bootstrap }).Build();
        await admin.CreateTopicsAsync([new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 }]);
        try
        {
            using var producer = new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = Bootstrap }).Build();
            await producer.ProduceAsync(topic, new Message<string, string> { Key = "before", Value = "produced before the listener" });

            // Arrange: start the listener after the old message was produced.
            await using var listener = await KafkaListener.StartAsync(Bootstrap, topic);
            var headers = new Headers { { "source", "unit-test"u8.ToArray() } };

            // Act: produce a new message after listener startup.
            await producer.ProduceAsync(topic, new Message<string, string> { Key = "booking-42", Value = """{ "id": 42 }""", Headers = headers });

            var message = await listener.WaitForAsync(m => m.Key == "booking-42", TimeSpan.FromSeconds(20));

            // Assert: the new message is received with its headers, while the old message is excluded.
            Assert.Equal("""{ "id": 42 }""", message.Body);
            Assert.Equal("unit-test", message.Headers["source"]);
            Assert.DoesNotContain(listener.Messages.Messages, m => m.Key == "before");
        }

        // Cleanup: delete the temporary topic.
        finally
        {
            await admin.DeleteTopicsAsync([topic]);
        }
    }
}
