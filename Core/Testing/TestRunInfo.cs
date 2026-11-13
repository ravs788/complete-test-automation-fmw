using Core.Logging;

namespace Core.Testing;

/// <summary>Identifies the test being run by <see cref="TestRunner"/>.</summary>
public sealed record TestRunInfo(
    ILoggingService Logger,
    string ProjectName,
    Type TestClass,
    string TestName,
    string Browser = "");
