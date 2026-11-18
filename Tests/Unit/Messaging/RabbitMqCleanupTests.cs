using Core.Messaging.RabbitMq;
using RabbitMQ.Client;
using Tests.Unit.TestDoubles;

namespace Tests.Unit.Messaging;

public class RabbitMqCleanupTests
{
    [Theory]
    [InlineData("CreateChannelAsync", false)]
    [InlineData("QueueDeclareAsync", true)]
    [InlineData("QueueBindAsync", true)]
    [InlineData("BasicConsumeAsync", true)]
    public async Task StartupFailure_Should_ReleaseEveryAcquiredResource_AndPreserveOriginalError(string failingMethod, bool channelAcquired)
    {
        var calls = new List<string>();
        var original = new InvalidOperationException("startup failed");
        var channel = InterfaceStub.Create<IChannel>((m, _) =>
        {
            calls.Add("channel." + m.Name);
            if (m.Name == failingMethod) throw original;
            if (m.Name == "CloseAsync") throw new IOException("already closed after startup failure");
            return InterfaceStub.Completed(m.ReturnType, m.Name == "QueueDeclareAsync" ? (object)new QueueDeclareOk("temporary", 0, 0)
                : m.Name == "BasicConsumeAsync" ? "consumer-tag" : null);
        });
        var connection = InterfaceStub.Create<IConnection>((m, _) =>
        {
            calls.Add("connection." + m.Name);
            if (m.Name == failingMethod) throw original;
            return InterfaceStub.Completed(m.ReturnType, m.Name == "CreateChannelAsync" ? channel : null);
        });

        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => RabbitMqListener.StartAsync(connection, "missing"));

        Assert.Same(original, failure);
        Assert.Contains("connection.CloseAsync", calls);
        Assert.Contains("connection.DisposeAsync", calls);
        Assert.Equal(channelAcquired, calls.Contains("channel.DisposeAsync"));
    }

    [Fact]
    public async Task CancelledStartup_Should_DisposeTheAcquiredConnection()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var original = new OperationCanceledException(cancellation.Token);
        var disposed = false;
        var connection = InterfaceStub.Create<IConnection>((m, _) =>
        {
            if (m.Name == "CreateChannelAsync") throw original;
            if (m.Name == "DisposeAsync") disposed = true;
            return InterfaceStub.Completed(m.ReturnType);
        });

        var failure = await Assert.ThrowsAsync<OperationCanceledException>(() =>
            RabbitMqListener.StartAsync(connection, "exchange", cancellationToken: cancellation.Token));

        Assert.Same(original, failure);
        Assert.True(disposed);
    }

    [Fact]
    public async Task Disposal_Should_ReleaseConnection_WhenChannelCloseAndDisposeFail()
    {
        var calls = new List<string>();
        var original = new IOException("channel close failed");
        var channel = InterfaceStub.Create<IChannel>((m, _) =>
        {
            calls.Add("channel." + m.Name);
            if (m.Name == "CloseAsync") throw original;
            if (m.Name == "DisposeAsync") throw new IOException("channel dispose failed");
            return InterfaceStub.Completed(m.ReturnType, m.Name == "QueueDeclareAsync" ? (object)new QueueDeclareOk("temporary", 0, 0)
                : m.Name == "BasicConsumeAsync" ? "consumer-tag" : null);
        });
        var connection = InterfaceStub.Create<IConnection>((m, _) =>
        {
            calls.Add("connection." + m.Name);
            return InterfaceStub.Completed(m.ReturnType, m.Name == "CreateChannelAsync" ? channel : null);
        });
        var listener = await RabbitMqListener.StartAsync(connection, "exchange");

        var failure = await Assert.ThrowsAsync<IOException>(() => listener.DisposeAsync().AsTask());

        Assert.Same(original, failure);
        Assert.Contains("channel.DisposeAsync", calls);
        Assert.Contains("connection.CloseAsync", calls);
        Assert.Contains("connection.DisposeAsync", calls);
    }
}
