using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Todo.Domain.Messages.Events;
using Todo.Infrastructure;
using Todo.Infrastructure.Events;
using Todo.QueryStack.Logic.Hubs;
using Todo.QueryStack.Logic.Services;
using Todo.QueryStack.Model;

namespace Todo.QueryStack.Logic.EventHandlers
{
    public class ToDoEventHandlers : 
        IEventHandler<CreatedToDoListEvent>, 
        IEventHandler<ChangedToDoListDescriptionEvent>,
        IEventHandler<ToDoListMementoPropagatedEvent>,
        IEventHandler<AddedNewToDoItemEvent>,
        IEventHandler<MarkedToDoItemAsCompletedEvent>,
        IEventHandler<ReOpenedToDoItemEvent>,
        IEventHandler<ChangedToDoItemImportanceEvent>,
        IEventHandler<ChangedToDoItemDescriptionEvent>,
        IEventHandler<ChangedToDoItemDueDateEvent>,
        IEventHandler<ToDoItemMementoPropagatedEvent>
    {
        private readonly IIdentityMapper _identityMapper;
        private readonly IEventNotifier notifier;

        public ToDoEventHandlers(IIdentityMapper identityMapper, IEventNotifier notifier)
        {
            Contract.Requires<ArgumentNullException>(identityMapper != null, "identityMapper");
            Contract.Requires<ArgumentNullException>(notifier != null, "notifier");
            _identityMapper = identityMapper;
            this.notifier = notifier;
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

                Task.Run(() => notifier.CreatedToDoListEventNotify(_list)).ConfigureAwait(false);
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

                    Task.Run(() => notifier.ChangedToDoListDescriptionEventNotify(list)).ConfigureAwait(false);
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

                    Task.Run(() => notifier.AddedNewToDoItemEventNotify(_item)).ConfigureAwait(false);
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

                    Task.Run(() => notifier.MarkedToDoItemAsCompletedEventNotify(item)).ConfigureAwait(false);
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

                    Task.Run(() => notifier.ReOpenedToDoItemEventNotify(item)).ConfigureAwait(false);
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

                    Task.Run(() => notifier.ChangedToDoItemImportanceEventNotify(item)).ConfigureAwait(false);
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

                    Task.Run(() => notifier.ChangedToDoItemDescriptionEventNotify(item)).ConfigureAwait(false);
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

                    Task.Run(() => notifier.ChangedToDoItemDueDateEventNotify(item)).ConfigureAwait(false);
                }
            }
        }

        public void Handle(ToDoListMementoPropagatedEvent @event)
        {
            using (var db = new ToDoContext())
            {
                int itemId = _identityMapper.GetModelId<ToDoList>(@event.Memento.Id);
                if (itemId.Equals(0))
                {
                    //ToDoList Not exists
                    var _list = new Model.ToDoList()
                    {
                        Title = @event.Memento.Title,
                        Description = @event.Memento.Description
                    };
                    db.Lists.Add(_list);
                    db.SaveChanges();
                    _identityMapper.Map<ToDoList>(_list.Id, @event.Memento.Id);
                }
                // otherwise it could be used for maintenance purposes


            }
        }

        public void Handle(ToDoItemMementoPropagatedEvent @event)
        {
            using (var db = new ToDoContext())
            {
                int itemId = _identityMapper.GetModelId<ToDoItem>(@event.Memento.Id);
                if (itemId.Equals(0))
                {
                    int listId = _identityMapper.GetModelId<ToDoList>(@event.Memento.ToDoListId);
                    ToDoList list = db.Lists.First(t => t.Id.Equals(listId));
                    var _item = new Model.ToDoItem()
                    {
                        //Id = @event.ToDoItemId,
                        ToDoListId = listId,
                        Description = @event.Memento.Description,
                        CreationDate = @event.Memento.CreationDate,
                        DueDate = @event.Memento.DueDate,
                        Importance = @event.Memento.Importance,
                        ClosingDate = @event.Memento.ClosingDate,
                        UserId = @event.Memento.UserId
                    };
                    list.Items.Add(_item);
                    db.SaveChanges();
                    _identityMapper.Map<ToDoItem>(_item.Id, @event.Memento.Id);
                }

            }
        }
    }
}
