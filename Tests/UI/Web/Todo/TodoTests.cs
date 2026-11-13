using System.Text.RegularExpressions;
using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Core.TestData.Todo;
using Microsoft.Playwright;
using Tests.Fixtures;

namespace Tests.UI.Web.Todo;

[AllureParentSuite("UI Web")]
[AllureSuite("TodoMVC")]
[AllureSubSuite("Todo items")]
[AllureTag("ui", "todomvc", "todo-items", "regression")]
[Trait("Category", "regression")]
public class TodoTests(PlaywrightFixture fixture) : TodoTestBase(fixture)
{
    [Fact]
    public Task TodoList_Should_ShowItem_When_ItemAdded() => RunAsync(async () =>
    {
        var item = TodoItemMother.Standard();

        var todoPage = await OpenWithItemAsync(item);

        await AllureApi.Step("Assert todo item is listed", async () =>
            await Assertions.Expect(todoPage.GetTodoItemByTitle(item.Title)).ToBeVisibleAsync());
    });

    [Fact]
    public Task TodoItem_Should_BeMarkedCompleted_When_Toggled() => RunAsync(async () =>
    {
        var item = TodoItemMother.Standard();
        var todoPage = await OpenWithItemAsync(item);

        await todoPage.CompleteTodoAsync(item.Title);

        await AllureApi.Step("Assert todo item is marked completed", async () =>
            await Assertions.Expect(todoPage.GetTodoItemByTitle(item.Title)).ToHaveClassAsync(new Regex("completed")));
    });
}
