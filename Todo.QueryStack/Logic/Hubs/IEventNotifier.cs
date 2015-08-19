using System;
using System.Threading.Tasks;
using Todo.QueryStack.Model;

namespace Todo.QueryStack.Logic.Hubs
{
    public interface IEventNotifier
    {
        Task CreatedToDoListEventNotify(ToDoList list);
        Task ChangedToDoListDescriptionEventNotify(ToDoList list);
        Task AddedNewToDoItemEventNotify(ToDoItem item);
        Task MarkedToDoItemAsCompletedEventNotify(ToDoItem item);
        Task ReOpenedToDoItemEventNotify(ToDoItem item);
        Task ChangedToDoItemImportanceEventNotify(ToDoItem item);
        Task ChangedToDoItemDescriptionEventNotify(ToDoItem item);
        Task ChangedToDoItemDueDateEventNotify(ToDoItem item);
    }
}
