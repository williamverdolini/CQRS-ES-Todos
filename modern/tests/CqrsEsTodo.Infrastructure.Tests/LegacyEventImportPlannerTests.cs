using System.Text.Json;
using CqrsEsTodo.Domain;
using CqrsEsTodo.Infrastructure;

namespace CqrsEsTodo.Infrastructure.Tests;

public sealed class LegacyEventImportPlannerTests
{
    [Fact]
    public void CreatePlan_MergesListAndItemStreamsInCheckpointOrder()
    {
        var listId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bundle = new LegacyEventBundle(1, "NEventStore",
        [
            Commit("1", listId, "CreatedToDoListEvent", JsonSerializer.Serialize(new { ToDoListId = listId, Title = "Work", Description = "List" })),
            Commit("2", itemId, "AddedNewToDoItemEvent", JsonSerializer.Serialize(new { ToDoListId = listId, ToDoItemId = itemId, CreationDate = DateTimeOffset.Parse("2026-11-14T10:00:00Z"), Description = "Old description", DueDate = (DateTimeOffset?)null, Importance = 2, AssignedTo = "legacy-user" })),
            Commit("3", itemId, "ChangedToDoItemDescriptionEvent", JsonSerializer.Serialize(new { Id = itemId, Description = "New description" })),
            Commit("4", itemId, "MarkedToDoItemAsCompletedEvent", JsonSerializer.Serialize(new { Id = itemId, ClosingDate = DateTimeOffset.Parse("2026-11-14T11:00:00Z"), AssignedTo = "closer" }))
        ]);

        var plan = LegacyEventImportPlanner.CreatePlan(bundle);

        var aggregate = Assert.Single(plan.Aggregates);
        var item = Assert.Single(aggregate.State.Items);
        Assert.Equal(listId, aggregate.AggregateId);
        Assert.Equal(4, plan.SourceEventCount);
        Assert.Equal(4, plan.ImportedEventCount);
        Assert.Equal("New description", item.Description);
        Assert.NotNull(item.CompletedAt);
        Assert.Equal("legacy-user", Assert.IsType<TodoItemAdded>(aggregate.Events[1].Event).AssignedTo);
        Assert.Equal("closer", Assert.IsType<TodoItemCompleted>(aggregate.Events[3].Event).AssignedTo);
    }

    [Fact]
    public void CreatePlan_UsesMementosAsBaselineAndRetainsUserId()
    {
        var listId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bundle = new LegacyEventBundle(1, "NEventStore",
        [
            Commit("1", listId, "ToDoListMementoPropagatedEvent", JsonSerializer.Serialize(new { Memento = new { Id = listId, Version = 1, Title = "Migrated", Description = "From old read model" } })),
            Commit("2", itemId, "ToDoItemMementoPropagatedEvent", JsonSerializer.Serialize(new { Memento = new { Id = itemId, Version = 1, ToDoListId = listId, Description = "Legacy item", CreationDate = DateTimeOffset.Parse("2026-11-12T08:30:00Z"), DueDate = (DateTimeOffset?)null, Importance = 3, ClosingDate = (DateTimeOffset?)DateTimeOffset.Parse("2026-11-13T10:00:00Z"), UserId = 17 } }))
        ]);

        var plan = LegacyEventImportPlanner.CreatePlan(bundle);

        var aggregate = Assert.Single(plan.Aggregates);
        var item = Assert.Single(aggregate.State.Items);
        Assert.Equal(3, plan.ImportedEventCount);
        Assert.Equal(17, item.UserId);
        Assert.Equal("Legacy item", item.Description);
        Assert.NotNull(item.CompletedAt);
    }

    [Fact]
    public void CreatePlan_RejectsUnknownEventsRatherThanDroppingHistory()
    {
        var bundle = new LegacyEventBundle(1, "NEventStore",
        [
            Commit("1", Guid.NewGuid(), "FutureTodoEvent", "{}")
        ]);

        Assert.Throws<InvalidDataException>(() => LegacyEventImportPlanner.CreatePlan(bundle));
    }

    [Fact]
    public void CreatePlan_RejectsOutOfOrderCheckpoints()
    {
        var listId = Guid.NewGuid();
        var bundle = new LegacyEventBundle(1, "NEventStore",
        [
            Commit("2", listId, "CreatedToDoListEvent", JsonSerializer.Serialize(new { ToDoListId = listId, Title = "Work", Description = "List" })),
            Commit("1", listId, "ChangedToDoListDescriptionEvent", JsonSerializer.Serialize(new { ToDoListId = listId, Description = "Changed" }))
        ]);

        Assert.Throws<InvalidDataException>(() => LegacyEventImportPlanner.CreatePlan(bundle));
    }

    private static LegacyEventCommit Commit(string token, Guid streamId, string eventType, string payload, int revision = 1)
    {
        using var document = JsonDocument.Parse(payload);
        return new LegacyEventCommit(
            token,
            streamId.ToString(),
            revision,
            revision,
            DateTime.SpecifyKind(DateTime.Parse("2026-11-14T12:00:00"), DateTimeKind.Utc),
            [new LegacyEventEnvelope(eventType, document.RootElement.Clone())]);
    }
}