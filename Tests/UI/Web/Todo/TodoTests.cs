using Core.Reporting;
using Core.Ui;
using Domain.TestData.Todo;
using Tests.Fixtures;

namespace Tests.UI.Web.Todo;

[ReportSuite("UI Web", "TodoMVC", "Todo items")]
[ReportTags("ui", "todomvc", "todo-items", "regression")]
[Trait("Category", "regression")]
public class TodoTests(BrowserFixture fixture) : TodoTestBase(fixture)
{
    [Fact]
    public Task TodoList_Should_ShowItem_When_ItemAdded() => RunAsync(async () =>
    {
        // Arrange
        var item = TodoItemMother.Standard();

        // Act
        var todoPage = await OpenWithItemAsync(item);

        // Assert
        await Report.Step("Assert todo item is listed", async () =>
            await Ui.Expect(todoPage.GetTodoItemByTitle(item.Title)).ToBeVisibleAsync());
    });

    [Fact]
    public Task TodoItem_Should_BeMarkedCompleted_When_Toggled() => RunAsync(async () =>
    {
        // Arrange
        var item = TodoItemMother.Standard();
        var todoPage = await OpenWithItemAsync(item);

        // Act
        await todoPage.CompleteTodoAsync(item.Title);

        // Assert
        await Report.Step("Assert todo item is marked completed", async () =>
            await Ui.Expect(todoPage.GetTodoItemByTitle(item.Title)).ToHaveClassAsync("completed"));
    });
}
