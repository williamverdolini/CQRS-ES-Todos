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
//using Todo.QueryStack.Logic.EventHandlers;
//using Todo.QueryStack.Logic;
using System.Collections.Generic;
using Todo.Infrastructure.Events;
using NEventStore.Client;
//using Todo.Infrastructure.Events.Polling;

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
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBus, IDispatchCommits>().ImplementedBy<InMemoryBus>().LifeStyle.Singleton);
            container.Register(
                Classes
                .FromAssemblyContaining<ToDoEventsConverters>()
                .BasedOn(typeof(IUpconvertEvents<,>)) // That implement ICommandHandler Interface
                .WithService.Base()
                .LifestyleTransient());

            // NeventStore with async PollingClient
            container.Register(Component.For<NEventStoreWithCustomPipelineFactory>());
            // NeventStore with sync dispatcher (OLD-STYLE reviewed)
            //container.Register(Component.For<NEventStoreWithSyncDispatcherFactory>());
            container.Register(Component.For<IStoreEvents>().UsingFactoryMethod(() => container.Resolve<NEventStoreWithCustomPipelineFactory>().Create()).LifeStyle.Singleton);
            container.Register(Component.For<IRepository>().ImplementedBy<EventStoreRepository>().LifeStyle.Transient);
            container.Register(Component.For<IConstructAggregates>().ImplementedBy<AggregateFactory>().LifeStyle.Transient);
            container.Register(Component.For<IDetectConflicts>().ImplementedBy<ConflictDetector>().LifeStyle.Transient);

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

        public static Wireup HookIntoReadModePipeline(this Wireup wireup, SerializationBinder binder)
        {
            return wireup;
        }

    }

    #region NEventStore Factories
    public class NEventStoreWithCustomPipelineFactory
    {
        private readonly IEnumerable<IPipelineHook> pipelineHooks;

        public NEventStoreWithCustomPipelineFactory(IEnumerable<IPipelineHook> pipelineHooks)
        {
            this.pipelineHooks = pipelineHooks;
        }
        public IStoreEvents Create()
        {
            return Wireup
                    
                .Init()
                    .LogToOutputWindow()
                    .UsingSqlPersistence("EventStore") // Connection string is in web.config
                        .WithDialect(new MsSqlDialect())
                            .InitializeStorageEngine()
                            .UsingNewtonsoftJsonSerialization(new VersionedEventSerializationBinder())
                // Compress Aggregate serialization. Does NOT allow to do a SQL-uncoding of varbinary Payload
                // Comment if you need to decode message with CAST([Payload] AS VARCHAR(MAX)) AS [Payload] (on some VIEW)
                            //.Compress()
                    .UsingEventUpconversion()
                        .WithConvertersFromAssemblyContaining(new Type[] { typeof(ToDoEventsConverters) })
                    .HookIntoPipelineUsing(pipelineHooks)
                    .Build();
        }
    }

    //Now you can remove this class to avoid NEventStore warning CS0618.
    public class NEventStoreWithSyncDispatcherFactory
    {
        private readonly IDispatchCommits syncDispatcher;

        public NEventStoreWithSyncDispatcherFactory(IDispatchCommits syncDispatcher)
        {
            this.syncDispatcher = syncDispatcher;
        }

        public IStoreEvents Create()
        {
            var store = Wireup
                    .Init()
                    .LogToOutputWindow()
                    .UsingInMemoryPersistence()
                    .UsingSqlPersistence("EventStore") // Connection string is in web.config
                        .WithDialect(new MsSqlDialect())
                        //.UsingJsonSerialization()
                            .UsingNewtonsoftJsonSerialization(new VersionedEventSerializationBinder())
                        // Compress Aggregate serialization. Does NOT allow to do a SQL-uncoding of varbinary Payload
                        // Comment if you need to decode message with CAST([Payload] AS VARCHAR(MAX)) AS [Payload] (on some VIEW)
                        //.Compress()
                    .UsingSynchronousDispatchScheduler()
                        .DispatchTo(syncDispatcher)
                        .Startup(DispatcherSchedulerStartup.Explicit)
                    .UsingEventUpconversion()
                        .WithConvertersFromAssemblyContaining(new Type[] { typeof(ToDoEventsConverters) })
                    .Build();

            store.StartDispatchScheduler();
            return store;
        }
    }
    #endregion

}