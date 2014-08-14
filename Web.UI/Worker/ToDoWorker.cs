using CommonDomain.Persistence;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Todo.Domain.Messages.Commands;
using Todo.Infrastructure;
using Todo.QueryStack;
using Todo.QueryStack.Model;
using Web.UI.Models.TodoList;

namespace Web.UI.Worker
{
    public class ToDoWorker
    {
        private readonly IBus bus;
        private readonly IDatabase database;

        public ToDoWorker(IBus commandBus, IRepository repo, IDatabase db)
        {
            Contract.Requires<ArgumentNullException>(commandBus != null, "commandBus");
            Contract.Requires<ArgumentNullException>(db != null, "db");

            bus = commandBus;
            database = db;
        }

        #region Command Responsibility
        public void CreateToDoList(CreateTodoListCommandModel model)
        {
            bus.Send<CreateToDoListCommand>(new CreateToDoListCommand(model.Id, model.Title, model.Description));
        }

        public void ChangeToDoListDescription(ChangeTodoListDescriptionCommandModel model)
        {
            bus.Send<ChangeToDoListDescriptionCommand>(new ChangeToDoListDescriptionCommand(model.Id, model.Description));
        }

        public void AddNewToDoItem(AddNewToDoItemCommandModel model)
        {
            bus.Send<AddNewToDoItemCommand>(new AddNewToDoItemCommand(model.ToDoListId, model.Id, model.CreationDate, model.Description, model.DueDate, model.Importance));
        }

        public void MarkToDoItemAsComplete(MarkToDoItemAsCompleteModel model)
        {
            bus.Send<MarkToDoItemAsCompleteCommand>(new MarkToDoItemAsCompleteCommand(model.Id,model.ClosingDate));
        }

        public void ReOpenToDoItem(ReOpenToDoItemModel model)
        {
            bus.Send<ReOpenToDoItemCommand>(new ReOpenToDoItemCommand(model.Id));
        }

        public void ChangeImportance(ChangeToDoItemImportanceModel model)
        {
            bus.Send<ChangeToDoItemImportanceCommand>(new ChangeToDoItemImportanceCommand(model.Id, model.Importance));
        }

        public void ChangeDescription(ChangeToDoItemDescriptionModel model)
        {
            bus.Send<ChangeToDoItemDescriptionCommand>(new ChangeToDoItemDescriptionCommand(model.Id, model.Description));
        }

        public void ChangeDueDate(ChangeToDoItemDueDateModel model)
        {
            bus.Send<ChangeToDoItemDueDateCommand>(new ChangeToDoItemDueDateCommand(model.Id, model.DueDate));
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
            return await database.ToDoLists.Include(l => l.Items).FirstAsync(t => t.Id.Equals(new Guid(Id)));
        }

        public async Task<ToDoItem> GetToDoItem(string Id)
        {
            return await database.ToDoItems.FirstAsync(t => t.Id.Equals(new Guid(Id)));
        }
        #endregion
    }
}