using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Todo.QueryStack.Model
{
    [Table("dbo.ToDoItem")]
    public class ToDoItem
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ToDoListId { get; set; }
        //[ForeignKey("ToDoListId")]
        //virtual public ToDoList List {get; set;}
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int Importance { get; set; }
        public DateTime? ClosingDate { get; set; }
        public int UserId { get; set; }
    }
}
