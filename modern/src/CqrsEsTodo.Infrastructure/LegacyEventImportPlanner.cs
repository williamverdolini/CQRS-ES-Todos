using System.Globalization;
using System.Text.Json;
using CqrsEsTodo.Domain;

namespace CqrsEsTodo.Infrastructure;

public static class LegacyEventImportPlanner
{
    public static LegacyEventImportPlan CreatePlan(LegacyEventBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        if (bundle.FormatVersion != 1 || !string.Equals(bundle.Source, "NEventStore", StringComparison.Ordinal))
        {
            throw new InvalidDataException("Only NEventStore event bundle format version 1 is supported.");
        }

        if (bundle.Commits.Count == 0)
        {
            throw new InvalidDataException("The event bundle contains no commits.");
        }

        var aggregates = new Dictionary<Guid, List<LegacyImportedEvent>>();
        var itemParents = new Dictionary<Guid, Guid>();
        var addedItems = new HashSet<Guid>();
        var createdLists = new HashSet<Guid>();
        var sourceEventCount = 0;
        LegacyEventSourceMetadata currentSource = null!;
        long previousCheckpoint = 0;

        foreach (var commit in bundle.Commits)
        {
            if (!long.TryParse(commit.CheckpointToken, NumberStyles.None, CultureInfo.InvariantCulture, out var checkpoint)
                || checkpoint <= previousCheckpoint)
            {
                throw new InvalidDataException("Commit checkpoint tokens must be numeric, unique, and in ascending order.");
            }

            if (string.IsNullOrWhiteSpace(commit.StreamId) || commit.StreamRevision < 1 || commit.Events.Count == 0)
            {
                throw new InvalidDataException($"Commit {commit.CheckpointToken} has invalid stream metadata or no events.");
            }

            previousCheckpoint = checkpoint;
            var occurredAt = AsUtc(commit.CommitStamp);

            for (var eventIndex = 0; eventIndex < commit.Events.Count; eventIndex++)
            {
                var legacyEvent = commit.Events[eventIndex];
                sourceEventCount++;
                if (legacyEvent.Payload.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException($"Event '{legacyEvent.Type}' in commit {commit.CheckpointToken} has no object payload.");
                }

                var sourceStreamRevision = commit.StreamRevision - commit.Events.Count + eventIndex + 1;
                if (sourceStreamRevision < 1)
                {
                    throw new InvalidDataException($"Commit {commit.CheckpointToken} has an invalid stream revision for its event count.");
                }

                currentSource = new LegacyEventSourceMetadata(
                    commit.CheckpointToken,
                    commit.StreamId,
                    sourceStreamRevision,
                    commit.CommitSequence,
                    eventIndex,
                    legacyEvent.Type,
                    legacyEvent.Payload.GetRawText(),
                    GetRawJson(commit.Headers),
                    GetRawJson(legacyEvent.Headers));

                var name = GetShortTypeName(legacyEvent.Type);
                var payload = legacyEvent.Payload;
                switch (name)
                {
                    case "CreatedToDoListEvent":
                    {
                        var listId = ReadGuid(payload, "ToDoListId", commit.StreamId);
                        if (!createdLists.Add(listId))
                        {
                            throw new InvalidDataException($"List {listId} has more than one creation event.");
                        }

                        Add(listId, new TodoListCreated(listId, ReadString(payload, "Title"), ReadString(payload, "Description")), occurredAt);
                        break;
                    }
                    case "ChangedToDoListDescriptionEvent":
                    {
                        var listId = ReadGuid(payload, "ToDoListId", commit.StreamId);
                        RequireCreatedList(listId, commit.CheckpointToken);
                        Add(listId, new TodoListDescriptionChanged(ReadString(payload, "Description")), occurredAt);
                        break;
                    }
                    case "ToDoListMementoPropagatedEvent":
                    {
                        var memento = ReadObject(payload, "Memento");
                        var listId = ReadGuid(memento, "Id");
                        if (createdLists.Add(listId))
                        {
                            Add(listId, new TodoListCreated(listId, ReadString(memento, "Title"), ReadString(memento, "Description")), occurredAt);
                        }

                        break;
                    }
                    case "AddedNewToDoItemEvent":
                    case "AddedNewToDoItemEvent_V0":
                    {
                        var listId = ReadGuid(payload, "ToDoListId");
                        var itemId = ReadGuid(payload, "ToDoItemId", commit.StreamId);
                        RequireCreatedList(listId, commit.CheckpointToken);
                        RegisterItem(itemId, listId, commit.CheckpointToken);
                        addedItems.Add(itemId);
                        Add(listId, new TodoItemAdded(
                            itemId,
                            ReadString(payload, "Description"),
                            ReadDateTimeOffset(payload, "CreationDate"),
                            ReadNullableDateTimeOffset(payload, "DueDate"),
                            ReadInt32(payload, "Importance"),
                            0,
                            ReadNullableString(payload, "AssignedTo")), occurredAt);
                        break;
                    }
                    case "ToDoItemMementoPropagatedEvent":
                    {
                        var memento = ReadObject(payload, "Memento");
                        var itemId = ReadGuid(memento, "Id", commit.StreamId);
                        var listId = ReadGuid(memento, "ToDoListId");
                        RegisterItem(itemId, listId, commit.CheckpointToken);
                        if (addedItems.Add(itemId))
                        {
                            RequireCreatedList(listId, commit.CheckpointToken);
                            Add(listId, new TodoItemAdded(
                                itemId,
                                ReadString(memento, "Description"),
                                ReadDateTimeOffset(memento, "CreationDate"),
                                ReadNullableDateTimeOffset(memento, "DueDate"),
                                ReadInt32(memento, "Importance"),
                                ReadInt32(memento, "UserId")), occurredAt);

                            var closingDate = ReadNullableDateTimeOffset(memento, "ClosingDate");
                            if (closingDate.HasValue)
                            {
                                Add(listId, new TodoItemCompleted(itemId, closingDate.Value), occurredAt);
                            }
                        }

                        break;
                    }
                    case "MarkedToDoItemAsCompletedEvent":
                    case "MarkedToDoItemAsCompletedEvent_V0":
                    {
                        var itemId = ReadGuid(payload, "Id", commit.StreamId);
                        var listId = RequireItemParent(itemId, commit.CheckpointToken);
                        Add(listId, new TodoItemCompleted(itemId, ReadDateTimeOffset(payload, "ClosingDate"), ReadNullableString(payload, "AssignedTo")), occurredAt);
                        break;
                    }
                    case "ReOpenedToDoItemEvent":
                    {
                        var itemId = ReadGuid(payload, "Id", commit.StreamId);
                        var listId = RequireItemParent(itemId, commit.CheckpointToken);
                        Add(listId, new TodoItemReopened(itemId), occurredAt);
                        break;
                    }
                    case "ChangedToDoItemImportanceEvent":
                    {
                        var itemId = ReadGuid(payload, "Id", commit.StreamId);
                        var listId = RequireItemParent(itemId, commit.CheckpointToken);
                        Add(listId, new TodoItemImportanceChanged(itemId, ReadInt32(payload, "Importance")), occurredAt);
                        break;
                    }
                    case "ChangedToDoItemDescriptionEvent":
                    {
                        var itemId = ReadGuid(payload, "Id", commit.StreamId);
                        var listId = RequireItemParent(itemId, commit.CheckpointToken);
                        Add(listId, new TodoItemDescriptionChanged(itemId, ReadString(payload, "Description")), occurredAt);
                        break;
                    }
                    case "ChangedToDoItemDueDateEvent":
                    {
                        var itemId = ReadGuid(payload, "Id", commit.StreamId);
                        var listId = RequireItemParent(itemId, commit.CheckpointToken);
                        Add(listId, new TodoItemDueDateChanged(itemId, ReadNullableDateTimeOffset(payload, "DueDate")), occurredAt);
                        break;
                    }
                    default:
                        throw new InvalidDataException($"Unsupported legacy event type '{legacyEvent.Type}' at checkpoint {commit.CheckpointToken}.");
                }
            }
        }

        var plannedAggregates = aggregates
            .OrderBy(item => item.Key)
            .Select(item =>
            {
                var state = TodoListAggregate.Rehydrate(item.Key, item.Value.Select(value => value.Event));
                if (!state.IsCreated)
                {
                    throw new InvalidDataException($"List {item.Key} has no creation event or memento.");
                }

                return new LegacyAggregateImport(item.Key, item.Value, state);
            })
            .ToArray();

        return new LegacyEventImportPlan(
            bundle.Commits.Count,
            sourceEventCount,
            plannedAggregates.Sum(item => item.Events.Count),
            bundle.Commits[^1].CheckpointToken,
            plannedAggregates);

        void Add(Guid listId, TodoEvent @event, DateTimeOffset timestamp)
        {
            if (!aggregates.TryGetValue(listId, out var events))
            {
                events = [];
                aggregates.Add(listId, events);
            }

            events.Add(new LegacyImportedEvent(@event, timestamp, currentSource));
        }

        void RequireCreatedList(Guid listId, string checkpoint)
        {
            if (!createdLists.Contains(listId))
            {
                throw new InvalidDataException($"Event at checkpoint {checkpoint} references list {listId} before its creation.");
            }
        }

        void RegisterItem(Guid itemId, Guid listId, string checkpoint)
        {
            if (itemId == Guid.Empty || listId == Guid.Empty)
            {
                throw new InvalidDataException($"Item event at checkpoint {checkpoint} has an empty list or item ID.");
            }

            if (itemParents.TryGetValue(itemId, out var existingListId) && existingListId != listId)
            {
                throw new InvalidDataException($"Item {itemId} is associated with more than one list.");
            }

            itemParents[itemId] = listId;
        }

        Guid RequireItemParent(Guid itemId, string checkpoint) =>
            itemParents.TryGetValue(itemId, out var listId)
                ? listId
                : throw new InvalidDataException($"Item event at checkpoint {checkpoint} appears before its item is added.");
    }

    private static string GetShortTypeName(string typeName)
    {
        var withoutAssembly = typeName.Split(',')[0].Trim();
        return withoutAssembly[(withoutAssembly.LastIndexOf('.') + 1)..];
    }

    private static JsonElement ReadObject(JsonElement payload, string propertyName)
    {
        var value = ReadProperty(payload, propertyName);
        return value.ValueKind == JsonValueKind.Object
            ? value
            : throw new InvalidDataException($"Legacy payload is missing object property '{propertyName}'.");
    }

    private static Guid ReadGuid(JsonElement payload, string propertyName, string? fallback = null)
    {
        if (TryReadProperty(payload, propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            && Guid.TryParse(value.GetString(), out var id)
            && id != Guid.Empty)
        {
            return id;
        }

        if (fallback is not null && Guid.TryParse(fallback, out id) && id != Guid.Empty)
        {
            return id;
        }

        throw new InvalidDataException($"Legacy payload is missing GUID property '{propertyName}'.");
    }

    private static string ReadString(JsonElement payload, string propertyName) =>
        ReadNullableString(payload, propertyName) ?? string.Empty;

    private static string? ReadNullableString(JsonElement payload, string propertyName)
    {
        if (!TryReadProperty(payload, propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new InvalidDataException($"Legacy payload property '{propertyName}' is not a string.");
        }

        return value.GetString();
    }

    private static int ReadInt32(JsonElement payload, string propertyName) =>
        TryReadProperty(payload, propertyName, out var value) && value.TryGetInt32(out var result)
            ? result
            : 0;

    private static DateTimeOffset ReadDateTimeOffset(JsonElement payload, string propertyName) =>
        ReadNullableDateTimeOffset(payload, propertyName)
        ?? throw new InvalidDataException($"Legacy payload is missing date property '{propertyName}'.");

    private static DateTimeOffset? ReadNullableDateTimeOffset(JsonElement payload, string propertyName)
    {
        if (!TryReadProperty(payload, propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String || !value.TryGetDateTimeOffset(out var timestamp))
        {
            throw new InvalidDataException($"Legacy payload property '{propertyName}' is not an ISO date.");
        }

        return timestamp.ToUniversalTime();
    }

    private static DateTimeOffset AsUtc(DateTime timestamp) => timestamp.Kind switch
    {
        DateTimeKind.Utc => new DateTimeOffset(timestamp),
        DateTimeKind.Local => new DateTimeOffset(timestamp).ToUniversalTime(),
        _ => new DateTimeOffset(DateTime.SpecifyKind(timestamp, DateTimeKind.Utc))
    };

    private static string? GetRawJson(JsonElement? element) =>
        element.HasValue && element.Value.ValueKind != JsonValueKind.Undefined
            ? element.Value.GetRawText()
            : null;

    private static JsonElement ReadProperty(JsonElement payload, string propertyName) =>
        TryReadProperty(payload, propertyName, out var value)
            ? value
            : throw new InvalidDataException($"Legacy payload is missing property '{propertyName}'.");

    private static bool TryReadProperty(JsonElement payload, string propertyName, out JsonElement value)
    {
        foreach (var property in payload.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

}