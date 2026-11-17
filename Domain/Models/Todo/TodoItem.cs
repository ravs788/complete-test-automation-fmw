namespace Domain.Models.Todo;

public sealed record TodoItem
{
    public string Title { get; init; } = "";
    public bool IsCompleted { get; init; }
}
