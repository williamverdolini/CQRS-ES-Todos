using Microsoft.EntityFrameworkCore;

namespace CqrsEsTodo.Infrastructure;

public sealed class LegacyEventImportService(CqrsTodoDbContext database)
{
    public async Task<LegacyEventImportResult> ApplyAsync(
        string bundleSha256,
        LegacyEventImportPlan plan,
        CancellationToken cancellationToken)
    {
        if (bundleSha256.Length != 64 || !bundleSha256.All(Uri.IsHexDigit))
        {
            throw new ArgumentException("The bundle ID must be a SHA-256 hex digest.", nameof(bundleSha256));
        }

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        var previousRun = await database.LegacyImportRuns
            .SingleOrDefaultAsync(item => item.BundleSha256 == bundleSha256, cancellationToken);
        if (previousRun is not null)
        {
            return new LegacyEventImportResult(true, previousRun.CommitCount, previousRun.ImportedEventCount, plan.Aggregates.Count);
        }

        if (await database.TodoEvents.AnyAsync(cancellationToken)
            || await database.TodoLists.AnyAsync(cancellationToken)
            || await database.TodoItems.AnyAsync(cancellationToken))
        {
            throw new InvalidOperationException("Legacy import requires an empty event store and read model.");
        }

        foreach (var aggregate in plan.Aggregates)
        {
            for (var index = 0; index < aggregate.Events.Count; index++)
            {
                var importedEvent = aggregate.Events[index];
                var (eventType, payload) = TodoApplicationService.TodoEventCodec.Serialize(importedEvent.Event);
                database.TodoEvents.Add(new StoredTodoEvent
                {
                    AggregateId = aggregate.AggregateId,
                    Version = index + 1,
                    EventType = eventType,
                    Payload = payload,
                    LegacySource = System.Text.Json.JsonSerializer.Serialize(importedEvent.Source),
                    OccurredAt = importedEvent.OccurredAt
                });
            }

            var state = aggregate.State;
            database.TodoLists.Add(new TodoListReadModel
            {
                Id = state.Id,
                Title = state.Title,
                Description = state.Description,
                Version = state.Version,
                Items = state.Items.Select(item => new TodoItemReadModel
                {
                    Id = item.Id,
                    ListId = state.Id,
                    Description = item.Description,
                    UserId = item.UserId,
                    CreatedAt = item.CreatedAt,
                    DueAt = item.DueAt,
                    Importance = item.Importance,
                    CompletedAt = item.CompletedAt
                }).ToList()
            });
        }

        database.LegacyImportRuns.Add(new LegacyImportRun
        {
            BundleSha256 = bundleSha256.ToLowerInvariant(),
            Source = "NEventStore",
            CommitCount = plan.CommitCount,
            SourceEventCount = plan.SourceEventCount,
            ImportedEventCount = plan.ImportedEventCount,
            ImportedAtUtc = DateTimeOffset.UtcNow
        });

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new LegacyEventImportResult(false, plan.CommitCount, plan.ImportedEventCount, plan.Aggregates.Count);
    }
}

public sealed record LegacyEventImportResult(bool AlreadyImported, int CommitCount, int ImportedEventCount, int AggregateCount);