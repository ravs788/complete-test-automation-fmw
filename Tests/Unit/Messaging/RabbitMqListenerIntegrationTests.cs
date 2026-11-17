using System.Text;
using Core.Messaging.RabbitMq;
using RabbitMQ.Client;

namespace Tests.Unit.Messaging;

/// <summary>Runs against a real broker when RABBITMQ_URI is set (e.g. amqp://guest:guest@localhost:5672/).</summary>
[Trait("Category", "integration")]
public class RabbitMqListenerIntegrationTests
{
    private static string Uri => Environment.GetEnvironmentVariable("RABBITMQ_URI")!;

    [RequiresEnvironmentFact("RABBITMQ_URI")]
    public async Task Listener_Should_ReceiveMatchingMessages_And_RemoveItsQueue()
    {
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
                await channel.BasicPublishAsync(exchange, "booking.created", false, props, Encoding.UTF8.GetBytes("""{ "id": 42 }"""));
                await channel.BasicPublishAsync(exchange, "invoice.created", Encoding.UTF8.GetBytes("ignored"));

                var message = await listener.WaitForAsync(m => m.Key == "booking.created", TimeSpan.FromSeconds(10));

                Assert.Equal("""{ "id": 42 }""", message.Body);
                Assert.Equal("unit-test", message.Headers["source"]);
                Assert.DoesNotContain(listener.Messages.Messages, m => m.Key == "invoice.created");
            }

            // The listener's exclusive, auto-delete queue is gone once it's disposed.
            await using var check = await admin.CreateChannelAsync();
            await Assert.ThrowsAnyAsync<Exception>(() => check.QueueDeclarePassiveAsync(queue));
        }
        finally
        {
            await channel.ExchangeDeleteAsync(exchange);
        }
    }
}
