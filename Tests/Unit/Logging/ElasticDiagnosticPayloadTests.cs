using Core.Logging;
using Core.Logging.Elastic;
using Tests.Unit.Http;

namespace Tests.Unit.Logging;

public sealed class ElasticDiagnosticPayloadTests
{
    [Fact]
    public void Publisher_Should_SanitizeTheActualSerializedRequest_WithoutChangingTestMetadata()
    {
        // Arrange
        using var server = new LoopbackHttpServer
        {
            FixedResponseJson = """{"_index":"logs","_id":"synthetic","_version":1,"result":"created","_shards":{"total":1,"successful":1,"failed":0},"_seq_no":0,"_primary_term":1}"""
        };
        var client = ElasticClientFactory.Create(ElasticServerChoices.ON_CLOUD,
            new LoggingConfig { Elastic = new ElasticSection { Url = server.BaseUrl } });
        var publisher = new ElasticResultsPublisher(client);
        var metadata = new LogMetadata { Reason = "GET https://user:canary-password@example.invalid/items?token=canary-token failed" };

        // Act
        publisher.Publish(metadata, "logs");

        // Assert
        var payload = Assert.Single(server.RequestBodies);
        Assert.DoesNotContain("canary", payload);
        Assert.Contains("REDACTED", payload);
        Assert.Contains("items", payload);
        Assert.Contains("canary", metadata.Reason);
    }
}
