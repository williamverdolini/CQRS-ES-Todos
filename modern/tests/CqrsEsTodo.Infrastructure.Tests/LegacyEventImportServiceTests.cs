using System.Text.Json;
using CqrsEsTodo.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CqrsEsTodo.Infrastructure.Tests;

public sealed class LegacyEventImportServiceTests
{
    [Fact]
    public async Task ApplyAsync_ImportsEventsAndProjectionOnlyOnce()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<CqrsTodoDbContext>().UseSqlite(connection).Options;
        await using var database = new CqrsTodoDbContext(options);
        await database.Database.EnsureCreatedAsync();
        var listId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bundle = new LegacyEventBundle(1, "NEventStore",
        [
            Commit("1", listId, "CreatedToDoListEvent", JsonSerializer.Serialize(new { ToDoListId = listId, Title = "Imported", Description = "Legacy" })),
            Commit("2", itemId, "ToDoItemMementoPropagatedEvent", JsonSerializer.Serialize(new { Memento = new { Id = itemId, Version = 1, ToDoListId = listId, Description = "Item", CreationDate = DateTimeOffset.Parse("2026-11-12T08:30:00Z"), DueDate = (DateTimeOffset?)null, Importance = 3, ClosingDate = (DateTimeOffset?)null, UserId = 12 } }))
        ]);
        var plan = LegacyEventImportPlanner.CreatePlan(bundle);
        var service = new LegacyEventImportService(database);
        var bundleHash = new string('a', 64);

        var first = await service.ApplyAsync(bundleHash, plan, CancellationToken.None);
        var second = await service.ApplyAsync(bundleHash, plan, CancellationToken.None);
        var list = await new TodoApplicationService(database).GetListAsync(listId, CancellationToken.None);

        Assert.False(first.AlreadyImported);
        Assert.True(second.AlreadyImported);
        Assert.Equal(2, await database.TodoEvents.CountAsync());
        Assert.Equal(1, await database.LegacyImportRuns.CountAsync());
        Assert.Equal(12, Assert.Single(list!.Items).UserId);

        var importedEvent = await database.TodoEvents.SingleAsync(item => item.EventType == "todo-item-added.v1");
        using var source = JsonDocument.Parse(importedEvent.LegacySource!);
        Assert.Equal("2", source.RootElement.GetProperty("CheckpointToken").GetString());
        Assert.Equal("ToDoItemMementoPropagatedEvent", source.RootElement.GetProperty("EventType").GetString());
        Assert.Contains("Item", source.RootElement.GetProperty("PayloadJson").GetString());
    }

    [Fact]
    public async Task ApplyAsync_RefusesANonEmptyTargetStore()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<CqrsTodoDbContext>().UseSqlite(connection).Options;
        await using var database = new CqrsTodoDbContext(options);
        await database.Database.EnsureCreatedAsync();
        database.TodoEvents.Add(new StoredTodoEvent
        {
            AggregateId = Guid.NewGuid(),
            Version = 1,
            EventType = "test",
            Payload = "{}",
            OccurredAt = DateTimeOffset.UtcNow
        });
        await database.SaveChangesAsync();

        var bundle = new LegacyEventBundle(1, "NEventStore", [Commit("1", Guid.NewGuid(), "CreatedToDoListEvent", "{}")]);
        var plan = LegacyEventImportPlanner.CreatePlan(bundle);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new LegacyEventImportService(database).ApplyAsync(new string('b', 64), plan, CancellationToken.None));
    }

    private static LegacyEventCommit Commit(string token, Guid streamId, string eventType, string payload)
    {
        using var document = JsonDocument.Parse(payload);
        return new LegacyEventCommit(token, streamId.ToString(), 1, 1, DateTime.UtcNow, [new LegacyEventEnvelope(eventType, document.RootElement.Clone())]);
    }
}