namespace Pages.Todo;

using Pages.Common;
using Microsoft.Playwright;
using System.Threading.Tasks;

public sealed class TodoPage(IPage page) : BasePage(page)
{
    private ILocator NewTodoInput => Page.Locator(".new-todo");
    private ILocator TodoItems => Page.Locator(".todo-list li");
    private ILocator ClearCompletedButton => Page.Locator(".clear-completed");

    public ILocator GetTodoItemByTitle(string title) => TodoItems.Filter(new() { HasText = title });
    public ILocator TodoCountElement => Page.Locator(".todo-count");

    public async Task NavigateAsync(string url) => await Page.GotoAsync(url);

    public async Task AddTodoAsync(string title)
    {
        await NewTodoInput.FillAsync(title);
        await NewTodoInput.PressAsync("Enter");
    }

    public async Task CompleteTodoAsync(string title)
    {
        var item = GetTodoItemByTitle(title);
        await item.Locator(".toggle").CheckAsync();
    }

    public async Task ClearCompletedAsync() => await ClearCompletedButton.ClickAsync();
}
