using System;
using System.Collections.Generic;
using NEventStore;
using NEventStore.Client;
using Todo.Infrastructure;
using Todo.Infrastructure.Events.Polling;

namespace Todo.QueryStack.Logic
{
    // This class is responsible to create the PollingClient and subcriver the commit-observers into it 
    // NOTE: In some scenario, there could be a single PollingClient with different ObserverCommits, 
    //       but it's important that only one observer update the checkpointStore.
    //       It should be improved with specialized IObserveCommits implementation that is responsible 
    //       for update the checkpoint store.
    public class EventObserverSubscriptionFactory
    {
        private readonly IStoreEvents eventStore;
        private readonly ICheckpointRepository checkpointRepo;
        private readonly IEnumerable<IObserver<ICommit>> commitObservers;

        public EventObserverSubscriptionFactory(IStoreEvents eventStore, ICheckpointRepository checkpointRepo, IEnumerable<IObserver<ICommit>> commitObservers)
        {
            Contract.Requires<ArgumentNullException>(eventStore != null, "eventStore");
            Contract.Requires<ArgumentNullException>(checkpointRepo != null, "checkpointRepo");
            Contract.Requires<ArgumentNullException>(commitObservers != null, "commitObservers");
            this.checkpointRepo = checkpointRepo;
            this.eventStore = eventStore;
            this.commitObservers = commitObservers;
        }

        public IObserveCommits Construct()
        {
            var pollingClient = new PollingClient(eventStore.Advanced);
            var checkpoint = checkpointRepo.LoadCheckpoint();
            IObserveCommits subscription = pollingClient.ObserveFrom(checkpoint);

            foreach (var commitObserver in commitObservers)
            {
                subscription.Subscribe(commitObserver);
            }

            return subscription;
        }


    }
}
