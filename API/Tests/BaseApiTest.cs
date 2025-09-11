using Core.Utilities;
using API.Utilities;

namespace API.Tests
{
    public abstract class BaseApiTest
    {
        protected ILoggingService Logger { get; private set; }
        protected ApiClient _client { get; private set; }
        protected DateTime _testStartTime;

        protected BaseApiTest()
        {
            var logConfig = LoggingConfig.Load();
            Logger = new ElasticLoggingService();
            Logger.Configure("test-logs-{0:yyyy.MM.dd}", logConfig.Username, logConfig.Password, logConfig.ElasticUrl);
        }

        [NUnit.Framework.SetUp]
        public virtual void SetUp()
        {
            _testStartTime = DateTime.Now;
            Logger.Info($"[SetUp] Starting API test '{NUnit.Framework.TestContext.CurrentContext.Test.Name}'");
            _client = new ApiClient();
        }

        [NUnit.Framework.TearDown]
        public virtual void TearDown()
        {
            DateTime endTime = DateTime.Now;
            Allure.Net.Commons.AllureLifecycle.Instance.UpdateTestCase(tc =>
            {
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "Start Time",
                    value = _testStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "End Time",
                    value = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "Duration (s)",
                    value = (endTime - _testStartTime).TotalSeconds.ToString("F3")
                });
            });

            var ctx = NUnit.Framework.TestContext.CurrentContext;
            Logger.Info($"[TearDown] Finished API test '{ctx.Test.Name}' | Outcome: {ctx.Result.Outcome.Status} | Duration(s): {(endTime - _testStartTime).TotalSeconds:F3}");
            if (ctx.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                Logger.Error($"[TearDown] Failure details: {ctx.Result.Message}");
            }

            // Publish result to Elasticsearch as a single document
            var metadata = new LogMetadata
            {
                ProjectName = "api",
                TestClassName = ctx.Test.ClassName ?? string.Empty,
                TestMethodName = ctx.Test.MethodName ?? ctx.Test.Name,
                Status = ctx.Result.Outcome.Status.ToString(),
                Duration = (endTime - _testStartTime).TotalSeconds.ToString("F3"),
                Reason = ctx.Result.Message ?? string.Empty,
                RunTime = endTime.ToString("o"),
                RunName = ctx.Test.FullName ?? ctx.Test.Name,
                TriggeredBy = System.Environment.UserName,
                Browser = string.Empty,
                StartTime = _testStartTime,
                EndTime = endTime
            };
            try
            {
                try
                {
                    PublishResults.ToElastic(metadata);
                }
                catch (System.Exception ex)
                {
                    NUnit.Framework.TestContext.Progress.WriteLine($"[Elastic] Publish failed: {ex.Message}");
                }
            }
            catch (System.Exception ex)
            {
                NUnit.Framework.TestContext.Progress.WriteLine($"[Elastic] Publish failed: {ex.Message}");
            }

            _client?.Dispose();
        }
    }
}
