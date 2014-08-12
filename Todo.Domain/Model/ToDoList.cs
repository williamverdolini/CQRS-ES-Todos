using CommonDomain.Core;
using System;
using System.Collections.Generic;
using Todo.Domain.Messages.Events;

namespace Todo.Domain.Model
{
    public class ToDoList : AggregateBase
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public IList<ToDoItem> Items { get; private set; }

        private ToDoList(Guid toDoListId)
        {
            Id = toDoListId;
        }

        public ToDoList(Guid toDoListId, string title, string description)
        {
            Id = toDoListId;
            Title = title;
            Description = description;
            CreateToDoList(toDoListId, title, description);
        }

        //static public ToDoList Crea(Guid toDoListId, string title, string description, IServiceCheckUniqueToDoList){

        //}

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

        public void ChangeDescription(string description)
        {
            RaiseEvent(new ChangedToDoListDescriptionEvent(this.Id, description));
        }

        void Apply(ChangedToDoListDescriptionEvent @event)
        {
            Description = @event.Description;
        }

    }
}
