namespace CqrsEsTodo.Domain;

public sealed class TodoListAggregate
{
    private readonly List<TodoEvent> uncommittedEvents = [];
    private readonly Dictionary<Guid, TodoItemState> items = [];

    private TodoListAggregate(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A list ID is required.", nameof(id));
        }

        Id = id;
    }

    public Guid Id { get; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public bool IsCreated { get; private set; }
    public IReadOnlyList<TodoEvent> UncommittedEvents => uncommittedEvents;
    public IReadOnlyCollection<TodoItemState> Items => items.Values.ToArray();

    public static TodoListAggregate Create(Guid id, string title, string description)
    {
        RequireText(title, nameof(title));
        var aggregate = new TodoListAggregate(id);
        aggregate.Raise(new TodoListCreated(id, title, description ?? string.Empty));
        return aggregate;
    }

    public static TodoListAggregate Rehydrate(Guid id, IEnumerable<TodoEvent> history)
    {
        var aggregate = new TodoListAggregate(id);
        foreach (var @event in history)
        {
            aggregate.Apply(@event);
        }

        return aggregate;
    }

    public void ChangeDescription(string description)
    {
        EnsureCreated();
        RequireText(description, nameof(description));
        Raise(new TodoListDescriptionChanged(description));
    }

    public Guid AddItem(string description, DateTimeOffset? dueAt, int importance, DateTimeOffset createdAt, int userId = 0, string? assignedTo = null)
    {
        EnsureCreated();
        RequireText(description, nameof(description));

        var itemId = Guid.NewGuid();
        Raise(new TodoItemAdded(itemId, description, createdAt, dueAt, importance, userId, assignedTo));
        return itemId;
    }

    public void CompleteItem(Guid itemId, DateTimeOffset completedAt, string? assignedTo = null)
    {
        EnsureCreated();
        var item = GetItem(itemId);
        if (item.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("The item is already complete.");
        }

        Raise(new TodoItemCompleted(itemId, completedAt, assignedTo));
    }

    public void ReopenItem(Guid itemId)
    {
        EnsureCreated();
        var item = GetItem(itemId);
        if (!item.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("The item is not complete.");
        }

        Raise(new TodoItemReopened(itemId));
    }

    public void ChangeItemImportance(Guid itemId, int importance)
    {
        EnsureCreated();
        GetItem(itemId);
        Raise(new TodoItemImportanceChanged(itemId, importance));
    }

    public void ChangeItemDueDate(Guid itemId, DateTimeOffset? dueAt)
    {
        EnsureCreated();
        GetItem(itemId);
        Raise(new TodoItemDueDateChanged(itemId, dueAt));
    }

    public void ChangeItemDescription(Guid itemId, string description)
    {
        EnsureCreated();
        GetItem(itemId);
        RequireText(description, nameof(description));
        Raise(new TodoItemDescriptionChanged(itemId, description));
    }

    public void ClearUncommittedEvents() => uncommittedEvents.Clear();

    private void Raise(TodoEvent @event)
    {
        Apply(@event);
        uncommittedEvents.Add(@event);
    }

    private void Apply(TodoEvent @event)
    {
        switch (@event)
        {
            case TodoListCreated created:
                IsCreated = true;
                Title = created.Title;
                Description = created.Description;
                break;
            case TodoListDescriptionChanged changed:
                Description = changed.Description;
                break;
            case TodoItemAdded added:
                items.Add(added.ItemId, new TodoItemState(added.ItemId, added.Description, added.CreatedAt, added.DueAt, added.Importance, null, added.UserId));
                break;
            case TodoItemCompleted completed:
                items[completed.ItemId] = GetItem(completed.ItemId) with { CompletedAt = completed.CompletedAt };
                break;
            case TodoItemReopened reopened:
                items[reopened.ItemId] = GetItem(reopened.ItemId) with { CompletedAt = null };
                break;
            case TodoItemImportanceChanged changed:
                items[changed.ItemId] = GetItem(changed.ItemId) with { Importance = changed.Importance };
                break;
            case TodoItemDueDateChanged changed:
                items[changed.ItemId] = GetItem(changed.ItemId) with { DueAt = changed.DueAt };
                break;
            case TodoItemDescriptionChanged changed:
                items[changed.ItemId] = GetItem(changed.ItemId) with { Description = changed.Description };
                break;
            default:
                throw new InvalidDataException($"Unsupported event type: {@event.GetType().Name}.");
        }

        Version++;
    }

    private TodoItemState GetItem(Guid itemId) =>
        items.TryGetValue(itemId, out var item)
            ? item
            : throw new KeyNotFoundException("The item does not exist in this list.");

    private void EnsureCreated()
    {
        if (!IsCreated)
        {
            throw new InvalidOperationException("The list has not been created.");
        }
    }

    private static void RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }
    }
}