using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using CommonDomain.Persistence.EventStore;
using NEventStore;
using NEventStore.Dispatcher;
using NEventStore.Persistence.Sql.SqlDialects;
using System;
using System.Reflection;
using Todo.Infrastructure;
using Todo.QueryStack.Logic.EventHandlers;

namespace Web.UI.Injection.Installers
{
    public class AggregateFactory : IConstructAggregates
    {
        public IAggregate Build(Type type, Guid id, IMemento snapshot)
        {
            //Here we could provide an AOP support
            //return Activator.CreateInstance(type) as IAggregate;

            ConstructorInfo constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Guid) }, null);

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    string.Format("Aggregate {0} cannot be created: constructor with only id parameter not provided",
                                  type.Name));
            }
            return constructor.Invoke(new object[] { id }) as IAggregate;
        }
    }

    public class EventStoreInstaller : IWindsorInstaller
    {
        private static IStoreEvents _store;

        private void SubscribeEvents(IBus bus) {
            bus.Subscribe<ToDoEventHandlers>();
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBus, IDispatchCommits>().ImplementedBy<InMemoryBus>().LifestyleSingleton());

            _store =
                    Wireup
                    .Init()
                    .LogToOutputWindow()
                    .UsingInMemoryPersistence()
                    .UsingSqlPersistence("EventStore") // Connection string is in web.config
                    .WithDialect(new MsSqlDialect())
                    .InitializeStorageEngine()
                    .UsingJsonSerialization()
                    .UsingSynchronousDispatchScheduler()
                    .DispatchTo(container.Resolve<IDispatchCommits>())
                    .Build();

            container.Register(
                Component.For<IStoreEvents>().Instance(_store),
                Component.For<IRepository>().ImplementedBy<EventStoreRepository>().LifeStyle.Transient,
                Component.For<IConstructAggregates>().ImplementedBy<AggregateFactory>().LifeStyle.Transient,
                Component.For<IDetectConflicts>().ImplementedBy<ConflictDetector>().LifeStyle.Transient);

            //// Elegant way to write the same Registration as before:
            //container.Register(
            //    Component.For<IStoreEvents>().Instance(_store),
            //    C<IRepository, EventStoreRepository>(),
            //    C<IConstructAggregates, AggregateFactory>(),
            //    C<IDetectConflicts, ConflictDetector>());		            
        }

        private static ComponentRegistration<TS> C<TS, TC>()
            where TC : TS
            where TS : class
        {
            return Component.For<TS>().ImplementedBy<TC>().LifeStyle.Transient;
        }

    }

}