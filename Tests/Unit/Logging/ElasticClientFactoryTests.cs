using Core.Logging;
using Core.Logging.Elastic;

namespace Tests.Unit.Logging;

public class ElasticClientFactoryTests
{
    private static LoggingConfig Config(string? url) => new() { Elastic = new ElasticSection { Url = url } };

    [Theory]
    [InlineData(ElasticServerChoices.ON_LOCALHOST_INSECURE, "http://localhost:9200", "http://localhost:9200")]
    [InlineData(ElasticServerChoices.ON_LOCALHOST_SECURE, "http://localhost:9200", "https://localhost:9200")]
    [InlineData(ElasticServerChoices.ON_CLOUD, "https://cluster.example:443", "https://cluster.example:443")]
    public void ResolveUrl_Should_UseHttps_OnlyForTheSecureLocalProfile(ElasticServerChoices profile, string configured, string expected)
    {
        Assert.Equal(expected, ElasticClientFactory.ResolveUrl(profile, Config(configured)));
    }

    [Fact]
    public void Create_Should_RequireAUrl_ForTheCloudProfile()
    {
        Assert.Throws<InvalidOperationException>(() => ElasticClientFactory.Create(ElasticServerChoices.ON_CLOUD, Config(null)));
    }

    [Fact]
    public void IsReachable_Should_ReturnFalse_When_UrlIsMissing()
    {
        Assert.False(ElasticConnectivity.IsReachable(null));
        Assert.False(ElasticConnectivity.IsReachable(" "));
    }
}
