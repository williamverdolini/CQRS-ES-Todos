using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using CommonDomain.Persistence.EventStore;
using NEventStore;
using NEventStore.Conversion;
using NEventStore.Dispatcher;
using NEventStore.Persistence.Sql.SqlDialects;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Runtime.Serialization;
using Todo.Domain.Messages.Events;
using Todo.Infrastructure;
using Todo.Infrastructure.Events.Versioning;
using Todo.QueryStack.Logic.EventHandlers;

namespace Web.UI.Injection.Installers
{
    //NOTE: for snapshots thanks to http://www.newdavesite.com/view/18365088
    public class AggregateFactory : IConstructAggregates
    {
        public IAggregate Build(Type type, Guid id, IMemento snapshot)
        {
            Type typeParam = snapshot != null ? snapshot.GetType() : typeof(Guid);
            object[] paramArray;
            if (snapshot != null)
                paramArray = new object[] { snapshot };
            else
                paramArray = new object[] { id };

            ConstructorInfo constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeParam }, null);

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    string.Format("Aggregate {0} cannot be created: constructor with proper parameter not provided",
                                  type.Name));
            }
            return constructor.Invoke(paramArray) as IAggregate;
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
            container.Register(
                Classes
                .FromAssemblyContaining<ToDoEventsConverters>()
                .BasedOn(typeof(IUpconvertEvents<,>)) // That implement ICommandHandler Interface
                .WithService.Base()
                .LifestyleTransient());

            //#region Work-Around for Event-Upconverion with SynchronousDispatchScheduler
            //IDictionary<Type, Func<object, object>> _registered = new Dictionary<Type, Func<object, object>>();
            //var converter = new ToDoEventsConverters();
            //_registered[typeof (AddedNewToDoItemEvent_V0)] = @event => converter.Convert(@event as AddedNewToDoItemEvent_V0);
            //// Workaround for Events Up-conversion. InMemoryBus is injected with EventUpconverterPipelineHook instance.
            ////container.Register(Component.For<EventUpconverterPipelineHook>().Instance(new EventUpconverterPipelineHook(_registered)));
            //#endregion

            _store =
                    Wireup
                    .Init()
                    .LogToOutputWindow()
                    .UsingInMemoryPersistence()
                    .UsingSqlPersistence("EventStore") // Connection string is in web.config
                        .WithDialect(new MsSqlDialect())
                            //.UsingJsonSerialization()
                            .UsingNewtonsoftJsonSerialization(new VersionedEventSerializationBinder())
                            //.Compress()
                    .UsingSynchronousDispatchScheduler()
                        .DispatchTo(container.Resolve<IDispatchCommits>())
                        .Startup(DispatcherSchedulerStartup.Explicit)
                    //// DOES NOT WORK WITH SynchronousDispatchScheduler
                    .UsingEventUpconversion()
                        .WithConvertersFromAssemblyContaining(new Type[] { typeof(ToDoEventsConverters) })
                    //    .AddConverter<AddedNewToDoItemEvent_V0, AddedNewToDoItemEvent>(new ToDoEventsConverters())
                    //.HookIntoPipelineUsing(new EventUpconverterPipelineHook(_registered))
                    //.WithConvertersFromAssemblyContaining(new Type[]{typeof(ToDoEventsConverters)})
                    .Build();

            _store.StartDispatchScheduler();

            //wireup.AddConverter(new ToDoEventsConverters());
            //_store = wireup.Build();

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

    public static class WireupExtensions
    {
        public static SerializationWireup UsingNewtonsoftJsonSerialization(this PersistenceWireup wireup, SerializationBinder binder, params Type[] knownTypes)
        {
            return wireup.UsingCustomSerialization(
                new NewtonsoftJsonSerializer(binder, new JsonConverter[] { }, knownTypes)
                    );
        }
    }
}