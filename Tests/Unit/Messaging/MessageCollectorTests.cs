using Core.Messaging;

namespace Tests.Unit.Messaging;

public class MessageCollectorTests
{
    [Fact]
    public async Task WaitForAsync_Should_ReturnAMessageThatArrivesLater()
    {
        // Arrange
        var collector = new MessageCollector<ReceivedMessage>();
        _ = Task.Run(async () =>
        {
            await Task.Delay(150);
            collector.Add(new ReceivedMessage { Key = "other", Body = "{}" });
            collector.Add(new ReceivedMessage { Key = "booking.created", Body = """{ "id": 7 }""" });
        });

        // Act
        var message = await collector.WaitForAsync(m => m.Key == "booking.created", TimeSpan.FromSeconds(5));

        // Assert
        Assert.Equal(7, message.BodyAs<Payload>()!.Id);
        Assert.Equal(2, collector.Messages.Count);
    }

    [Fact]
    public async Task WaitForAsync_Should_Timeout_With_TheDescription()
    {
        // Arrange
        var collector = new MessageCollector<ReceivedMessage>();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<TimeoutException>(() =>
            collector.WaitForAsync(_ => true, TimeSpan.FromMilliseconds(200), "an order.shipped event"));

        // Assert: the timeout identifies the awaited event.
        Assert.Contains("order.shipped", ex.Message);
    }

    private sealed record Payload(int Id);
}
