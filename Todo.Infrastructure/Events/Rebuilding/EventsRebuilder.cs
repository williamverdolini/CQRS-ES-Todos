using NEventStore;
using System;
using System.Linq;

namespace Todo.Infrastructure.Events.Rebuilding
{
    public class EventsRebuilder : IEventsRebuilder
    {
        private readonly IStoreEvents _store;
        private readonly IBus _bus;

        public EventsRebuilder(IStoreEvents store, IBus bus)
        {
            Contract.Requires<ArgumentNullException>(store != null, "store");
            Contract.Requires<ArgumentNullException>(bus != null, "bus");
            _store = store;
            _bus = bus;
        }

        public void Rebuild()
        {
            var commits = _store.Advanced.GetFrom(null).ToArray();
            //var count = 0;

            foreach (var commit in commits)
            {
                var evts = commit.Events
                    .Where(x => x.Body is Event)
                    .Select(evt => (dynamic)evt.Body)
                    //.Select(evt => new Message(commit.ExtractMessageId(evt), evt.Body, commit.Headers, commit.ExtractCorrelationId(evt), commit.CommitStamp))
                    .FirstOrDefault();

                //count += evts.Length;

                _bus.Publish(evts);
            }
        }
    }
}
