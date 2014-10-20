using CommonDomain.Persistence;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Todo.Domain.Messages.Commands;
using Todo.Infrastructure;
using Todo.Infrastructure.Events.Rebuilding;
using Todo.QueryStack;
using Todo.QueryStack.Model;
using Web.UI.Models.TodoList;

namespace Web.UI.Worker
{
    public class ToDoWorker
    {
        private readonly IBus bus;
        private readonly IDatabase database;
        private readonly IEventsRebuilder rebuilder;

        public ToDoWorker(IBus commandBus, IRepository repo, IDatabase db, IEventsRebuilder eventsRebuilder)
        {
            Contract.Requires<ArgumentNullException>(commandBus != null, "commandBus");
            Contract.Requires<ArgumentNullException>(db != null, "db");
            Contract.Requires<ArgumentNullException>(eventsRebuilder != null, "eventsRebuilder");
            bus = commandBus;
            database = db;
            rebuilder = eventsRebuilder;
        }

        #region Command Responsibility
        public void CreateToDoList(CreateTodoListCommandModel model)
        {
            bus.Send<CreateToDoListCommand>(new CreateToDoListCommand(model.Id, model.Title, model.Description));
        }

        public void ChangeToDoListDescription(ChangeTodoListDescriptionCommandModel model)
        {
            string todoListId = database.IdMaps.GetAggregateId<ToDoList>(int.Parse(model.Id)).ToString();
            bus.Send<ChangeToDoListDescriptionCommand>(new ChangeToDoListDescriptionCommand(todoListId, model.Description));
        }

        public void AddNewToDoItem(AddNewToDoItemCommandModel model)
        {
            string todoListId = database.IdMaps.GetAggregateId<ToDoList>(int.Parse(model.ToDoListId)).ToString();
            bus.Send<AddNewToDoItemCommand>(new AddNewToDoItemCommand(todoListId, model.Id, model.CreationDate, model.Description, model.DueDate, model.Importance));
            string _newId = database.IdMaps.GetModelId<ToDoItem>(new Guid(model.Id)).ToString();
            model.Id = _newId;
        }

        public void MarkToDoItemAsComplete(MarkToDoItemAsCompleteModel model)
        {
            string todoItemId = database.IdMaps.GetAggregateId<ToDoItem>(int.Parse(model.Id)).ToString();
            bus.Send<MarkToDoItemAsCompleteCommand>(new MarkToDoItemAsCompleteCommand(todoItemId, model.ClosingDate));
        }

        public void ReOpenToDoItem(ReOpenToDoItemModel model)
        {
            string todoItemId = database.IdMaps.GetAggregateId<ToDoItem>(int.Parse(model.Id)).ToString();
            bus.Send<ReOpenToDoItemCommand>(new ReOpenToDoItemCommand(todoItemId));
        }

        public void ChangeImportance(ChangeToDoItemImportanceModel model)
        {
            string todoItemId = database.IdMaps.GetAggregateId<ToDoItem>(int.Parse(model.Id)).ToString();
            bus.Send<ChangeToDoItemImportanceCommand>(new ChangeToDoItemImportanceCommand(todoItemId, model.Importance));
        }

        public void ChangeDescription(ChangeToDoItemDescriptionModel model)
        {
            string todoItemId = database.IdMaps.GetAggregateId<ToDoItem>(int.Parse(model.Id)).ToString();
            bus.Send<ChangeToDoItemDescriptionCommand>(new ChangeToDoItemDescriptionCommand(todoItemId, model.Description));
        }

        public void ChangeDueDate(ChangeToDoItemDueDateModel model)
        {
            string todoItemId = database.IdMaps.GetAggregateId<ToDoItem>(int.Parse(model.Id)).ToString();
            bus.Send<ChangeToDoItemDueDateCommand>(new ChangeToDoItemDueDateCommand(todoItemId, model.DueDate));
        }

        public void EventsRebuild()
        {
            rebuilder.Rebuild();
        }
        #endregion

        #region Query Responsibility
        public async Task<List<ToDoList>> GetLists()
        {
            return await database.ToDoLists.ToListAsync();
        }

        public async Task<ToDoList> GetListItems(string Id)
        {
            //Eagger loading of List's items
            int modelId = int.Parse(Id);
            return await database.ToDoLists.Include(l => l.Items).FirstAsync(t => t.Id.Equals(modelId));
        }

        public async Task<ToDoItem> GetToDoItem(string Id)
        {
            int modelId = int.Parse(Id);
            return await database.ToDoItems.FirstAsync(t => t.Id.Equals(modelId));
        }
        #endregion
    }
}