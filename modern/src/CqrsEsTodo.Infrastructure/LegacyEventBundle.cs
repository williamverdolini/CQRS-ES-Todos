using System.Text.Json;

namespace CqrsEsTodo.Infrastructure;

public sealed record LegacyEventBundle(
    int FormatVersion,
    string Source,
    IReadOnlyList<LegacyEventCommit> Commits);

public sealed record LegacyEventCommit(
    string CheckpointToken,
    string StreamId,
    int StreamRevision,
    int CommitSequence,
    DateTime CommitStamp,
    IReadOnlyList<LegacyEventEnvelope> Events,
    JsonElement? Headers = null);

public sealed record LegacyEventEnvelope(string Type, JsonElement Payload, JsonElement? Headers = null);

public sealed record LegacyEventSourceMetadata(
    string CheckpointToken,
    string StreamId,
    int StreamRevision,
    int CommitSequence,
    int EventIndex,
    string EventType,
    string PayloadJson,
    string? CommitHeadersJson,
    string? MessageHeadersJson);

public sealed record LegacyImportedEvent(
    CqrsEsTodo.Domain.TodoEvent Event,
    DateTimeOffset OccurredAt,
    LegacyEventSourceMetadata Source);

public sealed record LegacyAggregateImport(
    Guid AggregateId,
    IReadOnlyList<LegacyImportedEvent> Events,
    CqrsEsTodo.Domain.TodoListAggregate State);

public sealed record LegacyEventImportPlan(
    int CommitCount,
    int SourceEventCount,
    int ImportedEventCount,
    string LastCheckpointToken,
    IReadOnlyList<LegacyAggregateImport> Aggregates);