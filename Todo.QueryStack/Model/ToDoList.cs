using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Todo.QueryStack.Model
{
    [Table("dbo.ToDoList")]
    public class ToDoList
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        // Lazy loading of Items
        public virtual ICollection<ToDoItem> Items { get; private set; }

        //public ToDoList()
        //{
            //Items = new List<ToDoItem>();
        //}
    }

    public class NotifiedToDoList
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ICollection<ToDoItem> Items { get; private set; }
    }

}
