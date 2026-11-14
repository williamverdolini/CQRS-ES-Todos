namespace CqrsEsTodo.Domain;

public abstract record TodoEvent;

public sealed record TodoListCreated(Guid ListId, string Title, string Description) : TodoEvent;

public sealed record TodoListDescriptionChanged(string Description) : TodoEvent;

public sealed record TodoItemAdded(
    Guid ItemId,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DueAt,
    int Importance,
    int UserId = 0,
    string? AssignedTo = null) : TodoEvent;

public sealed record TodoItemCompleted(Guid ItemId, DateTimeOffset CompletedAt, string? AssignedTo = null) : TodoEvent;

public sealed record TodoItemReopened(Guid ItemId) : TodoEvent;

public sealed record TodoItemImportanceChanged(Guid ItemId, int Importance) : TodoEvent;

public sealed record TodoItemDueDateChanged(Guid ItemId, DateTimeOffset? DueAt) : TodoEvent;

public sealed record TodoItemDescriptionChanged(Guid ItemId, string Description) : TodoEvent;

public sealed record TodoItemState(
    Guid Id,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DueAt,
    int Importance,
    DateTimeOffset? CompletedAt,
    int UserId = 0);