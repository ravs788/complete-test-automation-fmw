using System.Text.Json;

namespace Core.TestData;

public class TestDataLoader : ITestDataLoader
{
    public static TestDataLoader Instance { get; } = new TestDataLoader();

    /// <summary>
    /// Loads test data from a file under "test-data" relative to output dir.
    /// Pass the path relative to "test-data", by convention "App/TestClass/Scenario.json".
    /// Use JSON only for large data-driven [Theory] tables or data owned outside the code;
    /// entities come from the Builders / Object Mothers in Core/TestData/App/.
    /// </summary>
    public T Load<T>(string filePath)
    {
        string baseDir = System.AppContext.BaseDirectory;
        var fullPath = Path.Combine(baseDir, "test-data", filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Test data file not found: {fullPath}");
        }

        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidDataException($"{filePath} is invalid JSON.");
    }
}
