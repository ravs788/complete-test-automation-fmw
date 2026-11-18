using Core.Logging.Elastic;

namespace Tests.Unit.Logging;

public class ElasticConnectivityTests
{
    private const string Elasticsearch8 = """{ "version": { "number": "8.15.3", "build_flavor": "default" }, "tagline": "You Know, for Search" }""";

    [Fact]
    public void Evaluate_Should_AcceptElasticsearch8()
    {
        // Arrange: use the Elasticsearch 8 response fixture.
        // Act & Assert
        Assert.True(ElasticConnectivity.Evaluate(200, Elasticsearch8).IsUsable);
    }

    [Fact]
    public void Evaluate_Should_AcceptNewerMajorVersions()
    {
        // Arrange: use an Elasticsearch 9 version response.
        // Act & Assert
        Assert.True(ElasticConnectivity.Evaluate(200, """{ "version": { "number": "9.1.0" } }""").IsUsable);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    public void Evaluate_Should_RejectFailedAuthentication(int status)
    {
        // Arrange: the theory supplies a failed authentication status.
        // Act
        var result = ElasticConnectivity.Evaluate(status, "{}");

        // Assert
        Assert.False(result.IsUsable);
        Assert.Contains("authentication failed", result.Reason);
    }

    [Fact]
    public void Evaluate_Should_RejectOpenSearch()
    {
        // Arrange: use an OpenSearch version response.
        // Act
        var result = ElasticConnectivity.Evaluate(200, """{ "version": { "distribution": "opensearch", "number": "2.17.1" } }""");

        // Assert
        Assert.False(result.IsUsable);
        Assert.Contains("OpenSearch", result.Reason);
    }

    [Fact]
    public void Evaluate_Should_RejectElasticsearch7()
    {
        // Arrange: use an unsupported Elasticsearch 7 version response.
        // Act
        var result = ElasticConnectivity.Evaluate(200, """{ "version": { "number": "7.17.0" } }""");

        // Assert
        Assert.False(result.IsUsable);
        Assert.Contains("too old", result.Reason);
    }

    [Theory]
    [InlineData(200, "<html>not elasticsearch</html>")]
    [InlineData(200, "{}")]
    [InlineData(500, "")]
    [InlineData(404, "")]
    public void Evaluate_Should_RejectAnythingElse(int status, string body)
    {
        // Arrange: the theory supplies an invalid status or response body.
        // Act & Assert
        Assert.False(ElasticConnectivity.Evaluate(status, body).IsUsable);
    }

    [Fact]
    public void Check_Should_ExplainAMissingUrl()
    {
        // Arrange: use a blank Elastic URL.
        // Act
        var result = ElasticConnectivity.Check(" ");

        // Assert
        Assert.False(result.IsUsable);
        Assert.Equal("no Elastic URL is configured", result.Reason);
    }
}
