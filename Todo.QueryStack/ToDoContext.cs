using System.Data.Entity;
using Todo.QueryStack.Migrations;
using Todo.QueryStack.Model;

namespace Todo.QueryStack
{
    public class ToDoContext : DbContext
    {
        public ToDoContext() : base("name=ToDoContext") { }

        public virtual DbSet<ToDoList> Lists { get; set; }

        public virtual DbSet<ToDoItem> Items { get; set; }

        public virtual DbSet<IdentityMap> IdMap { get; set; }

        public virtual DbSet<Checkpoint> Checkpoints { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<ToDoContext, Configuration>());
        }
    }
}
