namespace Core.TestData;

using System;

public readonly record struct TodoItem(string Id, string Title, bool IsCompleted);

public sealed class TodoItemBuilder
{
    private string _title = $"Task-{Guid.NewGuid():N}";
    private bool _isCompleted = false;

    public TodoItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TodoItemBuilder AsCompleted()
    {
        _isCompleted = true;
        return this;
    }

    public TodoItem Build() => new(Guid.NewGuid().ToString("N"), _title, _isCompleted);
}

public static class TodoItemMother
{
    public static TodoItem StandardItem() => new TodoItemBuilder().Build();
    public static TodoItem CompletedItem() => new TodoItemBuilder().AsCompleted().Build();
}
