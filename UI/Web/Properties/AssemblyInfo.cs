// Selenium tests share one machine's browsers and drivers and ran sequentially under NUnit
// (config.json: RunTestsInParallel=false), so keep xUnit from running test classes in parallel.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
