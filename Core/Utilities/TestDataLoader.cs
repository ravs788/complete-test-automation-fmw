using System.IO;
using System.Text.Json;

namespace Core.Utilities
{
    public class TestDataLoader : ITestDataLoader
    {
        public static TestDataLoader Instance { get; } = new TestDataLoader();

        public T Load<T>(string filePath)
        {
            var fullPath = Path.Combine(System.AppContext.BaseDirectory, "test-data", filePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Test data file not found: {fullPath}");
            }

            var json = File.ReadAllText(fullPath);
            return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidDataException($"{filePath} is invalid JSON.");
        }
    }
}
