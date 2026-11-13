namespace Core.TestData;

public interface ITestDataLoader
{
    T Load<T>(string filePath);
}
