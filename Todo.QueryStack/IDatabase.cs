using System.Linq;
using Todo.QueryStack.Logic.Services;
using Todo.QueryStack.Model;

namespace Todo.QueryStack
{
    public interface IDatabase
    {
        IQueryable<ToDoList> ToDoLists { get; }

        IQueryable<ToDoItem> ToDoItems { get; }

        IIdentityMapper IdMaps { get; }
    }
}
