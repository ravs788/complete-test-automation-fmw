using Domain.Models.Todo;
using Pages.Todo;
using Tests.Fixtures;
using Tests.Infrastructure;

namespace Tests.UI.Web.Todo;

public abstract class TodoTestBase(BrowserFixture fixture) : UiTestBase(fixture)
{
    protected override string BaseUrl => Settings.TodoMvc.BaseUrl;

    /// <summary>Opens the todo app and adds <paramref name="item"/>; returns the todo page.</summary>
    protected async Task<TodoPage> OpenWithItemAsync(TodoItem item)
    {
        var todoPage = new TodoPage(Session);
        await todoPage.OpenAsync();
        await todoPage.AddTodoAsync(item.Title);
        return todoPage;
    }
}
