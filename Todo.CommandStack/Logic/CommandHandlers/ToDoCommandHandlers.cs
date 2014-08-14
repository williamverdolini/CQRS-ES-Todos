using CommonDomain.Persistence;
using NEventStore;
using System;
using System.Reflection;
using Todo.Domain.Messages.Commands;
using Todo.Domain.Model;
using Todo.Infrastructure;
using Todo.Infrastructure.Commands;
using Todo.Infrastructure.Domain;

namespace Todo.CommandStack.Logic.CommandHandlers
{
    public class ToDoListCommandHandlers : 
        SnapshotCreator<ToDoList>,
        ICommandHandler<CreateToDoListCommand>, 
        ICommandHandler<ChangeToDoListDescriptionCommand>        
    {
        // Repository to get/save Aggregates/Entities from/to Domain Model
        private readonly IRepository _repo;
        // Event Store to Raise (Extra-Domain) Events
        private readonly IStoreEvents _store;

        public ToDoListCommandHandlers(IRepository repository, IStoreEvents eventStore) 
            : base(repository, eventStore)
        {
            //Guard clauses
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            Contract.Requires<ArgumentNullException>(eventStore != null, "eventStore");
            _repo = repository;
            _store = eventStore;
        }

        public void Handle(CreateToDoListCommand command)
        {
            ToDoList todoList = new ToDoList(command.Id, command.Title, command.Description);
            _repo.Save(todoList, Guid.NewGuid());
        }
    
        public void Handle(ChangeToDoListDescriptionCommand command)
        {
            ToDoList list = _repo.GetById<ToDoList>(command.Id);
            list.ChangeDescription(command.Description);
            _repo.Save(list, Guid.NewGuid());
        }
    }

    public class ToDoItemCommandHandlers :
        SnapshotCreator<ToDoItem>,
        ICommandHandler<AddNewToDoItemCommand>,
        ICommandHandler<MarkToDoItemAsCompleteCommand>,
        ICommandHandler<ReOpenToDoItemCommand>,
        ICommandHandler<ChangeToDoItemImportanceCommand>,
        ICommandHandler<ChangeToDoItemDescriptionCommand>,
        ICommandHandler<ChangeToDoItemDueDateCommand>
    {
        // Repository to get/save Aggregates/Entities from/to Domain Model
        private readonly IRepository _repo;
        // Event Store to Raise (Extra-Domain) Events
        private readonly IStoreEvents _store;

        public ToDoItemCommandHandlers(IRepository repository, IStoreEvents eventStore)
            : base(repository, eventStore)
        {
            //Guard clauses
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            Contract.Requires<ArgumentNullException>(eventStore != null, "eventStore");
            _repo = repository;
            _store = eventStore;
        }

        public void Handle(AddNewToDoItemCommand command)
        {
            ToDoItem item = new ToDoItem(command.TodoListId, command.Id, command.CreationDate, command.Description, command.DueDate, command.Importance);
            _repo.Save(item, Guid.NewGuid());
        }

        public void Handle(MarkToDoItemAsCompleteCommand command)
        {
            ToDoItem item = _repo.GetById<ToDoItem>(command.Id);
            item.Close(command.ClosingDate);
            _repo.Save(item, Guid.NewGuid());
        }

        public void Handle(ReOpenToDoItemCommand command)
        {
            ToDoItem item = _repo.GetById<ToDoItem>(command.Id);
            item.ReOpen();
            _repo.Save(item, Guid.NewGuid());
        }

        public void Handle(ChangeToDoItemImportanceCommand command)
        {
            ToDoItem item = _repo.GetById<ToDoItem>(command.Id);
            item.ChangeImportance(command.Importance);
            _repo.Save(item, Guid.NewGuid());
        }

        public void Handle(ChangeToDoItemDescriptionCommand command)
        {
            ToDoItem item = _repo.GetById<ToDoItem>(command.Id);
            item.ChangeDescription(command.Description);
            _repo.Save(item, Guid.NewGuid());
        }

        public void Handle(ChangeToDoItemDueDateCommand command)
        {
            ToDoItem item = _repo.GetById<ToDoItem>(command.Id);
            item.ChangeDueDate(command.DueDate);
            _repo.Save(item, Guid.NewGuid());
        }

    }
}
