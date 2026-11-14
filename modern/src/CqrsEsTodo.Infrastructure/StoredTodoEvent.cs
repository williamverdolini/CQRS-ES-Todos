namespace CqrsEsTodo.Infrastructure;

public sealed class StoredTodoEvent
{
    public Guid AggregateId { get; set; }
    public int Version { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? LegacySource { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}