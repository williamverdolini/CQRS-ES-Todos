using CommonDomain.Core;
using System;
using Todo.Domain.Messages.Events;


namespace Todo.Domain.Model
{
    public class ToDoItem : AggregateBase
    {
        public Guid ToDoListId { get; private set; }
        public string Description { get; private set; }
        public DateTime CreationDate { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int Importance { get; private set; }
        public DateTime? ClosingDate { get; private set; }
        public Guid UserId { get; private set; }

        //constructor with only id parameter for EventStore
        private ToDoItem(Guid toDoItemId)
        {
            Id = toDoItemId;
        }

        #region Add New ToDoItem 
        public ToDoItem(Guid toDoListId, Guid todoItemId, DateTime creationDate, string description, DateTime? dueDate, int importance)
        {
            RaiseEvent(new AddedNewToDoItemEvent(toDoListId, todoItemId, creationDate, description, dueDate, importance));
        }

        void Apply(AddedNewToDoItemEvent @event)
        {
            Id = @event.ToDoItemId;
            ToDoListId = @event.ToDoListId;
            Description = @event.Description;
            CreationDate = @event.CreationDate;
            DueDate = @event.DueDate;
            Importance = @event.Importance;
            ClosingDate = null;
        }
        #endregion

        #region Close ToDoItem
        public void Close(DateTime closingDate)
        {
            RaiseEvent(new MarkedToDoItemAsCompletedEvent(Id, closingDate));
        }

        void Apply(MarkedToDoItemAsCompletedEvent @event)
        {
            ClosingDate = @event.ClosingDate;
        }
        #endregion

        #region Re-Open ToDoItem
        public void ReOpen()
        {
            RaiseEvent(new ReOpenedToDoItemEvent(Id));
        }

        void Apply(ReOpenedToDoItemEvent @event)
        {
            ClosingDate = null;
        }
        #endregion

        #region Change Importance
        public void ChangeImportance(int Importance)
        {
            RaiseEvent(new ChangedToDoItemImportanceEvent(Id, Importance));
        }

        void Apply(ChangedToDoItemImportanceEvent @event)
        {
            Importance = @event.Importance;
        }
        #endregion

        #region Change Description
        public void ChangeDescription(string Description)
        {
            RaiseEvent(new ChangedToDoItemDescriptionEvent(Id, Description));
        }

        void Apply(ChangedToDoItemDescriptionEvent @event)
        {
            Description = @event.Description;
        }
        #endregion

        #region Change DueDate
        public void ChangeDueDate(DateTime? DueDate)
        {
            RaiseEvent(new ChangedToDoItemDueDateEvent(Id, DueDate));
        }

        void Apply(ChangedToDoItemDueDateEvent @event)
        {
            DueDate = @event.DueDate;
        }
        #endregion

        /*
        public void Assign(Guid id, Guid userId)
        {
            RaiseEvent(new AssignedToDoItemEvent(id, userId));
        }

        void Apply(AssignedToDoItemEvent @event)
        {
            UserId = @event.UserId;
        }

        public void Review(Guid id, string description, DateTime dueDate, int importance)
        {
            RaiseEvent(new ReviewedToDoItemEvent(id, description, dueDate, importance));
        }

        public void Apply(ReviewedToDoItemEvent @event)
        {
            Description = @event.Description;
            DueDate = @event.DueDate;
            Importance = @event.Importance;
        }
        */
    }
}
