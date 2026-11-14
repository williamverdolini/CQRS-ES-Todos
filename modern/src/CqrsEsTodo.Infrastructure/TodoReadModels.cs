namespace CqrsEsTodo.Infrastructure;

public sealed class TodoListReadModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Version { get; set; }
    public ICollection<TodoItemReadModel> Items { get; set; } = new List<TodoItemReadModel>();
}

public sealed class TodoItemReadModel
{
    public Guid Id { get; set; }
    public Guid ListId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public int Importance { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public TodoListReadModel? List { get; set; }
}