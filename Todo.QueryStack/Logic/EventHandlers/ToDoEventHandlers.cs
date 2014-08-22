using System;
using System.Data.Entity;
using System.Linq;
using Todo.Domain.Messages.Events;
using Todo.Infrastructure.Events;
using Todo.QueryStack.Logic.Services;
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
        private readonly IIdentityMapper _identityMapper;

        public ToDoEventHandlers(IIdentityMapper identityMapper)
        {
            _identityMapper = identityMapper;
        }

        public void Handle(CreatedToDoListEvent @event)
        {
            using(var db = new ToDoContext())
            {
                var _list = new Model.ToDoList()
                                {
                                    Title = @event.Title,
                                    Description = @event.Description
                                };
                db.Lists.Add(_list);
                db.SaveChanges();
                _identityMapper.Map<ToDoList>(_list.Id  , @event.ToDoListId);                
            }
        }

        public void Handle(ChangedToDoListDescriptionEvent @event)
        {
            using (var db = new ToDoContext())
            {
                int modelId = _identityMapper.GetModelId<ToDoList>(@event.ToDoListId);
                ToDoList list = db.Lists.First(t => t.Id.Equals(modelId));
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
                int listId = _identityMapper.GetModelId<ToDoList>(@event.ToDoListId);
                ToDoList list = db.Lists.First(t => t.Id.Equals(listId));
                if (list != null)
                {
                    var _item = new Model.ToDoItem()
                    {
                        //Id = @event.ToDoItemId,
                        ToDoListId = listId,
                        Description = @event.Description,
                        CreationDate = @event.CreationDate,
                        DueDate = @event.DueDate,
                        Importance = @event.Importance,
                        ClosingDate = null,
                        UserId = 0
                    };
                    list.Items.Add(_item);
                    db.SaveChanges();
                    _identityMapper.Map<ToDoItem>(_item.Id, @event.ToDoItemId);                

                }
            }
        }

        public void Handle(MarkedToDoItemAsCompletedEvent @event)
        {
            using (var db = new ToDoContext())
            {
                int itemId = _identityMapper.GetModelId<ToDoItem>(@event.Id);
                ToDoItem item = db.Items.First(t => t.Id.Equals(itemId));
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
                int itemId = _identityMapper.GetModelId<ToDoItem>(@event.Id);
                ToDoItem item = db.Items.First(t => t.Id.Equals(itemId));
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
                int itemId = _identityMapper.GetModelId<ToDoItem>(@event.Id);
                ToDoItem item = db.Items.First(t => t.Id.Equals(itemId));
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
                int itemId = _identityMapper.GetModelId<ToDoItem>(@event.Id);
                ToDoItem item = db.Items.First(t => t.Id.Equals(itemId));
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
                int itemId = _identityMapper.GetModelId<ToDoItem>(@event.Id);
                ToDoItem item = db.Items.First(t => t.Id.Equals(itemId));
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
