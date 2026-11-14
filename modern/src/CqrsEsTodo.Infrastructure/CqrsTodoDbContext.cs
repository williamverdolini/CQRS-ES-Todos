using Microsoft.EntityFrameworkCore;

namespace CqrsEsTodo.Infrastructure;

public sealed class CqrsTodoDbContext(DbContextOptions<CqrsTodoDbContext> options) : DbContext(options)
{
    public DbSet<StoredTodoEvent> TodoEvents => Set<StoredTodoEvent>();
    public DbSet<TodoListReadModel> TodoLists => Set<TodoListReadModel>();
    public DbSet<TodoItemReadModel> TodoItems => Set<TodoItemReadModel>();
    public DbSet<LegacyImportRun> LegacyImportRuns => Set<LegacyImportRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var todoEvent = modelBuilder.Entity<StoredTodoEvent>();
        todoEvent.ToTable("TodoEvents");
        todoEvent.HasKey(item => new { item.AggregateId, item.Version });
        todoEvent.Property(item => item.EventType).HasMaxLength(100).IsRequired();
        todoEvent.Property(item => item.Payload).IsRequired();
        todoEvent.Property(item => item.OccurredAt).IsRequired();

        var todoList = modelBuilder.Entity<TodoListReadModel>();
        todoList.ToTable("TodoLists");
        todoList.HasKey(item => item.Id);
        todoList.Property(item => item.Title).HasMaxLength(200).IsRequired();
        todoList.Property(item => item.Description).HasMaxLength(2000).IsRequired();
        todoList.HasMany(item => item.Items).WithOne(item => item.List).HasForeignKey(item => item.ListId);

        var todoItem = modelBuilder.Entity<TodoItemReadModel>();
        todoItem.ToTable("TodoItems");
        todoItem.HasKey(item => item.Id);
        todoItem.Property(item => item.Description).HasMaxLength(2000).IsRequired();
        todoItem.Property(item => item.UserId).HasDefaultValue(0);

        var importRun = modelBuilder.Entity<LegacyImportRun>();
        importRun.ToTable("LegacyImportRuns");
        importRun.HasKey(item => item.BundleSha256);
        importRun.Property(item => item.BundleSha256).HasMaxLength(64).IsRequired();
        importRun.Property(item => item.Source).HasMaxLength(50).IsRequired();
    }
}