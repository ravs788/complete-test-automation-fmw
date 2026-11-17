using Core.Configuration;

namespace Tests.Fixtures;

/// <summary>
/// Shared state for one API test class: the settings. The HTTP transport (Playwright or HttpClient) is chosen with
/// <c>"Http"</c> in <c>providers.json</c> and created per test by <see cref="Core.Api.ApiClient.CreateAsync"/> (R41).
/// </summary>
public class ApiFixture
{
    public TestSettings Settings => ConfigurationProvider.Current;
}
