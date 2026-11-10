namespace Automation.Core.Configuration;

using System;
using System.Threading;

public static class ConfigurationProvider
{
    private static readonly Lazy<TestSettings> _lazySettings = new(
        () => new TestSettings(),
        LazyThreadSafetyMode.ExecutionAndPublication
    );

    public static TestSettings Current => _lazySettings.Value;
}
