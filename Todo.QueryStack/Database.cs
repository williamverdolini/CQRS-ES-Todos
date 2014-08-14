using System.Linq;
using Todo.QueryStack.Model;

namespace Todo.QueryStack
{
    public class Database : IDatabase
    {
        private ToDoContext Context;
        public Database()
        {
            Context = new ToDoContext();
            //Context.Configuration.AutoDetectChangesEnabled = false;
            // Lazy loading is turned off
            Context.Configuration.LazyLoadingEnabled = false;
            Context.Database.Log = s => { System.Diagnostics.Debug.WriteLine(s); }; ; 

        }

        public IQueryable<ToDoList> ToDoLists
        {
            get { return Context.Lists; }
        }

        public IQueryable<ToDoItem> ToDoItems
        {
            get { return Context.Items; }
        }

    }
}
