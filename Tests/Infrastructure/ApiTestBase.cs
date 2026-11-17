using System.Runtime.CompilerServices;
using Core.Api;
using Core.Configuration;
using Core.Logging;
using Core.Testing;
using Domain.Api.RestfulBooker;
using Domain.Models.RestfulBooker;
using Tests.Fixtures;

namespace Tests.Infrastructure;

/// <summary>
/// Base for Restful Booker API tests. Each test gets its own <see cref="ApiClient"/> (on the HTTP transport selected
/// in providers.json); wrap the body in <see cref="RunAsync"/>. Bookings created through
/// <see cref="CreateBookingAsync"/> are deleted after the test.
/// </summary>
public abstract class ApiTestBase : IClassFixture<ApiFixture>, IAsyncLifetime
{
    protected ApiTestBase(ApiFixture fixture)
    {
        Fixture = fixture;
        Logger = LoggingServiceFactory.CreateLogger("api-logs-{0:yyyy.MM.dd}");
    }

    protected ApiFixture Fixture { get; }
    protected ApiSettings Api => Fixture.Settings.RestfulBooker;
    protected ILoggingService Logger { get; }
    protected ApiClient Client { get; private set; } = null!;

    /// <summary>Things this test created; removed after the test (bookings are registered automatically).</summary>
    protected CleanupTracker Cleanup { get; } = new();

    private Task<IReadOnlyDictionary<string, string>>? _cleanupAuth;

    public async Task InitializeAsync() =>
        Client = await ApiClient.CreateAsync(Api.BaseUrl, Fixture.Settings.Resilience.Http, message => Logger.Info(message));

    public async Task DisposeAsync()
    {
        if (Client == null)
        {
            return;
        }

        await Cleanup.RunAsync(message => Logger.Error(message));
        await Client.DisposeAsync();
    }

    /// <summary>
    /// POSTs <paramref name="booking"/> and records its ID, so the booking is deleted after the test
    /// (the demo API is shared, so tests shouldn't leave data behind).
    /// </summary>
    protected async Task<CreatedBooking> CreateBookingAsync(Booking booking)
    {
        var created = await Client.PostAsync<CreatedBooking>("booking", booking)
                      ?? throw new InvalidOperationException("POST booking returned no body.");
        var id = created.BookingId;
        Cleanup.Add($"DELETE booking/{id}", async () =>
        {
            // One token for all of this test's cleanups.
            var auth = await (_cleanupAuth ??= GetAuthHeadersAsync());
            var response = await Client.SendRawAsync("DELETE", $"booking/{id}", headers: auth);
            // 404/405: the test already deleted it.
            Logger.Info($"[Cleanup] DELETE booking/{id} -> {response.Status}");
        });
        return created;
    }

    /// <summary>Headers that authenticate PUT, PATCH and DELETE (a fresh Restful Booker token).</summary>
    protected Task<IReadOnlyDictionary<string, string>> GetAuthHeadersAsync() =>
        AuthClient.GetAuthHeadersAsync(Client, Api.Username, Api.Password);

    protected Task RunAsync(Func<Task> test, [CallerMemberName] string testName = "") =>
        TestRunner.RunAsync(new TestRunInfo(Logger, "api", GetType(), testName), test);
}
