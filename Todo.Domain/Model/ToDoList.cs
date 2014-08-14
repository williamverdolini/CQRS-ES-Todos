using CommonDomain;
using CommonDomain.Core;
using System;
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
        }

        //constructor with IMemento parameter for EventStore Snapshooting
        private ToDoList(ToDoListMemento mementoItem)
        {
            Id = mementoItem.Id;
            Version = mementoItem.Version;
            Title = mementoItem.Title;
            Description = mementoItem.Description;
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
