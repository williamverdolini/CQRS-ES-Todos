using CommonDomain.Core;
using CommonDomain.Persistence;
using NEventStore;
using System;
using Todo.Infrastructure.Domain;

namespace Todo.Infrastructure.Commands
{
    public abstract class SnapshotCreator<T> where T : AggregateBase
    {
        private readonly IRepository _repo;
        private readonly IStoreEvents _store;

        public SnapshotCreator(IRepository repo, IStoreEvents store)
        {
            Contract.Requires<ArgumentNullException>(repo != null, "repo");
            Contract.Requires<ArgumentNullException>(store != null, "repo");
            _repo = repo;
            _store = store;
        }

        /// <summary>
        /// Save new Aggregate Snapshot depending on specific Snapshooting policies.
        /// NOTE: In real context, it should be an external thread to save snapshots, without interfere with online process
        /// </summary>
        /// <param name="command"></param>
        public void SaveSnapShot(Command command)
        {
            T list = _repo.GetById<T>(command.Id);
            // Create a Snapshot every 1000 version of the Aggregate
            // NOTE: very nasty logic/implementation, but just for training purposes
            if (list.Version % 1000 == 0)
                _store.Advanced.AddSnapshot(new Snapshot(list.Id.ToString(), list.Version, ((IMementoCreator)list).CreateMemento()));
        }
    }
}
