using Confluent.Kafka;
using Core.Messaging.Kafka;
using Tests.Unit.TestDoubles;

namespace Tests.Unit.Messaging;

public class KafkaCleanupTests
{
    private static ConsumeResult<string?, string> Result(string key, Headers headers) => new()
    {
        Topic = "test", Partition = 0, Offset = 1,
        Message = new Message<string?, string> { Key = key, Value = "body", Headers = headers }
    };

    [Fact]
    public async Task RepeatedAndNullHeaders_Should_NotStopLaterMessages()
    {
        // Arrange
        var records = new Queue<ConsumeResult<string?, string>>();
        records.Enqueue(Result("first", new Headers { { "source", "old"u8.ToArray() }, { "source", "latest"u8.ToArray() }, { "empty", null! } }));
        records.Enqueue(Result("second", new Headers()));
        var disposed = false;
        var consumer = InterfaceStub.Create<IConsumer<string?, string>>((m, _) => m.Name switch
        {
            "Consume" => records.TryDequeue(out var result) ? result : null,
            "Close" => null,
            "Dispose" => MarkDisposed(),
            _ => throw new NotSupportedException(m.Name)
        });
        object? MarkDisposed() { disposed = true; return null; }

        // Act: start the listener over the scripted consumer.
        var listener = new KafkaListener(consumer, "test");
        try
        {
            // Act: wait for the first message.
            var first = await listener.WaitForAsync(m => m.Key == "first", TimeSpan.FromSeconds(5));

            // Assert: repeated and null headers are normalized.
            Assert.Equal("latest", first.Headers["source"]);
            Assert.Equal("", first.Headers["empty"]);

            // Act & Assert: the next message remains available.
            Assert.Equal("second", (await listener.WaitForAsync(m => m.Key == "second", TimeSpan.FromSeconds(5))).Key);
        }

        // Cleanup: dispose the listener.
        finally { await listener.DisposeAsync(); }

        // Assert: the underlying consumer was disposed.
        Assert.True(disposed);
    }

    [Fact]
    public async Task WorkerFailure_Should_BeObservable_AndReleaseConsumer_EvenIfCloseFails()
    {
        // Arrange
        var original = new InvalidOperationException("consume failed");
        var disposed = false;
        var closed = false;
        var consumer = InterfaceStub.Create<IConsumer<string?, string>>((m, _) =>
        {
            if (m.Name == "Consume") throw original;
            if (m.Name == "Close") { closed = true; throw new IOException("close failed"); }
            if (m.Name == "Dispose") { disposed = true; return null; }
            throw new NotSupportedException(m.Name);
        });

        // Act
        var listener = new KafkaListener(consumer, "test");

        // Act & Assert: the worker failure is observable through completion and waiting.
        await Assert.ThrowsAsync<InvalidOperationException>(() => listener.Completion.WaitAsync(TimeSpan.FromSeconds(5)));
        var waitFailure = await Assert.ThrowsAsync<InvalidOperationException>(() => listener.WaitForAsync(_ => false, TimeSpan.FromSeconds(30)));

        // Assert: waiting preserves the original error.
        Assert.Same(original, waitFailure);

        // Act & Assert: disposal surfaces the original worker failure.
        var disposeFailure = await Assert.ThrowsAsync<InvalidOperationException>(() => listener.DisposeAsync().AsTask());

        // Assert: cleanup attempted both close and disposal.
        Assert.Same(original, disposeFailure);
        Assert.True(closed);
        Assert.True(disposed);
    }
}
