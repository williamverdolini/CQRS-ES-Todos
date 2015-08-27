using System.Collections.Generic;
using NEventStore.Client;

namespace Todo.Infrastructure.Events.Polling
{    
    public class CommitObserverStarter
    {
        private readonly IEnumerable<IObserveCommits> commitObservers;

        public CommitObserverStarter(IEnumerable<IObserveCommits> commitObservers)
        {
            this.commitObservers = commitObservers;
        }

        public void Start()
        {
            foreach (var observer in commitObservers)
            {
                observer.Start();
            }
        }
    }
}
