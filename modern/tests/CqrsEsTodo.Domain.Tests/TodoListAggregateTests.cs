using CqrsEsTodo.Domain;

namespace CqrsEsTodo.Domain.Tests;

public sealed class TodoListAggregateTests
{
    [Fact]
    public void Create_EmitsEventAndSetsState()
    {
        var list = TodoListAggregate.Create(Guid.NewGuid(), "Work", "Things to do");

        Assert.True(list.IsCreated);
        Assert.Equal("Work", list.Title);
        Assert.Equal("Things to do", list.Description);
        Assert.IsType<TodoListCreated>(Assert.Single(list.UncommittedEvents));
    }

    [Fact]
    public void Rehydrate_RestoresItemsAndCompletionFromHistory()
    {
        var listId = Guid.NewGuid();
        var list = TodoListAggregate.Create(listId, "Work", "Things to do");
        var itemId = list.AddItem("Review pull request", null, 2, DateTimeOffset.UtcNow, userId: 42, assignedTo: "legacy-assignee");
        list.CompleteItem(itemId, DateTimeOffset.UtcNow);

        var restored = TodoListAggregate.Rehydrate(listId, list.UncommittedEvents);

        var item = Assert.Single(restored.Items);
        Assert.Equal(itemId, item.Id);
        Assert.NotNull(item.CompletedAt);
        Assert.Equal(3, restored.Version);
        Assert.Empty(restored.UncommittedEvents);
    }

    [Fact]
    public void Create_RejectsAnEmptyTitle()
    {
        Assert.Throws<ArgumentException>(() => TodoListAggregate.Create(Guid.NewGuid(), " ", "Description"));
    }

    [Fact]
    public void CompleteItem_RejectsCompletingAnItemTwice()
    {
        var list = TodoListAggregate.Create(Guid.NewGuid(), "Work", "Things to do");
        var itemId = list.AddItem("Review pull request", null, 0, DateTimeOffset.UtcNow);
        list.CompleteItem(itemId, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => list.CompleteItem(itemId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ItemUpdates_EmitEventsAndReplayCurrentValues()
    {
        var listId = Guid.NewGuid();
        var list = TodoListAggregate.Create(listId, "Work", "Things to do");
        var itemId = list.AddItem("Review pull request", null, 2, DateTimeOffset.UtcNow, userId: 42, assignedTo: "legacy-assignee");
        var dueAt = DateTimeOffset.UtcNow.AddDays(2);

        list.ChangeItemImportance(itemId, 5);
        list.ChangeItemDueDate(itemId, dueAt);
        list.ChangeItemDescription(itemId, "Ship reviewed changes");
        var restored = TodoListAggregate.Rehydrate(listId, list.UncommittedEvents);

        var item = Assert.Single(restored.Items);
        Assert.Equal(5, item.Importance);
        Assert.Equal(dueAt, item.DueAt);
        Assert.Equal("Ship reviewed changes", item.Description);
        Assert.Equal(42, item.UserId);
        Assert.Equal(5, restored.Version);
    }
}