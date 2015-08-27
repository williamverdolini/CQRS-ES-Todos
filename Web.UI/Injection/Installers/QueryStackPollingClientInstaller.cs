using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NEventStore;
using NEventStore.Client;
using Todo.Infrastructure.Events.Polling;
using Todo.QueryStack;
using Todo.QueryStack.Logic;

namespace Web.UI.Injection.Installers
{
    public class QueryStackPollingClientInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            // Lazy Dependencies
            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());
            //DI Registration for isolated-thread PollingClient
            container.Register(Component.For<ICheckpointRepository>().ImplementedBy<CheckpointRepository>());
            container.Register(Component.For<IObserver<ICommit>>().ImplementedBy<ReadModelCommitObserver>());
            container.Register(Component.For<EventObserverSubscriptionFactory>().LifeStyle.Transient);
            container.Register(Component.For<IObserveCommits>().UsingFactoryMethod(() => container.Resolve<EventObserverSubscriptionFactory>().Construct()));
            container.Register(Component.For<IPipelineHook>().ImplementedBy<LowLatencyPollingPipelineHook>().LifeStyle.Singleton);
            // Thread isolated pollingClient starter
            container.Register(Component.For<CommitObserverStarter>());
        }
    }
}