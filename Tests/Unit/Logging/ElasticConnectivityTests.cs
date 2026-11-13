using Core.Logging.Elastic;

namespace Tests.Unit.Logging;

public class ElasticConnectivityTests
{
    private const string Elasticsearch8 = """{ "version": { "number": "8.15.3", "build_flavor": "default" }, "tagline": "You Know, for Search" }""";

    [Fact]
    public void Evaluate_Should_AcceptElasticsearch8()
    {
        Assert.True(ElasticConnectivity.Evaluate(200, Elasticsearch8).IsUsable);
    }

    [Fact]
    public void Evaluate_Should_AcceptNewerMajorVersions()
    {
        Assert.True(ElasticConnectivity.Evaluate(200, """{ "version": { "number": "9.1.0" } }""").IsUsable);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    public void Evaluate_Should_RejectFailedAuthentication(int status)
    {
        var result = ElasticConnectivity.Evaluate(status, "{}");

        Assert.False(result.IsUsable);
        Assert.Contains("authentication failed", result.Reason);
    }

    [Fact]
    public void Evaluate_Should_RejectOpenSearch()
    {
        var result = ElasticConnectivity.Evaluate(200, """{ "version": { "distribution": "opensearch", "number": "2.17.1" } }""");

        Assert.False(result.IsUsable);
        Assert.Contains("OpenSearch", result.Reason);
    }

    [Fact]
    public void Evaluate_Should_RejectElasticsearch7()
    {
        var result = ElasticConnectivity.Evaluate(200, """{ "version": { "number": "7.17.0" } }""");

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
        Assert.False(ElasticConnectivity.Evaluate(status, body).IsUsable);
    }

    [Fact]
    public void Check_Should_ExplainAMissingUrl()
    {
        var result = ElasticConnectivity.Check(" ");

        Assert.False(result.IsUsable);
        Assert.Equal("no Elastic URL is configured", result.Reason);
    }
}
