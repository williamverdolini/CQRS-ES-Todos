using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Todo.Infrastructure.Events;
using Todo.Infrastructure.Events.Rebuilding;
using Todo.QueryStack;
using Todo.QueryStack.Logic.EventHandlers;
using Todo.QueryStack.Logic.Hubs;
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
            // DI for SignalR Notifier
            container.Register(Component.For<IEventNotifier>().ImplementedBy<EventNotifier>().LifestyleSingleton()
                .DependsOn(Dependency.OnValue<IHubConnectionContext<dynamic>>(GlobalHost.ConnectionManager.GetHubContext<NotifierHub>().Clients)));

            //container.Register(Component.For<IEventNotifier>().ImplementedBy<EventNotifier>().LifestyleSingleton());
            //container.Register(Component.For<IHubConnectionContext<dynamic>>().UsingFactoryMethod(() => GlobalHost.ConnectionManager.GetHubContext<NotifierHub>().Clients).LifestyleSingleton());

            // DI Registration for Events Rebuilding
            container.Register(Component.For<IEventsRebuilder>().ImplementedBy<EventsRebuilder>().LifestyleTransient());
        }
    }
}