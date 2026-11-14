# CQRS-ES Todos on .NET 10

This is the modern replacement track. The original .NET Framework solution remains at the repository root while this vertical slice is evaluated.

## Run the API and SQL Server

From this directory, start the development stack with:

```powershell
docker compose --env-file .env.example up --build
```

The API is available at `http://localhost:8080`. OpenAPI is available at `http://localhost:8080/openapi/v1.json`; `/health/live` and `/health/ready` report process and database health. EF Core applies checked-in migrations in Development and rebuilds the query projection from stored events when a migration is pending.

For a local environment, copy `.env.example` to `.env` and replace the development-only SQL Server password. Do not use the example credentials outside a disposable local environment.

## Run tests

From the repository root:

```powershell
dotnet test modern/CqrsEsTodo.Modern.sln
```

Commands append versioned domain events and update the SQL query-side projection in the same transaction. Reads use the projection; the maintenance endpoint can rebuild it by replaying the event streams. This is a synchronous projection for the first migration slice, not a separate background worker.

## API

- `GET /api/lists`
- `POST /api/lists`
- `GET /api/lists/{id}`
- `GET /api/items/{id}`
- `PUT /api/lists/{id}/description`
- `POST /api/lists/{id}/items`
- `PUT /api/lists/{listId}/items/{itemId}/completion`
- `PUT /api/lists/{listId}/items/{itemId}/importance`
- `PUT /api/lists/{listId}/items/{itemId}/due-date`
- `PUT /api/lists/{listId}/items/{itemId}/description`
- `POST /api/maintenance/rebuild-read-model`

The rebuild endpoint is only mapped in the Development environment.

## Import legacy events

1. Build `Todo.Legacy.EventExporter` from `CqrsEsTodo.sln` on Windows with the .NET Framework 4.5.1 Developer Pack and restored NuGet packages. Set its `EventStore` connection string in `Todo.Legacy.EventExporter/App.config` to the legacy database.
2. Run `Todo.Legacy.EventExporter.exe --output <new-bundle-path>`. It reads NEventStore commits in checkpoint order, applies the legacy v0 event upconverters, and refuses to overwrite an existing bundle.
3. Validate the output without a database connection:

```powershell
dotnet run --project modern/tools/CqrsEsTodo.LegacyImporter -- --bundle <bundle-path>
```

4. Point `ConnectionStrings__TodoDb` at a separate, empty SQL Server database and apply the modern schema migrations:

```powershell
$env:ConnectionStrings__TodoDb = 'Server=127.0.0.1,1433;Database=CqrsEsTodoImport;User Id=sa;Password=<local-secret>;Encrypt=True;TrustServerCertificate=True'
modern/.tools/dotnet-ef.exe database update --project modern/src/CqrsEsTodo.Infrastructure/CqrsEsTodo.Infrastructure.csproj --startup-project modern/src/CqrsEsTodo.Api/CqrsEsTodo.Api.csproj
```

5. Apply only after reviewing the dry-run summary:

```powershell
dotnet run --project modern/tools/CqrsEsTodo.LegacyImporter -- --bundle <bundle-path> --apply
```

The importer rejects a target containing any events or projection rows, records the bundle SHA-256 in `LegacyImportRuns`, and treats a repeat import of the exact bundle as a no-op. Each flattened event also stores its original checkpoint, stream/revision, event type, payload, and headers in `TodoEvents.LegacySource`. Do not point it at the populated development database or discard the legacy database before reconciling imported list/item counts and rebuilding the projection.

The sample at [examples/legacy-event-bundle.sample.json](examples/legacy-event-bundle.sample.json) can be used to exercise dry-run validation. The exporter itself requires the legacy Framework targeting pack, which is not installed in the current environment.