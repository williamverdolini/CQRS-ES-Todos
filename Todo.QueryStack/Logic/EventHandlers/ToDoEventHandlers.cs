using System;
using System.Data.Entity;
using System.Linq;
using Todo.Domain.Messages.Events;
using Todo.Infrastructure.Events;
using Todo.QueryStack.Model;

namespace Todo.QueryStack.Logic.EventHandlers
{
    public class ToDoEventHandlers : 
        IEventHandler<CreatedToDoListEvent>, 
        IEventHandler<ChangedToDoListDescriptionEvent>,
        IEventHandler<AddedNewToDoItemEvent>,
        IEventHandler<MarkedToDoItemAsCompletedEvent>,
        IEventHandler<ReOpenedToDoItemEvent>,
        IEventHandler<ChangedToDoItemImportanceEvent>,
        IEventHandler<ChangedToDoItemDescriptionEvent>,
        IEventHandler<ChangedToDoItemDueDateEvent>
    {
        public void Handle(CreatedToDoListEvent @event)
        {
            using(var db = new ToDoContext())
            {
                db.Lists.Add(new Model.ToDoList()
                                {
                                    Id = @event.ToDoListId,
                                    Title = @event.Title,
                                    Description = @event.Description
                                });
                db.SaveChanges();
            }
        }

        public void Handle(ChangedToDoListDescriptionEvent @event)
        {
            using (var db = new ToDoContext())
            {
                ToDoList list = db.Lists.First(t => t.Id.Equals(@event.ToDoListId));
                if (list != null)
                {
                    list.Description = @event.Description;
                    db.Entry(list).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public void Handle(AddedNewToDoItemEvent @event)
        {
            using (var db = new ToDoContext())
            {
                ToDoList list = db.Lists.First(t => t.Id.Equals(@event.ToDoListId));
                if (list != null)
                {
                    list.Items.Add(new Model.ToDoItem()
                    {
                        ToDoListId = @event.ToDoListId,
                        Id = @event.ToDoItemId,
                        Description = @event.Description,
                        CreationDate = @event.CreationDate,
                        DueDate = @event.DueDate,
                        Importance = @event.Importance,
                        ClosingDate = null,
                        UserId = 0
                    });
                    db.SaveChanges();
                }
            }
        }

        public void Handle(MarkedToDoItemAsCompletedEvent @event)
        {
            using (var db = new ToDoContext())
            {
                ToDoItem item = db.Items.First(t => t.Id.Equals(@event.Id));
                if (item != null)
                {
                    item.ClosingDate = @event.ClosingDate;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public void Handle(ReOpenedToDoItemEvent @event)
        {
            using (var db = new ToDoContext())
            {
                ToDoItem item = db.Items.First(t => t.Id.Equals(@event.Id));
                if (item != null)
                {
                    item.ClosingDate = null;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public void Handle(ChangedToDoItemImportanceEvent @event)
        {
            using (var db = new ToDoContext())
            {
                ToDoItem item = db.Items.First(t => t.Id.Equals(@event.Id));
                if (item != null)
                {
                    item.Importance = @event.Importance;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public void Handle(ChangedToDoItemDescriptionEvent @event)
        {
            using (var db = new ToDoContext())
            {
                ToDoItem item = db.Items.First(t => t.Id.Equals(@event.Id));
                if (item != null)
                {
                    item.Description = @event.Description;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public void Handle(ChangedToDoItemDueDateEvent @event)
        {
            using (var db = new ToDoContext())
            {
                ToDoItem item = db.Items.First(t => t.Id.Equals(@event.Id));
                if (item != null)
                {
                    item.DueDate = @event.DueDate;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
    }
}
