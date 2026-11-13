using System.Runtime.CompilerServices;
using Core.Api;
using Core.Api.RestfulBooker;
using Core.Configuration;
using Core.Logging;
using Core.Testing;
using Tests.Fixtures;

namespace Tests.Infrastructure;
/// <summary>
/// Base for Restful Booker API tests. Each test gets its own <see cref="ApiClient"/>
/// (a Playwright APIRequestContext); wrap the body in <see cref="RunAsync"/>.
/// </summary>
public abstract class ApiTestBase : IClassFixture<ApiFixture>, IAsyncLifetime
{
    protected ApiTestBase(ApiFixture fixture)
    {
        Fixture = fixture;
        Logger = LoggingServiceFactory.CreateLogger("test-logs-{0:yyyy.MM.dd}");
    }

    protected ApiFixture Fixture { get; }
    protected ApiSettings Api => Fixture.Settings.RestfulBooker;
    protected ILoggingService Logger { get; }
    protected ApiClient Client { get; private set; } = null!;

    public async Task InitializeAsync() => Client = await ApiClient.CreateAsync(Fixture.Playwright, Api.BaseUrl);

    public async Task DisposeAsync()
    {
        if (Client != null)
        {
            await Client.DisposeAsync();
        }
    }

    protected Task<string> GetAuthTokenAsync() => AuthClient.GetTokenAsync(Client, Api.Username, Api.Password);

    protected Task RunAsync(Func<Task> test, [CallerMemberName] string testName = "") =>
        TestRunner.RunAsync(new TestRunInfo(Logger, "api", GetType(), testName), test);
}
