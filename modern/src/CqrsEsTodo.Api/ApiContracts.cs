namespace CqrsEsTodo.Api;

public sealed record CreateListRequest(string Title, string Description);

public sealed record ChangeDescriptionRequest(string Description);

public sealed record AddItemRequest(string Description, DateTimeOffset? DueAt, int Importance = 0);

public sealed record CompleteItemRequest(bool Completed);

public sealed record ChangeImportanceRequest(int Importance);

public sealed record ChangeDueDateRequest(DateTimeOffset? DueAt);

public sealed record ChangeItemDescriptionRequest(string Description);