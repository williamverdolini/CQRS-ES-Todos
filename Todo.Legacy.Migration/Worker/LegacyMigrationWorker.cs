using System;
using Todo.Infrastructure;
using Todo.Legacy.Migration.Logic;

namespace Todo.Legacy.Migration.Worker
{
    public class LegacyMigrationWorker
    {
        private readonly ILegacyMigrationManager _migrator;

        public LegacyMigrationWorker(ILegacyMigrationManager migrator)
        {
            Contract.Requires<ArgumentNullException>(migrator != null, "migrator");
            _migrator = migrator;
        }

        public void ExecuteMigration()
        {
            _migrator.ExecuteMigration();
        }

    }
}
