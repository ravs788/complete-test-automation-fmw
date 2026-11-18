using System.Text;
using Core.Messaging.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Tests.Unit.Messaging;

/// <summary>Runs against a real broker when RABBITMQ_URI is set (e.g. amqp://guest:guest@localhost:5672/).</summary>
[Trait("Category", "integration")]
public class RabbitMqListenerIntegrationTests
{
    private static string Uri => Environment.GetEnvironmentVariable("RABBITMQ_URI")!;

    [RequiresEnvironmentFact("RABBITMQ_URI")]
    public async Task Listener_Should_ReceiveMatchingMessages_And_RemoveItsQueue()
    {
        // Arrange
        var exchange = $"test-exchange-{Guid.NewGuid():N}";
        await using var admin = await new ConnectionFactory { Uri = new Uri(Uri) }.CreateConnectionAsync();
        await using var channel = await admin.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: false, autoDelete: false);
        try
        {
            string queue;
            await using (var listener = await RabbitMqListener.StartAsync(Uri, exchange, "booking.*"))
            {
                queue = listener.Queue;
                var props = new BasicProperties { Headers = new Dictionary<string, object?> { ["source"] = "unit-test" } };

                // Act: publish matching and non-matching messages.
                await channel.BasicPublishAsync(exchange, "booking.created", false, props, Encoding.UTF8.GetBytes("""{ "id": 42 }"""));
                await channel.BasicPublishAsync(exchange, "invoice.created", Encoding.UTF8.GetBytes("ignored"));

                var message = await listener.WaitForAsync(m => m.Key == "booking.created", TimeSpan.FromSeconds(10));

                // Assert: only the matching message is collected with its headers.
                Assert.Equal("""{ "id": 42 }""", message.Body);
                Assert.Equal("unit-test", message.Headers["source"]);
                Assert.DoesNotContain(listener.Messages.Messages, m => m.Key == "invoice.created");
            }

            // The listener's exclusive, auto-delete queue is gone once it's disposed.

            // Arrange: open a channel to check the disposed listener's queue.
            await using var check = await admin.CreateChannelAsync();

            // Act & Assert: the listener's auto-delete queue is no longer available.
            var missing = await Assert.ThrowsAsync<OperationInterruptedException>(() => check.QueueDeclarePassiveAsync(queue));

            // Assert: the broker reports the queue as missing.
            Assert.Equal((ushort)404, missing.ShutdownReason?.ReplyCode);
        }

        // Cleanup: delete the temporary exchange.
        finally
        {
            await channel.ExchangeDeleteAsync(exchange);
        }
    }
}
