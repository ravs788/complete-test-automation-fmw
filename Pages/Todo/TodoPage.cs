using Core.Ui;
using Pages.Common;

namespace Pages.Todo;

/// <summary>The Playwright TodoMVC demo (test ids use Playwright's default <c>data-testid</c>).</summary>
public sealed class TodoPage(IUiSession session) : BasePage(session)
{
    private IElement NewTodoInput => Session.Find(Locate.Placeholder("What needs to be done?"));
    private IElement TodoItems => TestId("todo-item");

    public IElement GetTodoItemByTitle(string title) =>
        TodoItems.WithChild(ByTestId("todo-title"), title);

    public IElement TodoCountElement => TestId("todo-count");

    /// <summary>Opens the todo app (the context's BaseURL).</summary>
    public async Task OpenAsync() => await GotoAsync("./");

    public async Task AddTodoAsync(string title)
    {
        await NewTodoInput.FillAsync(title);
        await NewTodoInput.PressAsync("Enter");
    }

    public async Task CompleteTodoAsync(string title) =>
        await GetTodoItemByTitle(title).Find(Locate.Role(UiRole.Checkbox, "Toggle Todo")).CheckAsync();

    public async Task ClearCompletedAsync() =>
        await Session.Find(Locate.Role(UiRole.Button, "Clear completed")).ClickAsync();
}
