using Todo.Infrastructure.Events;
using System;

namespace Todo.Domain.Messages.Events
{
    // ToDoList Events
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


    // ToDoItem Events
    public class AddedNewToDoItemEvent : Event
    {
        public Guid ToDoListId { get; private set; }
        public Guid ToDoItemId { get; private set; }
        public DateTime CreationDate { get; private set; }
        public string Description { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int Importance { get; private set; }

        public AddedNewToDoItemEvent(Guid toDoListId, Guid toDoItemId, DateTime creationDate, string description, DateTime? dueDate, int importance)
        {
            ToDoListId = toDoListId;
            ToDoItemId = toDoItemId;
            CreationDate = creationDate;
            Description = description;
            DueDate = dueDate;
            Importance = importance;
        }
    }

    public class MarkedToDoItemAsCompletedEvent : Event
    {
        public DateTime ClosingDate { get; private set; }

        public MarkedToDoItemAsCompletedEvent(Guid id, DateTime closingDate)
        {
            Id = id;
            ClosingDate = closingDate;
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

    /*
    public class ReviewedToDoItemEvent : Event
    {
        public string Description { get; private set; }
        public DateTime DueDate { get; private set; }
        public int Importance { get; private set; }

        public ReviewedToDoItemEvent(Guid id, string description, DateTime dueDate, int importance)
        {
            Id = id;
            DueDate = dueDate;
            Importance = importance;
        }
    }

    public class AssignedToDoItemEvent : Event
    {
        public Guid UserId { get; private set; }

        public AssignedToDoItemEvent(Guid id, Guid userId)
        {
            Id = id;
            UserId = userId;
        }
    }
    */
}
