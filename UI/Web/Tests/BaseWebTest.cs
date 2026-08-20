using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using Core.Utilities;
using UI.Web.Utilities;
using Allure.Net.Commons;
using OpenQA.Selenium.Remote;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace UI.Web
{
    public abstract class BaseWebTest
    {
        protected IWebDriver? Driver { get; private set; }
        protected IScreenshotHelper? ScreenshotHelper { get; private set; }
        protected ILoggingService Logger { get; private set; }

        private DateTime _testStartTime;
        private Process? _localDriverProcess;

        public BaseWebTest()
        {
            Logger = LoggingServiceFactory.CreateLogger("ui-web-logs-{0:yyyy.MM.dd}");
        }

        [SetUp]
        public virtual void SetUp()
        {
            _testStartTime = DateTime.Now;
            EnsureLocalWebDriverBypassesProxy();
            DisableProxyForWebDriverClient();

            var config = Core.Utilities.ConfigLoader.Load<UI.Web.Utilities.ConfigSettings>();
            string browser = "firefox"; // default
            var headless = config.Headless || IsRunningInGitHubActions();

            if (TestContext.CurrentContext.Test.Arguments.Length > 0 && TestContext.CurrentContext.Test.Arguments[0] is string arg)
                browser = arg.ToLowerInvariant();

            bool useGrid = !string.IsNullOrWhiteSpace(config.GridUrl);
            Uri? remoteUri = useGrid ? new Uri(config.GridUrl) : null;
            var commandTimeout = TimeSpan.FromSeconds(config.DriverCommandTimeoutSec <= 0 ? 60 : config.DriverCommandTimeoutSec);

            Logger.Info($"[SetUp] Starting test '{TestContext.CurrentContext.Test.Name}' on browser '{browser}' (Headless: {headless})");

            switch (browser)
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                    chromeOptions.AddUserProfilePreference("credentials_enable_Service", false);
                    chromeOptions.AddUserProfilePreference("profile.password_manager_leak_detection", false);
                    if (headless)
                    {
                        chromeOptions.AddArgument("--headless=new");
                        chromeOptions.AddArgument("--window-size=1920,1080");
                    }
                    if (useGrid)
                    {
                        var commandExecutor = new HttpCommandExecutor(remoteUri!, commandTimeout);
                        Driver = new RemoteWebDriver(commandExecutor, chromeOptions.ToCapabilities());
                    }
                    else
                    {
                        Driver = CreateLocalWebDriver(
                            config.ChromeDriverPath,
                            "chromedriver",
                            chromeOptions.ToCapabilities(),
                            commandTimeout,
                            () => new ChromeDriver(CreateChromeService(config, commandTimeout), chromeOptions, commandTimeout));
                    }
                    break;
                case "firefox":
                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                    if (!string.IsNullOrWhiteSpace(config.FirefoxBinaryPath))
                    {
                        firefoxOptions.BinaryLocation = config.FirefoxBinaryPath;
                    }
                    if (headless)
                    {
                        firefoxOptions.AddArgument("--headless");
                        firefoxOptions.AddArgument("--width=1920");
                        firefoxOptions.AddArgument("--height=1080");
                    }
                    if (useGrid)
                    {
                        var commandExecutor = new HttpCommandExecutor(remoteUri!, commandTimeout);
                        Driver = new RemoteWebDriver(commandExecutor, firefoxOptions.ToCapabilities());
                    }
                    else
                    {
                        Driver = CreateLocalWebDriver(
                            config.FirefoxDriverPath,
                            "geckodriver",
                            firefoxOptions.ToCapabilities(),
                            commandTimeout,
                            () => new FirefoxDriver(CreateFirefoxService(config, commandTimeout), firefoxOptions, commandTimeout));
                    }
                    break;
                case "edge":
                    var edgeOptions = new EdgeOptions();
                    edgeOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                    if (headless)
                    {
                        edgeOptions.AddArgument("--headless=new");
                        edgeOptions.AddArgument("--window-size=1920,1080");
                    }
                    if (useGrid)
                    {
                        var commandExecutor = new HttpCommandExecutor(remoteUri!, commandTimeout);
                        Driver = new RemoteWebDriver(commandExecutor, edgeOptions.ToCapabilities());
                    }
                    else
                    {
                        Driver = CreateLocalWebDriver(
                            config.EdgeDriverPath,
                            "msedgedriver",
                            edgeOptions.ToCapabilities(),
                            commandTimeout,
                            () => new EdgeDriver(CreateEdgeService(config, commandTimeout), edgeOptions, commandTimeout));
                    }
                    break;
                default:
                    var defaultOptions = new FirefoxOptions();
                    defaultOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                    if (!string.IsNullOrWhiteSpace(config.FirefoxBinaryPath))
                    {
                        defaultOptions.BinaryLocation = config.FirefoxBinaryPath;
                    }
                    if (headless)
                    {
                        defaultOptions.AddArgument("--headless");
                        defaultOptions.AddArgument("--width=1920");
                        defaultOptions.AddArgument("--height=1080");
                    }
                    if (useGrid)
                    {
                        var commandExecutor = new HttpCommandExecutor(remoteUri!, commandTimeout);
                        Driver = new RemoteWebDriver(commandExecutor, defaultOptions.ToCapabilities());
                    }
                    else
                    {
                        Driver = CreateLocalWebDriver(
                            config.FirefoxDriverPath,
                            "geckodriver",
                            defaultOptions.ToCapabilities(),
                            commandTimeout,
                            () => new FirefoxDriver(CreateFirefoxService(config, commandTimeout), defaultOptions, commandTimeout));
                    }
                    break;
            }
            Driver!.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(config.PageLoadTimeoutSec <= 0 ? 30 : config.PageLoadTimeoutSec);
            Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(config.ScriptTimeoutSec <= 0 ? 30 : config.ScriptTimeoutSec);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Math.Max(config.ImplicitWaitTimeoutSec, 0));

            if (!headless)
            {
                Driver.Manage().Window.Maximize();
            }

            try
            {
                Driver.Navigate().GoToUrl(config.BaseUrl);
            }
            catch (WebDriverTimeoutException ex)
            {
                Logger.Error($"[SetUp] Timed out navigating to {config.BaseUrl}: {ex.Message}");
                throw;
            }
            Logger.Info($"[SetUp] Browser ready and navigated to {config.BaseUrl}");

            ScreenshotHelper = Driver != null
                ? new Utilities.AllureScreenshotHelper(Driver)
                : null;

            // Append browser name to Allure test case name (for reporting clarity)
            AllureLifecycle.Instance.UpdateTestCase(x => x.name = $"{x.name} ({browser})");
        }

        private IWebDriver? CreateLocalWebDriver(
            string configuredDriverPath,
            string executableName,
            ICapabilities capabilities,
            TimeSpan commandTimeout,
            Func<IWebDriver> seleniumManagerFactory)
        {
            var driverPath = ResolveDriverPath(configuredDriverPath, executableName);
            if (string.IsNullOrWhiteSpace(driverPath))
            {
                Logger.Info($"[SetUp] No local '{executableName}' executable was found; using Selenium Manager.");
                return seleniumManagerFactory();
            }

            var port = FindFreeLoopbackPort();
            var serviceUri = new Uri($"http://127.0.0.1:{port}");
            _localDriverProcess = StartLocalDriverProcess(driverPath, executableName, port);

            try
            {
                WaitForDriverStatusAsync(_localDriverProcess, new Uri(serviceUri, "/status"), commandTimeout)
                    .GetAwaiter()
                    .GetResult();

                var commandExecutor = new HttpCommandExecutor(serviceUri, commandTimeout);
                return new RemoteWebDriver(commandExecutor, capabilities);
            }
            catch
            {
                StopLocalDriverProcess();
                throw;
            }
        }

        private static bool IsRunningInGitHubActions() =>
            string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase);

        private static Process StartLocalDriverProcess(string driverPath, string executableName, int port)
        {
            var arguments = executableName.Equals("geckodriver", StringComparison.OrdinalIgnoreCase)
                ? $"--port {port}"
                : $"--port={port}";

            var startInfo = new ProcessStartInfo
            {
                FileName = driverPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            ApplyProxyBypass(startInfo.Environment);
            return Process.Start(startInfo)
                ?? throw new WebDriverException($"Could not start WebDriver process '{driverPath}'.");
        }

        private static int FindFreeLoopbackPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }

        private static async Task WaitForDriverStatusAsync(Process process, Uri statusUri, TimeSpan timeout)
        {
            using var handler = new HttpClientHandler { UseProxy = false };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(1) };
            var deadline = DateTime.UtcNow.Add(timeout);

            while (DateTime.UtcNow < deadline)
            {
                if (process.HasExited)
                {
                    throw new WebDriverException($"WebDriver process exited before it became ready. ExitCode={process.ExitCode}");
                }

                try
                {
                    using var response = await client.GetAsync(statusUri);
                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
                catch (HttpRequestException)
                {
                    // The driver process is still starting.
                }
                catch (TaskCanceledException)
                {
                    // Keep polling until the outer startup timeout expires.
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }

            throw new WebDriverException($"WebDriver process did not become ready at {statusUri} within {timeout.TotalSeconds:F0} seconds.");
        }

        private static ChromeDriverService CreateChromeService(UI.Web.Utilities.ConfigSettings config, TimeSpan initializationTimeout)
        {
            var driverPath = ResolveDriverPath(config.ChromeDriverPath, "chromedriver");
            var service = CreateDriverService(
                driverPath,
                ChromeDriverService.CreateDefaultService,
                ChromeDriverService.CreateDefaultService);
            service.InitializationTimeout = initializationTimeout;
            return service;
        }

        private static FirefoxDriverService CreateFirefoxService(UI.Web.Utilities.ConfigSettings config, TimeSpan initializationTimeout)
        {
            var driverPath = ResolveDriverPath(config.FirefoxDriverPath, "geckodriver");
            var service = CreateDriverService(
                driverPath,
                FirefoxDriverService.CreateDefaultService,
                FirefoxDriverService.CreateDefaultService);
            service.InitializationTimeout = initializationTimeout;
            return service;
        }

        private static EdgeDriverService CreateEdgeService(UI.Web.Utilities.ConfigSettings config, TimeSpan initializationTimeout)
        {
            var driverPath = ResolveDriverPath(config.EdgeDriverPath, "msedgedriver");
            var service = CreateDriverService(
                driverPath,
                EdgeDriverService.CreateDefaultService,
                EdgeDriverService.CreateDefaultService);
            service.InitializationTimeout = initializationTimeout;
            return service;
        }

        private static TService CreateDriverService<TService>(
            string? driverPath,
            Func<TService> createDefaultService,
            Func<string, string, TService> createServiceFromPath)
            where TService : DriverService
        {
            if (string.IsNullOrWhiteSpace(driverPath))
            {
                return createDefaultService();
            }

            return createServiceFromPath(
                Path.GetDirectoryName(driverPath)!,
                Path.GetFileName(driverPath));
        }

        private static string? ResolveDriverPath(string configuredPath, string executableName)
        {
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return configuredPath;
            }

            var executableFileName = OperatingSystem.IsWindows() ? $"{executableName}.exe" : executableName;
            var outputDriverPath = Path.Combine(AppContext.BaseDirectory, executableFileName);
            if (File.Exists(outputDriverPath))
            {
                return outputDriverPath;
            }

            var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrWhiteSpace(homeDirectory))
            {
                return null;
            }

            var seleniumCache = Path.Combine(homeDirectory, ".cache", "selenium", executableName);
            if (!Directory.Exists(seleniumCache))
            {
                return null;
            }

            return Directory
                .EnumerateFiles(seleniumCache, executableFileName, SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Select(file => file.FullName)
                .FirstOrDefault();
        }

        private static void EnsureLocalWebDriverBypassesProxy()
        {
            var loopbackHosts = new[] { "localhost", "127.0.0.1", "::1" };
            EnsureProxyBypass("NO_PROXY", loopbackHosts);
            EnsureProxyBypass("no_proxy", loopbackHosts);
        }

        private static void DisableProxyForWebDriverClient()
        {
            foreach (var variableName in new[] { "HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "http_proxy", "https_proxy", "all_proxy" })
            {
                Environment.SetEnvironmentVariable(variableName, null);
            }

            HttpClient.DefaultProxy = NoProxy.Instance;
            WebRequest.DefaultWebProxy = null;
        }

        private static void EnsureProxyBypass(string variableName, string[] hosts)
        {
            var currentValue = Environment.GetEnvironmentVariable(variableName);
            var entries = (currentValue ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            foreach (var host in hosts)
            {
                if (!entries.Contains(host, StringComparer.OrdinalIgnoreCase))
                {
                    entries.Add(host);
                }
            }

            Environment.SetEnvironmentVariable(variableName, string.Join(",", entries));
        }

        private static void ApplyProxyBypass(IDictionary<string, string?> environment)
        {
            var loopbackBypass = "localhost,127.0.0.1,::1";
            environment["NO_PROXY"] = MergeProxyBypass(environment.TryGetValue("NO_PROXY", out var noProxy) ? noProxy : null, loopbackBypass);
            environment["no_proxy"] = MergeProxyBypass(environment.TryGetValue("no_proxy", out var lowerNoProxy) ? lowerNoProxy : null, loopbackBypass);
        }

        private static string MergeProxyBypass(string? currentValue, string additionalValue)
        {
            var entries = (currentValue ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            foreach (var host in additionalValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!entries.Contains(host, StringComparer.OrdinalIgnoreCase))
                {
                    entries.Add(host);
                }
            }

            return string.Join(",", entries);
        }

        private void StopLocalDriverProcess()
        {
            if (_localDriverProcess == null)
            {
                return;
            }

            try
            {
                if (!_localDriverProcess.HasExited)
                {
                    _localDriverProcess.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Do not mask the original test result during cleanup.
            }
            finally
            {
                _localDriverProcess.Dispose();
                _localDriverProcess = null;
            }
        }

        private sealed class NoProxy : IWebProxy
        {
            public static readonly NoProxy Instance = new();

            private NoProxy()
            {
            }

            public System.Net.ICredentials? Credentials { get; set; }

            public Uri? GetProxy(Uri destination) => null;

            public bool IsBypassed(Uri host) => true;
        }

        [TearDown]
        public virtual void TearDown()
        {
            DateTime endTime = DateTime.Now;
            AllureLifecycle.Instance.UpdateTestCase(tc =>
            {
                tc.parameters.Add(new Parameter
                {
                    name = "Start Time",
                    value = _testStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Parameter
                {
                    name = "End Time",
                    value = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Parameter
                {
                    name = "Duration (s)",
                    value = (endTime - _testStartTime).TotalSeconds.ToString("F3")
                });
            });

            // Log test outcome and duration
            var ctx = TestContext.CurrentContext;
            string browser = (ctx.Test.Arguments.Length > 0 && ctx.Test.Arguments[0] is string b) ? b.ToLowerInvariant() : "firefox";
            Logger.Info($"[TearDown] Finished test '{ctx.Test.Name}' on browser '{browser}' | Outcome: {ctx.Result.Outcome.Status} | Duration(s): {(endTime - _testStartTime).TotalSeconds:F3}");
            if (ctx.Result.Outcome.Status == TestStatus.Failed)
            {
                Logger.Error($"[TearDown] Failure details: {ctx.Result.Message}");
            }

            // Publish result via configured provider
            var metadata = new LogMetadata
            {
                ProjectName = "ui-web",
                TestClassName = ctx.Test.ClassName ?? string.Empty,
                TestMethodName = ctx.Test.MethodName ?? ctx.Test.Name,
                Status = ctx.Result.Outcome.Status.ToString(),
                Duration = (endTime - _testStartTime).TotalSeconds.ToString("F3"),
                Reason = ctx.Result.Message ?? string.Empty,
                RunTime = endTime.ToString("o"),
                RunName = ctx.Test.FullName ?? ctx.Test.Name,
                TriggeredBy = System.Environment.UserName,
                Browser = browser,
                StartTime = _testStartTime,
                EndTime = endTime
            };
            try
            {
                var publisher = ResultsPublisherFactory.Create();
                publisher.Publish(metadata);
            }
            catch (System.Exception ex)
            {
                TestContext.Progress.WriteLine($"[Results] Publish failed: {ex.Message}");
            }


            try
            {
                // Capture screenshot if test failed
                if (ScreenshotHelper != null && TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                {
                    try
                    {
                        ScreenshotHelper.CaptureAndAttach("Test Failure", false);
                    }
                    catch { }
                }
            }
            finally
            {
                Driver?.Quit();
                Driver?.Dispose();
                Driver = null;
                StopLocalDriverProcess();
            }
        }
    }
}
