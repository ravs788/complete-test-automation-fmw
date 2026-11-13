namespace Core.Utilities
{
    public interface ITestDataLoader
    {
        T Load<T>(string filePath);
    }
}
