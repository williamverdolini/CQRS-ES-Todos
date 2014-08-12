using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.QueryStack.Model;

namespace Todo.QueryStack
{
    public class ToDoContext : DbContext
    {
        public ToDoContext() : base("name=ToDoContext") { }

        public virtual DbSet<ToDoList> Lists { get; set; }

        public virtual DbSet<ToDoItem> Items { get; set; }
    }
}
