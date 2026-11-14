using CqrsEsTodo.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CqrsEsTodo.Infrastructure.Tests;

public sealed class TodoApplicationServiceTests
{
    [Fact]
    public async Task Commands_UpdateEventStreamAndQueryProjection()
    {
        await using var connection = await CreateConnectionAsync();
        var options = CreateOptions(connection);
        await using var database = new CqrsTodoDbContext(options);
        await database.Database.EnsureCreatedAsync();
        var service = new TodoApplicationService(database);

        var list = await service.CreateListAsync("Work", "Things to do", CancellationToken.None);
        var withItem = await service.AddItemAsync(list.Id, "Review pull request", null, 2, CancellationToken.None);
        var item = Assert.Single(withItem!.Items);
        var dueAt = DateTimeOffset.UtcNow.AddDays(1);
        await service.ChangeItemDescriptionAsync(list.Id, item.Id, "Ship reviewed changes", CancellationToken.None);
        await service.ChangeItemImportanceAsync(list.Id, item.Id, 5, CancellationToken.None);
        await service.ChangeItemDueDateAsync(list.Id, item.Id, dueAt, CancellationToken.None);
        await service.CompleteItemAsync(list.Id, item.Id, CancellationToken.None);
        var queried = await service.GetListAsync(list.Id, CancellationToken.None);
        var queriedItem = await service.GetItemAsync(item.Id, CancellationToken.None);

        Assert.Equal(6, await database.TodoEvents.CountAsync());
        Assert.Equal(6, queried!.Version);
        var projectedItem = Assert.Single(queried.Items);
        Assert.Equal("Ship reviewed changes", projectedItem.Description);
        Assert.Equal(5, projectedItem.Importance);
        Assert.Equal(dueAt, projectedItem.DueAt);
        Assert.NotNull(projectedItem.CompletedAt);
        Assert.Equal(list.Id, queriedItem!.ListId);
    }

    [Fact]
    public async Task RebuildReadModel_ReplaysStoredEvents()
    {
        await using var connection = await CreateConnectionAsync();
        var options = CreateOptions(connection);
        await using var database = new CqrsTodoDbContext(options);
        await database.Database.EnsureCreatedAsync();
        var service = new TodoApplicationService(database);

        var list = await service.CreateListAsync("Work", "Things to do", CancellationToken.None);
        var withItem = await service.AddItemAsync(list.Id, "Review pull request", null, 2, CancellationToken.None);
        var item = Assert.Single(withItem!.Items);
        await service.ChangeItemImportanceAsync(list.Id, item.Id, 5, CancellationToken.None);
        database.TodoLists.RemoveRange(await database.TodoLists.ToListAsync());
        await database.SaveChangesAsync();

        Assert.Null(await service.GetListAsync(list.Id, CancellationToken.None));

        await service.RebuildReadModelAsync(CancellationToken.None);
        var rebuilt = await service.GetListAsync(list.Id, CancellationToken.None);

        Assert.Equal(3, rebuilt!.Version);
        Assert.Equal(5, Assert.Single(rebuilt.Items).Importance);
    }

    private static async Task<SqliteConnection> CreateConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static DbContextOptions<CqrsTodoDbContext> CreateOptions(SqliteConnection connection) =>
        new DbContextOptionsBuilder<CqrsTodoDbContext>()
            .UseSqlite(connection)
            .Options;
}