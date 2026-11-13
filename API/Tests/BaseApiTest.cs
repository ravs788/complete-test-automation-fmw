using System.Runtime.CompilerServices;
using Core.Utilities;
using API.Utilities;

namespace API.Tests
{
    /// <summary>
    /// Base class for API tests. xUnit creates a new instance per test, so the constructor
    /// is the per-test setup and <see cref="Dispose"/> the per-test teardown.
    /// Wrap each test body in <see cref="RunAsync"/> to get logging, Allure timing and results publishing.
    /// </summary>
    public abstract class BaseApiTest : IDisposable
    {
        protected ILoggingService Logger { get; }
        protected ApiClient _client { get; }

        protected BaseApiTest()
        {
            Logger = LoggingServiceFactory.CreateLogger("test-logs-{0:yyyy.MM.dd}");
            _client = new ApiClient();
        }

        protected Task RunAsync(Func<Task> test, [CallerMemberName] string testName = "") =>
            TestRunner.RunAsync(new TestRunInfo(Logger, "api", GetType(), testName), test);

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
