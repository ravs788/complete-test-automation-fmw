using Core.Logging;
using Core.Logging.Console;
using Core.Logging.Elastic;

namespace Tests.Unit.Logging;

/// <summary>
/// The behaviour every logging and results-publishing provider must have (R40, R46): <c>TryCreate</c> never throws;
/// it either returns a working service or false with a reason (so the selector can fall back to console), and a
/// created service never fails the test it logs for.
/// </summary>
public abstract class LoggingProviderContractTests
{
    protected abstract ILoggingProviderFactory Loggers { get; }

    protected abstract IResultsPublisherFactory Publishers { get; }

    /// <summary>Settings under which this provider may be unavailable (e.g. a backend that isn't running).</summary>
    protected virtual LoggingConfig Config => new();

    [Fact]
    public void Logger_Should_BeCreated_OrDeclined_WithAReason()
    {
        if (Loggers.TryCreate(Config, "contract-logs-{0:yyyy.MM.dd}", out var logger, out var reason))
        {
            logger.Info("contract info");
            logger.Error("contract error");
            logger.Debug("contract debug");
        }
        else
        {
            Assert.False(string.IsNullOrWhiteSpace(reason));
        }
    }

    [Fact]
    public void Publisher_Should_BeCreated_OrDeclined_WithAReason()
    {
        if (Publishers.TryCreate(Config, out var publisher, out var reason))
        {
            publisher.Publish(new LogMetadata { ProjectName = "contract", TestMethodName = "Publisher", Status = "Passed" });
        }
        else
        {
            Assert.False(string.IsNullOrWhiteSpace(reason));
        }
    }
}

public sealed class ConsoleLoggingProviderContractTests : LoggingProviderContractTests
{
    protected override ILoggingProviderFactory Loggers { get; } = new ConsoleLoggingProviderFactory();

    protected override IResultsPublisherFactory Publishers { get; } = new ConsoleResultsPublisherFactory();
}

/// <summary>Elasticsearch pointed at a closed loopback port: must decline with a reason, not throw.</summary>
public sealed class ElasticLoggingProviderContractTests : LoggingProviderContractTests
{
    protected override ILoggingProviderFactory Loggers { get; } = new ElasticLoggingProviderFactory();

    protected override IResultsPublisherFactory Publishers { get; } = new ElasticResultsPublisherFactory();

    protected override LoggingConfig Config => new() { Elastic = new ElasticSection { Url = "http://127.0.0.1:1" } };
}
