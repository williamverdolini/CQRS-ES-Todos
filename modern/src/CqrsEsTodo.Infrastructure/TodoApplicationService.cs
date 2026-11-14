using System.Text.Json;
using CqrsEsTodo.Domain;
using Microsoft.EntityFrameworkCore;

namespace CqrsEsTodo.Infrastructure;

public sealed class TodoApplicationService(CqrsTodoDbContext database)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TodoListResponse> CreateListAsync(string title, string description, CancellationToken cancellationToken)
    {
        var aggregate = TodoListAggregate.Create(Guid.NewGuid(), title, description);
        await SaveAsync(aggregate, cancellationToken);
        return ToResponse(aggregate);
    }

    public async Task<IReadOnlyList<TodoListResponse>> GetListsAsync(CancellationToken cancellationToken)
    {
        var lists = await database.TodoLists.AsNoTracking()
            .Include(item => item.Items)
            .OrderBy(item => item.Title)
            .ToListAsync(cancellationToken);

        return lists.Select(ToResponse).ToArray();
    }

    public async Task<TodoListResponse?> GetListAsync(Guid id, CancellationToken cancellationToken)
    {
        var list = await database.TodoLists.AsNoTracking()
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return list is null ? null : ToResponse(list);
    }

    public async Task<TodoItemResponse?> GetItemAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await database.TodoItems.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return item is null ? null : ToResponse(item);
    }

    public async Task<TodoListResponse?> ChangeDescriptionAsync(Guid id, string description, CancellationToken cancellationToken)
    {
        var aggregate = await LoadAsync(id, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        aggregate.ChangeDescription(description);
        await SaveAsync(aggregate, cancellationToken);
        return ToResponse(aggregate);
    }

    public async Task<TodoListResponse?> AddItemAsync(Guid id, string description, DateTimeOffset? dueAt, int importance, CancellationToken cancellationToken)
    {
        var aggregate = await LoadAsync(id, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        aggregate.AddItem(description, dueAt, importance, DateTimeOffset.UtcNow);
        await SaveAsync(aggregate, cancellationToken);
        return ToResponse(aggregate);
    }

    public async Task<TodoListResponse?> CompleteItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        var aggregate = await LoadAsync(listId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        aggregate.CompleteItem(itemId, DateTimeOffset.UtcNow);
        await SaveAsync(aggregate, cancellationToken);
        return ToResponse(aggregate);
    }

    public async Task<TodoListResponse?> ReopenItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        var aggregate = await LoadAsync(listId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        aggregate.ReopenItem(itemId);
        await SaveAsync(aggregate, cancellationToken);
        return ToResponse(aggregate);
    }

    public async Task<TodoListResponse?> ChangeItemImportanceAsync(Guid listId, Guid itemId, int importance, CancellationToken cancellationToken)
    {
        var aggregate = await LoadAsync(listId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        aggregate.ChangeItemImportance(itemId, importance);
        await SaveAsync(aggregate, cancellationToken);
        return ToResponse(aggregate);
    }

    public async Task<TodoListResponse?> ChangeItemDueDateAsync(Guid listId, Guid itemId, DateTimeOffset? dueAt, CancellationToken cancellationToken)
    {
        var aggregate = await LoadAsync(listId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        aggregate.ChangeItemDueDate(itemId, dueAt);
        await SaveAsync(aggregate, cancellationToken);
        return ToResponse(aggregate);
    }

    public async Task<TodoListResponse?> ChangeItemDescriptionAsync(Guid listId, Guid itemId, string description, CancellationToken cancellationToken)
    {
        var aggregate = await LoadAsync(listId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        aggregate.ChangeItemDescription(itemId, description);
        await SaveAsync(aggregate, cancellationToken);
        return ToResponse(aggregate);
    }

    public async Task RebuildReadModelAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        var existingLists = await database.TodoLists.Include(item => item.Items).ToListAsync(cancellationToken);
        database.TodoLists.RemoveRange(existingLists);
        await database.SaveChangesAsync(cancellationToken);
        database.ChangeTracker.Clear();

        var storedEvents = await database.TodoEvents.AsNoTracking()
            .OrderBy(item => item.AggregateId)
            .ThenBy(item => item.Version)
            .ToListAsync(cancellationToken);

        foreach (var group in storedEvents.GroupBy(item => item.AggregateId))
        {
            var aggregate = Rehydrate(group.Key, group);
            if (aggregate.IsCreated)
            {
                database.TodoLists.Add(ToReadModel(aggregate));
            }
        }

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<TodoListAggregate?> LoadAsync(Guid id, CancellationToken cancellationToken)
    {
        var storedEvents = await database.TodoEvents.AsNoTracking()
            .Where(item => item.AggregateId == id)
            .OrderBy(item => item.Version)
            .ToListAsync(cancellationToken);

        if (storedEvents.Count == 0)
        {
            return null;
        }

        var aggregate = Rehydrate(id, storedEvents);
        return aggregate.IsCreated ? aggregate : null;
    }

    private async Task SaveAsync(TodoListAggregate aggregate, CancellationToken cancellationToken)
    {
        var events = aggregate.UncommittedEvents;
        var firstVersion = aggregate.Version - events.Count + 1;

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        for (var index = 0; index < events.Count; index++)
        {
            var (eventType, payload) = TodoEventCodec.Serialize(events[index]);
            database.TodoEvents.Add(new StoredTodoEvent
            {
                AggregateId = aggregate.Id,
                Version = firstVersion + index,
                EventType = eventType,
                Payload = payload,
                OccurredAt = DateTimeOffset.UtcNow
            });
        }

        var readModel = await database.TodoLists
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == aggregate.Id, cancellationToken);

        if (readModel is null)
        {
            readModel = new TodoListReadModel { Id = aggregate.Id };
            database.TodoLists.Add(readModel);
        }

        UpdateReadModel(readModel, aggregate);

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        aggregate.ClearUncommittedEvents();
    }

    private void UpdateReadModel(TodoListReadModel readModel, TodoListAggregate aggregate)
    {
        readModel.Title = aggregate.Title;
        readModel.Description = aggregate.Description;
        readModel.Version = aggregate.Version;

        foreach (var state in aggregate.Items)
        {
            var item = readModel.Items.SingleOrDefault(existing => existing.Id == state.Id);
            if (item is null)
            {
                item = new TodoItemReadModel { Id = state.Id, ListId = aggregate.Id };
                readModel.Items.Add(item);
                database.TodoItems.Add(item);
            }

            item.Description = state.Description;
            item.UserId = state.UserId;
            item.CreatedAt = state.CreatedAt;
            item.DueAt = state.DueAt;
            item.Importance = state.Importance;
            item.CompletedAt = state.CompletedAt;
        }
    }

    private TodoListReadModel ToReadModel(TodoListAggregate aggregate)
    {
        var readModel = new TodoListReadModel { Id = aggregate.Id };
        UpdateReadModel(readModel, aggregate);
        return readModel;
    }

    private static TodoListAggregate Rehydrate(Guid id, IEnumerable<StoredTodoEvent> storedEvents) =>
        TodoListAggregate.Rehydrate(id, storedEvents.Select(TodoEventCodec.Deserialize));

    private static TodoListResponse ToResponse(TodoListAggregate aggregate) =>
        new(aggregate.Id, aggregate.Title, aggregate.Description, aggregate.Version,
            aggregate.Items.Select(item => new TodoItemResponse(item.Id, aggregate.Id, item.UserId, item.Description, item.CreatedAt, item.DueAt, item.Importance, item.CompletedAt)).ToArray());

    private static TodoListResponse ToResponse(TodoListReadModel list) =>
        new(list.Id, list.Title, list.Description, list.Version,
            list.Items.Select(ToResponse).ToArray());

    private static TodoItemResponse ToResponse(TodoItemReadModel item) =>
        new(item.Id, item.ListId, item.UserId, item.Description, item.CreatedAt, item.DueAt, item.Importance, item.CompletedAt);

    internal static class TodoEventCodec
    {
        public static (string EventType, string Payload) Serialize(TodoEvent @event) => @event switch
        {
            TodoListCreated value => ("todo-list-created.v1", JsonSerializer.Serialize(value, JsonOptions)),
            TodoListDescriptionChanged value => ("todo-list-description-changed.v1", JsonSerializer.Serialize(value, JsonOptions)),
            TodoItemAdded value => ("todo-item-added.v1", JsonSerializer.Serialize(value, JsonOptions)),
            TodoItemCompleted value => ("todo-item-completed.v1", JsonSerializer.Serialize(value, JsonOptions)),
            TodoItemReopened value => ("todo-item-reopened.v1", JsonSerializer.Serialize(value, JsonOptions)),
            TodoItemImportanceChanged value => ("todo-item-importance-changed.v1", JsonSerializer.Serialize(value, JsonOptions)),
            TodoItemDueDateChanged value => ("todo-item-due-date-changed.v1", JsonSerializer.Serialize(value, JsonOptions)),
            TodoItemDescriptionChanged value => ("todo-item-description-changed.v1", JsonSerializer.Serialize(value, JsonOptions)),
            _ => throw new InvalidOperationException($"Unsupported event type: {@event.GetType().Name}.")
        };

        public static TodoEvent Deserialize(StoredTodoEvent storedEvent) => storedEvent.EventType switch
        {
            "todo-list-created.v1" => JsonSerializer.Deserialize<TodoListCreated>(storedEvent.Payload, JsonOptions)!,
            "todo-list-description-changed.v1" => JsonSerializer.Deserialize<TodoListDescriptionChanged>(storedEvent.Payload, JsonOptions)!,
            "todo-item-added.v1" => JsonSerializer.Deserialize<TodoItemAdded>(storedEvent.Payload, JsonOptions)!,
            "todo-item-completed.v1" => JsonSerializer.Deserialize<TodoItemCompleted>(storedEvent.Payload, JsonOptions)!,
            "todo-item-reopened.v1" => JsonSerializer.Deserialize<TodoItemReopened>(storedEvent.Payload, JsonOptions)!,
            "todo-item-importance-changed.v1" => JsonSerializer.Deserialize<TodoItemImportanceChanged>(storedEvent.Payload, JsonOptions)!,
            "todo-item-due-date-changed.v1" => JsonSerializer.Deserialize<TodoItemDueDateChanged>(storedEvent.Payload, JsonOptions)!,
            "todo-item-description-changed.v1" => JsonSerializer.Deserialize<TodoItemDescriptionChanged>(storedEvent.Payload, JsonOptions)!,
            _ => throw new InvalidDataException($"Unknown persisted event type: {storedEvent.EventType}.")
        };
    }
}

public sealed record TodoListResponse(Guid Id, string Title, string Description, int Version, IReadOnlyList<TodoItemResponse> Items);

public sealed record TodoItemResponse(Guid Id, Guid ListId, int UserId, string Description, DateTimeOffset CreatedAt, DateTimeOffset? DueAt, int Importance, DateTimeOffset? CompletedAt);