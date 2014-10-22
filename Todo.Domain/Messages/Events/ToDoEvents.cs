using Todo.Infrastructure.Events;
using System;
using Todo.Infrastructure.Events.Versioning;
using Todo.Domain.Model;

namespace Todo.Domain.Messages.Events
{
    #region ToDoList Events
    public class CreatedToDoListEvent : Event
    {
        public Guid ToDoListId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }

        public CreatedToDoListEvent(Guid toDoListId, string title, string description)
        {
            ToDoListId = toDoListId;
            Title = title;
            Description = description;
        }
    }

    public class ChangedToDoListDescriptionEvent : Event
    {
        public Guid ToDoListId { get; private set; }
        public string Description { get; private set; }

        public ChangedToDoListDescriptionEvent(Guid toDoListId, string description)
        {
            ToDoListId = toDoListId;
            Description = description;
        }
    }
    #endregion

    #region ToDoItem Events
    [VersionedEvent("AddedNewToDoItemEvent", 0)]
    public class AddedNewToDoItemEvent_V0 : Event
    {
        public Guid ToDoListId { get; private set; }
        public Guid ToDoItemId { get; private set; }
        public DateTime CreationDate { get; private set; }
        public string Description { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int Importance { get; private set; }

        public AddedNewToDoItemEvent_V0(Guid toDoListId, Guid toDoItemId, DateTime creationDate, string description, DateTime? dueDate, int importance)
        {
            ToDoListId = toDoListId;
            ToDoItemId = toDoItemId;
            CreationDate = creationDate;
            Description = description;
            DueDate = dueDate;
            Importance = importance;
        }
    }

    [VersionedEvent("AddedNewToDoItemEvent", 1)]
    public class AddedNewToDoItemEvent : Event
    {
        public Guid ToDoListId { get; private set; }
        public Guid ToDoItemId { get; private set; }
        public DateTime CreationDate { get; private set; }
        public string Description { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int Importance { get; private set; }
        public string AssignedTo { get; private set; }

        public AddedNewToDoItemEvent(Guid toDoListId, Guid toDoItemId, DateTime creationDate, string description, DateTime? dueDate, int importance, string assignedTo)
        {
            ToDoListId = toDoListId;
            ToDoItemId = toDoItemId;
            CreationDate = creationDate;
            Description = description;
            DueDate = dueDate;
            Importance = importance;
            AssignedTo = assignedTo;
        }
    }

    [VersionedEvent("MarkedToDoItemAsCompletedEvent", 0)]
    public class MarkedToDoItemAsCompletedEvent_V0 : Event
    {
        public DateTime ClosingDate { get; private set; }

        public MarkedToDoItemAsCompletedEvent_V0(Guid id, DateTime closingDate)
        {
            Id = id;
            ClosingDate = closingDate;
        }
    }

    [VersionedEvent("MarkedToDoItemAsCompletedEvent", 1)]
    public class MarkedToDoItemAsCompletedEvent : Event
    {
        public DateTime ClosingDate { get; private set; }
        public string AssignedTo { get; private set; }

        public MarkedToDoItemAsCompletedEvent(Guid id, DateTime closingDate, string assignedTo)
        {
            Id = id;
            ClosingDate = closingDate;
            AssignedTo = assignedTo;
        }
    }

    public class ReOpenedToDoItemEvent : Event
    {
        public ReOpenedToDoItemEvent(Guid id)
        {
            Id = id;
        }
    }

    public class ChangedToDoItemImportanceEvent : Event
    {
        public int Importance { get; private set; }

        public ChangedToDoItemImportanceEvent(Guid id, int importance)
        {
            Id = id;
            Importance = importance;
        }
    }

    public class ChangedToDoItemDescriptionEvent : Event
    {
        public string Description { get; private set; }

        public ChangedToDoItemDescriptionEvent(Guid id, string description)
        {
            Id = id;
            Description = description;
        }
    }

    public class ChangedToDoItemDueDateEvent : Event
    {
        public DateTime? DueDate { get; private set; }

        public ChangedToDoItemDueDateEvent(Guid id, DateTime? dueDate)
        {
            Id = id;
            DueDate = dueDate;
        }
    }
    #endregion

    #region Memento Propagation Events
    public class ToDoListMementoPropagatedEvent : Event
    {
        public ToDoListMemento Memento { get; private set; }

        public ToDoListMementoPropagatedEvent(ToDoListMemento memento)
        {
            Memento = memento;
        }
    }

    public class ToDoItemMementoPropagatedEvent : Event
    {
        public ToDoItemMemento Memento { get; private set; }

        public ToDoItemMementoPropagatedEvent(ToDoItemMemento memento)
        {
            Memento = memento;
        }
    }
    #endregion

}
