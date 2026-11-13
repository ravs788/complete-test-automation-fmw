namespace Tests.UI.Web.Todo;

using Allure.Net.Commons.Attributes;
using Core.TestData;
using Pages.Todo;
using Tests.Fixtures;
using Tests.Infrastructure;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

[AllureSuite("TodoMVC")]
[AllureTag("todo", "ui", "playwright", "regression")]
[Trait("Category", "regression")]
public class TodoTests(PlaywrightFixture fixture) : UiTestBase(fixture)
{
    protected override string BaseUrl => Settings.TodoMvc.BaseUrl;

    [Fact]
    public Task UserCanAddAndVerifyTodoItem() => RunAsync(async () =>
    {
        // 1. Arrange: Dynamic test data via Object Mother (UUID-backed, collision-free)
        var item = TodoItemMother.StandardItem();
        var todoPage = new TodoPage(Page);
        await todoPage.NavigateAsync(BaseUrl);

        // 2. Act: Pure intent execution
        await todoPage.AddTodoAsync(item.Title);

        // 3. Assert: Auto-retrying web-first assertion
        await Assertions.Expect(todoPage.GetTodoItemByTitle(item.Title)).ToBeVisibleAsync();
    });

    [Fact]
    public Task UserCanCompleteTodoItem() => RunAsync(async () =>
    {
        // 1. Arrange
        var item = TodoItemMother.StandardItem();
        var todoPage = new TodoPage(Page);
        await todoPage.NavigateAsync(BaseUrl);
        await todoPage.AddTodoAsync(item.Title);

        // 2. Act
        await todoPage.CompleteTodoAsync(item.Title);

        // 3. Assert
        await Assertions.Expect(todoPage.GetTodoItemByTitle(item.Title)).ToHaveClassAsync(new Regex("completed"));
    });
}
