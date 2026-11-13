using Core.Models.Todo;

namespace Core.TestData.Todo;

/// <summary>Ready-made <see cref="TodoItem"/>s for common scenarios (Object Mother).</summary>
public static class TodoItemMother
{
    public static TodoItem Standard() => new TodoItemBuilder().Build();

    public static TodoItem Completed() => new TodoItemBuilder().AsCompleted().Build();
}
