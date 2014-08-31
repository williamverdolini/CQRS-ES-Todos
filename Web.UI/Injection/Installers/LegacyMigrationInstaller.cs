using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Todo.Legacy.Migration.Infrastructure;
using Todo.Legacy.Migration.Logic;
using Todo.Legacy.Migration.Worker;
using Todo.QueryStack;

namespace Web.UI.Injection.Installers
{
    public class LegacyMigrationInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // DI Registration for IDatabase (Migration)
            container.Register(Component.For<IDatabase>().ImplementedBy<LegacyDatabase>().LifestyleTransient());
            // DI Registration for Migration Manager, with dependy on specific IDatabase implementation for migration
            container.Register(Component.For<ILegacyMigrationManager>().ImplementedBy<LegacySnapshotCreator>().LifestyleTransient()
                .DependsOn(Dependency.OnComponent<IDatabase, LegacyDatabase>()));            
            // Register Worker Services
            container.Register(Component.For<LegacyMigrationWorker>().LifeStyle.Transient);

        }
    }
}