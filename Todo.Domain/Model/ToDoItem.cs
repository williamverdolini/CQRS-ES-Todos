using CommonDomain;
using CommonDomain.Core;
using System;
using Todo.Domain.Messages.Events;
using Todo.Infrastructure.Domain;

namespace Todo.Domain.Model
{
    public class ToDoItem : AggregateBase, IMementoCreator
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

        //constructor with IMemento parameter for EventStore Snapshooting
        private ToDoItem(ToDoItemMemento mementoItem)
        {
            Id = mementoItem.Id;
            Version = mementoItem.Version;
            ToDoListId = mementoItem.ToDoListId;
            Description = mementoItem.Description;
            CreationDate = mementoItem.CreationDate;
            DueDate = mementoItem.DueDate;
            Importance = mementoItem.Importance;
            ClosingDate = mementoItem.ClosingDate;
            UserId = mementoItem.UserId;
        }

        #region Add New ToDoItem 
        public ToDoItem(Guid toDoListId, Guid todoItemId, DateTime creationDate, string description, DateTime? dueDate, int importance)
        {
            Id = todoItemId;
            ToDoListId = toDoListId;
            Description = description;
            CreationDate = creationDate;
            DueDate = dueDate;
            Importance = importance;
            ClosingDate = null;
        }
        #endregion

        #region Close ToDoItem
        public void Close(DateTime closingDate)
        {
            ClosingDate = closingDate;
        }
        #endregion

        #region Re-Open ToDoItem
        public void ReOpen()
        {
            ClosingDate = null;
        }
        #endregion

        #region Change Importance
        public void ChangeImportance(int importance)
        {
            Importance = importance;
        }
        #endregion

        #region Change Description
        public void ChangeDescription(string description)
        {
            Description = description;
        }
        #endregion

        #region Change DueDate
        public void ChangeDueDate(DateTime? dueDate)
        {
            DueDate = dueDate;
        }
        #endregion

        #region TodoItem Memento factory
        public IMemento CreateMemento()
        {
            return new ToDoItemMemento(Id, Version, ToDoListId, Description, CreationDate, DueDate, Importance, ClosingDate, UserId);
        }
        #endregion

    }

    public class ToDoItemMemento : IMemento
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid ToDoListId { get; private set; }
        public string Description { get; private set; }
        public DateTime CreationDate { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int Importance { get; private set; }
        public DateTime? ClosingDate { get; private set; }
        public Guid UserId { get; private set; }

        public ToDoItemMemento(Guid id, int version, Guid toDoList, string description, DateTime creationDate, DateTime? dueDate, int importance, DateTime? closingDate, Guid userId)
        {
            Id = id;
            Version = version;
            ToDoListId = toDoList;
            Description = description;
            CreationDate = creationDate;
            DueDate = dueDate;
            Importance = importance;
            ClosingDate = closingDate;
            UserId = userId;
        }
    }


}
