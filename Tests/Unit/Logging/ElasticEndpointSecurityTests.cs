using Core.Logging;
using Core.Logging.Elastic;

namespace Tests.Unit.Logging;

public sealed class ElasticEndpointSecurityTests
{
    [Theory]
    [InlineData(ElasticServerChoices.ON_CLOUD)]
    [InlineData(ElasticServerChoices.ON_LOCALHOST_INSECURE)]
    public void RemoteHttp_Should_BeRejectedBeforeAClientCanSendCredentials(ElasticServerChoices profile)
    {
        var config = new LoggingConfig
        {
            Elastic = new ElasticSection
            {
                Url = "http://example.invalid:9200",
                Username = "synthetic-user",
                Password = "synthetic-password"
            }
        };
        var error = Assert.Throws<InvalidOperationException>(() => ElasticClientFactory.Create(profile, config));
        Assert.Contains("HTTPS", error.Message);
        Assert.DoesNotContain(config.Elastic.Password!, error.ToString());
    }

    [Theory]
    [InlineData("http://127.0.0.1:9200")]
    [InlineData("https://example.invalid:9200")]
    public void SupportedEndpoints_Should_CreateAClientWithoutNetworkAccess(string url)
    {
        Assert.NotNull(ElasticClientFactory.Create(ElasticServerChoices.ON_CLOUD,
            new LoggingConfig { Elastic = new ElasticSection { Url = url } }));
    }

    [Fact]
    public void UrlCredentials_Should_BeRejectedWithoutRepeatingTheSecret()
    {
        var error = Assert.Throws<InvalidOperationException>(() => ElasticClientFactory.Create(ElasticServerChoices.ON_CLOUD,
            new LoggingConfig { Elastic = new ElasticSection { Url = "https://user:canary-secret@example.invalid" } }));
        Assert.DoesNotContain("canary", error.ToString());
    }
}
