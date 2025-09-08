using System.IO;
using System.Text.Json;

namespace Core.Utilities
{
    public class TestDataLoader : ITestDataLoader
    {
        public static TestDataLoader Instance { get; } = new TestDataLoader();

        /// <summary>
        /// Loads test data from a file under "test-data" relative to output dir.
        /// To avoid path issues, always pass the file path relative to the "test-data" directory, e.g.
        ///   "BookingApiPostTests/HappyPath.json"
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
}
