using Todo.QueryStack.Model;

namespace Todo.QueryStack.Mappers
{
    public interface INotifierMapper
    {
        NotifiedToDoList Map(ToDoList source);
        NotifiedToDoItem Map(ToDoItem source);
    }

    public sealed class NotifierMapper : INotifierMapper
    {
        public NotifiedToDoList Map(ToDoList source)
        {
            if (source == null)
            {
                return null;
            }

            return new NotifiedToDoList
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description
            };
        }

        public NotifiedToDoItem Map(ToDoItem source)
        {
            if (source == null)
            {
                return null;
            }

            return new NotifiedToDoItem
            {
                Id = source.Id,
                ToDoListId = source.ToDoListId,
                Description = source.Description,
                CreationDate = source.CreationDate,
                DueDate = source.DueDate,
                Importance = source.Importance,
                ClosingDate = source.ClosingDate,
                UserId = source.UserId
            };
        }
    }
}