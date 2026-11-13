using Microsoft.Playwright;
using Pages.Common;

namespace Pages.Todo;

/// <summary>The Playwright TodoMVC demo (test ids use Playwright's default <c>data-testid</c>).</summary>
public sealed class TodoPage(IPage page) : BasePage(page)
{
    private ILocator NewTodoInput => Page.GetByPlaceholder("What needs to be done?");
    private ILocator TodoItems => TestId("todo-item");

    public ILocator GetTodoItemByTitle(string title) =>
        TodoItems.Filter(new() { Has = TestId("todo-title").Filter(new() { HasTextString = title }) });

    public ILocator TodoCountElement => TestId("todo-count");

    /// <summary>Opens the todo app (the context's BaseURL).</summary>
    public async Task OpenAsync() => await GotoAsync("./");

    public async Task AddTodoAsync(string title)
    {
        await NewTodoInput.FillAsync(title);
        await NewTodoInput.PressAsync("Enter");
    }

    public async Task CompleteTodoAsync(string title) =>
        await GetTodoItemByTitle(title).GetByRole(AriaRole.Checkbox, new() { Name = "Toggle Todo" }).CheckAsync();

    public async Task ClearCompletedAsync() =>
        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear completed" }).ClickAsync();
}
