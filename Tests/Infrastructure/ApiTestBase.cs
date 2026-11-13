using System.Runtime.CompilerServices;
using Core.Api;
using Core.Api.RestfulBooker;
using Core.Configuration;
using Core.Logging;
using Core.Models.RestfulBooker;
using Core.Testing;
using Tests.Fixtures;

namespace Tests.Infrastructure;

/// <summary>
/// Base for Restful Booker API tests. Each test gets its own <see cref="ApiClient"/>
/// (a Playwright APIRequestContext); wrap the body in <see cref="RunAsync"/>. Bookings created through
/// <see cref="CreateBookingAsync"/> are deleted after the test.
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

    private readonly List<int> _createdBookingIds = [];

    public async Task InitializeAsync() => Client = await ApiClient.CreateAsync(Fixture.Playwright, Api.BaseUrl);

    public async Task DisposeAsync()
    {
        if (Client == null)
        {
            return;
        }

        await DeleteCreatedBookingsAsync();
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
        _createdBookingIds.Add(created.BookingId);
        return created;
    }

    private async Task DeleteCreatedBookingsAsync()
    {
        if (_createdBookingIds.Count == 0)
        {
            return;
        }

        try
        {
            var auth = await GetAuthHeadersAsync();
            foreach (var id in _createdBookingIds)
            {
                var response = await Client.SendRawAsync("DELETE", $"booking/{id}", headers: auth);
                // 404/405: the test already deleted it.
                Logger.Info($"[Cleanup] DELETE booking/{id} -> {response.Status}");
            }
        }
        catch (Exception ex)
        {
            // Cleanup must never fail the test.
            Logger.Error($"[Cleanup] Could not delete bookings {string.Join(", ", _createdBookingIds)}: {ex.Message}");
        }
    }

    /// <summary>Headers that authenticate PUT, PATCH and DELETE (a fresh Restful Booker token).</summary>
    protected Task<IReadOnlyDictionary<string, string>> GetAuthHeadersAsync() =>
        AuthClient.GetAuthHeadersAsync(Client, Api.Username, Api.Password);

    protected Task RunAsync(Func<Task> test, [CallerMemberName] string testName = "") =>
        TestRunner.RunAsync(new TestRunInfo(Logger, "api", GetType(), testName), test);
}
