using System.Security.Cryptography;
using System.Text.Json;
using CqrsEsTodo.Infrastructure;
using Microsoft.EntityFrameworkCore;

return await RunAsync(args);

static async Task<int> RunAsync(string[] arguments)
{
	try
	{
		if (arguments.Length == 0 || arguments.Contains("--help"))
		{
			PrintUsage();
			return arguments.Length == 0 ? 2 : 0;
		}

		if (arguments.Length < 2
			|| arguments.Length > 3
			|| arguments[0] != "--bundle"
			|| (arguments.Length == 3 && arguments[2] != "--apply"))
		{
			throw new ArgumentException("Use --bundle <path> with an optional trailing --apply.");
		}

		var bundleBytes = await File.ReadAllBytesAsync(arguments[1]);
		var bundle = JsonSerializer.Deserialize<LegacyEventBundle>(bundleBytes, new JsonSerializerOptions(JsonSerializerDefaults.Web)
		{
			PropertyNameCaseInsensitive = true
		}) ?? throw new InvalidDataException("The bundle file is empty or invalid JSON.");

		var plan = LegacyEventImportPlanner.CreatePlan(bundle);
		var bundleHash = Convert.ToHexString(SHA256.HashData(bundleBytes)).ToLowerInvariant();

		Console.WriteLine($"Bundle SHA-256: {bundleHash}");
		Console.WriteLine($"Commits: {plan.CommitCount}; source events: {plan.SourceEventCount}; imported events: {plan.ImportedEventCount}; lists: {plan.Aggregates.Count}");
		Console.WriteLine($"Last checkpoint: {plan.LastCheckpointToken}");

		if (arguments.Length != 3)
		{
			Console.WriteLine("DRY RUN: no database connection opened and no data written. Re-run with --apply to import into an empty, migrated database.");
			return 0;
		}

		var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TodoDb");
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException("Set ConnectionStrings__TodoDb before using --apply.");
		}

		var options = new DbContextOptionsBuilder<CqrsTodoDbContext>()
			.UseSqlServer(connectionString)
			.Options;
		await using var database = new CqrsTodoDbContext(options);
		if (!await database.Database.CanConnectAsync())
		{
			throw new InvalidOperationException("The target SQL Server is not reachable.");
		}

		var pendingMigrations = await database.Database.GetPendingMigrationsAsync();
		if (pendingMigrations.Any())
		{
			throw new InvalidOperationException("Apply all modern database migrations before running the import.");
		}

		var result = await new LegacyEventImportService(database).ApplyAsync(bundleHash, plan, CancellationToken.None);
		Console.WriteLine(result.AlreadyImported
			? "This exact bundle was already imported; no data was written."
			: $"Imported {result.ImportedEventCount} events into {result.AggregateCount} list streams.");
		return 0;
	}
	catch (Exception exception)
	{
		Console.Error.WriteLine($"Import failed: {exception.Message}");
		return 1;
	}
}

static void PrintUsage()
{
	Console.WriteLine("Usage: CqrsEsTodo.LegacyImporter --bundle <path> [--apply]");
	Console.WriteLine("Without --apply, validates and summarizes the bundle without connecting to a database.");
}
