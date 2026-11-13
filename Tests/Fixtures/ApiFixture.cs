namespace Tests.Fixtures;

using Core.Configuration;
using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

/// <summary>One Playwright instance per API test class; no browser is launched.</summary>
public class ApiFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public TestSettings Settings => ConfigurationProvider.Current;

    public async Task InitializeAsync() => Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

    public Task DisposeAsync()
    {
        Playwright?.Dispose();
        return Task.CompletedTask;
    }
}
