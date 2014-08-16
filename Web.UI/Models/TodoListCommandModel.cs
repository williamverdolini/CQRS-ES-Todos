using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.UI.Models.TodoList
{
    public class CreateTodoListCommandModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class ChangeTodoListDescriptionCommandModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    public class AddNewToDoItemCommandModel
    {
        public string ToDoListId { get; set; }
        public string Id { get; set; }
        public string JsonDueDate { get; set; }
        public DateTime? DueDate { get { return JsonDueDate != null ? DateTime.Parse(JsonDueDate) : new Nullable<DateTime>(); } }
        public int Importance { get; set; }
        public string Description { get; set; }
        public string JsonCreationDate { get; set; }
        public DateTime CreationDate { get { return DateTime.Parse(JsonCreationDate); } }
    }

    public class MarkToDoItemAsCompleteModel
    {
        public string Id { get; set; }
        public string ToDoListId { get; set; }
        public string JsonClosingDate { get; set; }
        public DateTime ClosingDate { get { return DateTime.Parse(JsonClosingDate); } }
    }

    public class ReOpenToDoItemModel
    {
        public string Id { get; set; }
        public string ToDoListId { get; set; }
    }

    public class ChangeToDoItemImportanceModel
    {
        public string Id { get; set; }
        public string ToDoListId { get; set; }
        public int Importance { get; set; }
    }

    public class ChangeToDoItemDescriptionModel
    {
        public string Id { get; set; }
        public string ToDoListId { get; set; }
        public string Description { get; set; }
    }

    public class ChangeToDoItemDueDateModel
    {
        public string Id { get; set; }
        public string ToDoListId { get; set; }
        public DateTime? DueDate { get; set; }        
    }

}