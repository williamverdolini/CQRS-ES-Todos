using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Todo.Infrastructure.Events;
using Todo.QueryStack;
using Todo.QueryStack.Logic.EventHandlers;
using Todo.QueryStack.Logic.Services;

namespace Web.UI.Injection.Installers
{
    public class QueryStackInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // QueryStack Event Handlers Registration
            container.Register(
                Classes
                .FromAssemblyContaining<ToDoEventHandlers>()
                .BasedOn(typeof(IEventHandler<>)) // That implement ICommandHandler Interface
                .WithService.Base()    // and its name contain "CommandHandler"
                .LifestyleSingleton()
                );

            // DI Registration for IDatabase (QueryStack)
            container.Register(Component.For<IDatabase>().ImplementedBy<Database>().LifestyleTransient());
            container.Register(Component.For<IIdentityMapper>().ImplementedBy<IdentityMapper>().LifestyleTransient());
        }
    }
}