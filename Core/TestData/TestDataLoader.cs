using System.Text.Json;

namespace Core.TestData;

/// <summary>
/// Loads JSON test data copied to the test output directory (source: <c>Tests/test-data/</c>).
/// Use JSON only for large data-driven [Theory] tables or data owned outside the code; entities come from the
/// Builders / Object Mothers in Core/TestData/App/.
/// </summary>
public static class TestDataLoader
{
    /// <summary>Deserializes the file at <paramref name="filePath"/>, relative to "test-data" (by convention "App/TestClass/Scenario.json").</summary>
    public static T Load<T>(string filePath)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "test-data", filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Test data file not found: {fullPath}");
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllText(fullPath))
               ?? throw new InvalidDataException($"{filePath} is invalid JSON.");
    }
}
