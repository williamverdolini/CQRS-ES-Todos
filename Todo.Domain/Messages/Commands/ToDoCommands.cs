using System;
using Todo.Infrastructure.Commands;

namespace Todo.Domain.Messages.Commands
{
    // ToDoList Commands
    public class CreateToDoListCommand : Command
    {
        public string Title { get; private set; }
        public string Description { get; private set; }

        // GUID generated externally
        public CreateToDoListCommand(string id, string title, string description)
        {
            Id = new Guid(id);
            Title = title;
            Description = description;
        }
    }

    public class ChangeToDoListDescriptionCommand : Command
    {
        public string Description { get; private set; }

        // GUID generated externally
        public ChangeToDoListDescriptionCommand(string id, string description)
        {
            Id = new Guid(id);
            Description = description;
        }
    }

    // ToDoItem Commands
    public class AddNewToDoItemCommand : Command
    {
        public Guid TodoListId { get; private set; }
        public DateTime CreationDate { get; private set; }
        public string Description { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int Importance { get; private set; }

        public AddNewToDoItemCommand(string todoListId, string id, DateTime creationDate, string description, DateTime? dueDate, int importance)
        {
            TodoListId = new Guid(todoListId);
            Id = new Guid(id);
            CreationDate = creationDate;
            Description = description;
            DueDate = dueDate;
            Importance = importance;
        }
    }

    public class MarkToDoItemAsCompleteCommand : Command
    {
        public DateTime ClosingDate { get; private set; }

        public MarkToDoItemAsCompleteCommand(string id, DateTime closingDate)
        {
            Id = new Guid(id);
            ClosingDate = closingDate;
        }
    }

    public class ReOpenToDoItemCommand : Command
    {
        public ReOpenToDoItemCommand(string id)
        {
            Id = new Guid(id);
        }
    }

    public class ChangeToDoItemImportanceCommand : Command
    {
        public int Importance { get; set; }

        public ChangeToDoItemImportanceCommand(string id, int importance)
        {
            Id = new Guid(id);
            Importance = importance;
        }
    }

    public class ChangeToDoItemDescriptionCommand : Command
    {
        public string Description{ get; set; }

        public ChangeToDoItemDescriptionCommand(string id, string description)
        {
            Id = new Guid(id);
            Description = description;
        }
    }

    public class ChangeToDoItemDueDateCommand : Command
    {
        public DateTime? DueDate { get; set; }

        public ChangeToDoItemDueDateCommand(string id, DateTime? dueDate)
        {
            Id = new Guid(id);
            DueDate = dueDate;
        }
    }
}
