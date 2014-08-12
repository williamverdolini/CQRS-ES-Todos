using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FluentValidation;
using Todo.CommandStack.Logic.CommandHandlers;
using Todo.CommandStack.Logic.Validators;
using Todo.Infrastructure.Commands;
using Todo.Infrastructure.Events;
using Todo.QueryStack;
using Todo.QueryStack.Logic.EventHandlers;

namespace Web.UI.Injection.Installers
{
    public class MessagesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes
                .FromAssemblyContaining<ToDoCommandHandlers>()
                .BasedOn(typeof(ICommandHandler<>)) // That implement ICommandHandler Interface
                .WithService.Base()    // and its name contain "CommandHandler"
                .LifestyleSingleton()
                );

            container.Register(
                Classes
                .FromAssemblyContaining<CreateToDoListCommandValidator>()
                .BasedOn(typeof(IValidator<>)) // That implement IValidator Interface
                .WithService.Base()    // and its name contain "Validator"
                .LifestyleSingleton()
                );

            container.Register(
                Classes
                .FromAssemblyContaining<ToDoEventHandlers>()
                .BasedOn(typeof(IEventHandler<>)) // That implement ICommandHandler Interface
                .WithService.Base()    // and its name contain "CommandHandler"
                .LifestyleSingleton()
                );

            // DI Registration for Typed Factory for Command and Event Handlers
            container.AddFacility<TypedFactoryFacility>()
                .Register(Component.For<ICommandHandlerFactory>().AsFactory())
                .Register(Component.For<ICommandValidatorFactory>().AsFactory())
                .Register(Component.For<IEventHandlerFactory>().AsFactory());

            // DI Registration for IDatabase (QueryStack)
            container.Register(Component.For<IDatabase>().ImplementedBy<Database>().LifestyleTransient());
        }
    }


}