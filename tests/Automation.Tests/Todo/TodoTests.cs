namespace Automation.Tests.Todo;

using Automation.Core.Configuration;
using Automation.Core.TestData;
using Automation.Pages.Todo;
using Automation.Tests.Fixtures;
using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

public class TodoTests : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private TodoPage _todoPage = null!;

    public TodoTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        (_context, _page) = await _fixture.CreateCleanPageAsync();
        _todoPage = new TodoPage(_page);
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.CloseAsync();
        }
    }

    [Fact]
    public async Task UserCanAddAndVerifyTodoItem()
    {
        // 1. Arrange: Dynamic test data via Object Mother (UUID-backed, collision-free)
        var item = TodoItemMother.StandardItem();
        await _todoPage.NavigateAsync(_fixture.Settings.BaseUrl);

        // 2. Act: Pure intent execution
        await _todoPage.AddTodoAsync(item.Title);

        // 3. Assert: Auto-retrying web-first assertion
        var todoLocator = _todoPage.GetTodoItemByTitle(item.Title);
        await Assertions.Expect(todoLocator).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UserCanCompleteTodoItem()
    {
        // 1. Arrange
        var item = TodoItemMother.StandardItem();
        await _todoPage.NavigateAsync(_fixture.Settings.BaseUrl);
        await _todoPage.AddTodoAsync(item.Title);

        // 2. Act
        await _todoPage.CompleteTodoAsync(item.Title);

        // 3. Assert
        var completedItem = _todoPage.GetTodoItemByTitle(item.Title);
        await Assertions.Expect(completedItem).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("completed"));
    }
}
