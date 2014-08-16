using CommonDomain;
using CommonDomain.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using Todo.Domain.Messages.Events;
using Todo.Infrastructure.Domain;

namespace Todo.Domain.Model
{
    public class ToDoList : AggregateBase, IMementoCreator
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public IList<ToDoItem> Items { get; private set; }

        //constructor with only id parameter for EventStore
        private ToDoList(Guid toDoListId)
        {
            Id = toDoListId;
            Items = new List<ToDoItem>();
        }

        //constructor with IMemento parameter for EventStore Snapshooting
        private ToDoList(ToDoListMemento mementoItem)
        {
            Id = mementoItem.Id;
            Version = mementoItem.Version;
            Title = mementoItem.Title;
            Description = mementoItem.Description;
            Items = new List<ToDoItem>();
        }

        public ToDoList(Guid toDoListId, string title, string description)
        {
            Id = toDoListId;
            Title = title;
            Description = description;
            CreateToDoList(toDoListId, title, description);
        }

        #region Create New ToDoList
        private void CreateToDoList(Guid toDoListId, string title, string description)
        {
            RaiseEvent(new CreatedToDoListEvent(toDoListId, title, description));
        }

        void Apply(CreatedToDoListEvent @event)
        {
            Id = @event.ToDoListId;
            Title = @event.Title;
            Description = @event.Description;
        }
        #endregion

        #region Change ToDoList Description
        public void ChangeDescription(string description)
        {
            RaiseEvent(new ChangedToDoListDescriptionEvent(this.Id, description));
        }

        void Apply(ChangedToDoListDescriptionEvent @event)
        {
            Description = @event.Description;
        }
        #endregion

        #region Add New ToDoItem 
        public void AddNewItem(ToDoItem item)
        {
            RaiseEvent(new AddedNewToDoItemEvent(item.ToDoListId, item.Id, item.CreationDate, item.Description, item.DueDate, item.Importance));
        }

        void Apply(AddedNewToDoItemEvent @event)
        {
            ToDoItem item = new ToDoItem(@event.ToDoListId, @event.ToDoItemId, @event.CreationDate, @event.Description, @event.DueDate, @event.Importance);
            Items.Add(item);
        }
        #endregion

        #region Mark Item as Complete
        public void CloseItem(Guid todoItemId, DateTime closingDate)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(todoItemId));
            if(todo!= null)
                RaiseEvent(new MarkedToDoItemAsCompletedEvent(todo.Id, closingDate));                
        }

        void Apply(MarkedToDoItemAsCompletedEvent @event)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(@event.Id));
            todo.Close(@event.ClosingDate);
        }
        #endregion

        #region Re-Open Item
        public void ReOpenItem(Guid todoItemId)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(todoItemId));
            if (todo != null)
                RaiseEvent(new ReOpenedToDoItemEvent(todo.Id));
        }

        void Apply(ReOpenedToDoItemEvent @event)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(@event.Id));
            todo.ReOpen();
        }
        #endregion

        #region Change Item Importance
        public void ChangeItemImportance(Guid todoItemId, int importance)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(todoItemId));
            if (todo != null)
                RaiseEvent(new ChangedToDoItemImportanceEvent(todo.Id, importance));
        }

        void Apply(ChangedToDoItemImportanceEvent @event)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(@event.Id));
            todo.ChangeImportance(@event.Importance);
        }
        #endregion

        #region Change Item Description
        public void ChangeItemDescription(Guid todoItemId, string description)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(todoItemId));
            if (todo != null)
                RaiseEvent(new ChangedToDoItemDescriptionEvent(todo.Id, description));
        }

        void Apply(ChangedToDoItemDescriptionEvent @event)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(@event.Id));
            todo.ChangeDescription(@event.Description);
        }
        #endregion

        #region Change Item DueDate
        public void ChangeItemDueDate(Guid todoItemId, DateTime? dueDate)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(todoItemId));
            if (todo != null)
                RaiseEvent(new ChangedToDoItemDueDateEvent(todo.Id, dueDate));
        }

        void Apply(ChangedToDoItemDueDateEvent @event)
        {
            ToDoItem todo = Items.First<ToDoItem>(item => item.Id.Equals(@event.Id));
            todo.ChangeDueDate(@event.DueDate);
        }
        #endregion


        #region TodoItem Memento factory
        public IMemento CreateMemento()
        {
            return new ToDoListMemento(Id, Version, Title, Description);
        }
        #endregion
    }

    public class ToDoListMemento : IMemento
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Title { get; private set; }
        public string Description { get; private set; }

        public ToDoListMemento(Guid id, int version, string title, string description)
        {
            Id = id;
            Version = version;
            Title = title;
            Description = description;
        }
    }


}
