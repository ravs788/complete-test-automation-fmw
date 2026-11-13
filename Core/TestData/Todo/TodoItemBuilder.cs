using Core.Models.Todo;

namespace Core.TestData.Todo;

/// <summary>Builds <see cref="TodoItem"/>s with a unique title by default, so parallel tests never collide.</summary>
public sealed class TodoItemBuilder
{
    private string _title = $"Task-{Guid.NewGuid():N}";
    private bool _isCompleted;

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

    public TodoItem Build() => new() { Title = _title, IsCompleted = _isCompleted };
}
