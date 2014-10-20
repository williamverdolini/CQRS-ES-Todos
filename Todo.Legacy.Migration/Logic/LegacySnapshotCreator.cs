using CommonDomain.Persistence;
using NEventStore;
using System;
using System.Collections.Generic;
using System.Transactions;
using Todo.Domain.Messages.Events;
using Todo.Infrastructure;
using Todo.QueryStack;
using Todo.QueryStack.Model;


namespace Todo.Legacy.Migration.Logic
{

    internal class MigratedToDoListEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class LegacySnapshotCreator : ILegacyMigrationManager
    {
        private readonly IStoreEvents _store;
        private readonly IDatabase _db;

        private const string AggregateTypeHeader = "AggregateType";

        public LegacySnapshotCreator(IStoreEvents store, IDatabase database)
        {
            Contract.Requires<ArgumentNullException>(store != null, "store");
            Contract.Requires<ArgumentNullException>(database != null, "database");
            _store = store;
            _db = database;
        }

        /// <summary>
        /// Migration Logic for ToDoList and ToDoItem.
        /// NOTE: uses Snapshot and Memento implementation to create initial version of AggregateRoots
        /// </summary>
        /// <param name="command"></param>
        public void ExecuteMigration()
        {
            #region ToDoList Migration
            // For each list in legacy database, creates a new repository ID, 
            // mapping and snapshot with the data retrieved from readModel (legacy database)
            foreach (var list in _db.ToDoLists)
            {
                if (_db.IdMaps.GetAggregateId<ToDoList>(list.Id).Equals(Guid.Empty))
                {
                    using (var scope = new TransactionScope())
                    {
                        // Get fresh new ID
                        Guid entityId = Guid.NewGuid();
                        while (!_db.IdMaps.GetModelId<ToDoList>(entityId).Equals(0))
                            entityId = Guid.NewGuid();

                        // Save Ids mapping
                        _db.IdMaps.Map<ToDoList>(list.Id, entityId);

                        // Create Memento from ReadModel
                        var entity = new Todo.Domain.Model.ToDoListMemento(entityId, 1, list.Title, list.Description);

                        //At this point the entity migration is complete, but an event is needed in order
                        //to replay correctly all events (for brand new projections, upgrade read-model, etc)
                        //Create a fake External event: Memento Propagation Events
                        using (var stream = _store.OpenStream(entityId, 0, int.MaxValue))
                        {
                            //// Header of stream (like NEeventStore does). See: https://github.com/NEventStore/NEventStore/blob/master/src/NEventStore/CommonDomain/Persistence/EventStore/EventStoreRepository.cs#L197
                            //var headers = new Dictionary<string, object>();
                            //headers[AggregateTypeHeader] = entity.GetType().FullName.Replace("Memento", "");
                            // Memento Propagation Event
                            var propagationEvent = new ToDoListMementoPropagatedEvent(entity);
                            // Header of stream (like NEeventStore does). See: https://github.com/NEventStore/NEventStore/blob/master/src/NEventStore/CommonDomain/Persistence/EventStore/EventStoreRepository.cs#L197
                            
                            stream.UncommittedHeaders[AggregateTypeHeader] = entity.GetType().FullName.Replace("Memento", "");
                            stream.Add(new EventMessage { Body = propagationEvent });
                            stream.CommitChanges(Guid.NewGuid());
                        }

                        // Save Snapshot from entity's Memento image
                        _store.Advanced.AddSnapshot(new Snapshot(entity.Id.ToString(), entity.Version, entity));

                        scope.Complete();
                        System.Diagnostics.Debug.WriteLine("Successfully migrated ToDoList id: {0}, Guid: {1}, Title:{2}", new object[] { list.Id, entityId.ToString(), list.Title });
                    }
                }
            }
            #endregion

            #region ToDoItem Migration
            foreach (var item in _db.ToDoItems)
            {
                if (_db.IdMaps.GetAggregateId<ToDoItem>(item.Id).Equals(Guid.Empty))
                {
                    using (var scope = new TransactionScope())
                    {
                        // Get fresh new ID
                        Guid entityId = Guid.NewGuid();
                        while (!_db.IdMaps.GetModelId<ToDoItem>(entityId).Equals(0))
                            entityId = Guid.NewGuid();

                        // Save Ids mapping
                        _db.IdMaps.Map<ToDoItem>(item.Id, entityId);

                        // Create Memento from ReadModel
                        Guid todoListId = _db.IdMaps.GetAggregateId<ToDoList>(item.ToDoListId);
                        var entity = new Todo.Domain.Model.ToDoItemMemento(entityId, 1, todoListId, item.Description, item.CreationDate, item.DueDate, item.Importance, item.ClosingDate, item.UserId);

                        // At this point the entity migration is complete, but an event is needed in order
                        // to replay correctly all events (for brand new projections, upgrade read-model, etc)
                        // Create a fake External event: Memento Propagation Events
                        using (var stream = _store.OpenStream(entityId, 0, int.MaxValue))
                        {
                            //// Header of stream (like NEeventStore does). See: https://github.com/NEventStore/NEventStore/blob/master/src/NEventStore/CommonDomain/Persistence/EventStore/EventStoreRepository.cs#L197
                            //var headers = new Dictionary<string, object>();
                            //headers[AggregateTypeHeader] = entity.GetType().FullName.Replace("Memento", "");
                            // Memento Propagation Event
                            var propagationEvent = new ToDoItemMementoPropagatedEvent(entity);
                            // Header of stream (like NEeventStore does). See: https://github.com/NEventStore/NEventStore/blob/master/src/NEventStore/CommonDomain/Persistence/EventStore/EventStoreRepository.cs#L197
                            
                            stream.UncommittedHeaders[AggregateTypeHeader] = entity.GetType().FullName.Replace("Memento", "");
                            stream.Add(new EventMessage { Body = propagationEvent });
                            stream.CommitChanges(Guid.NewGuid());
                        }

                        // Save Snapshot from entity's Memento image
                        _store.Advanced.AddSnapshot(new Snapshot(entity.Id.ToString(), entity.Version, entity));

                        scope.Complete();
                        System.Diagnostics.Debug.WriteLine("Successfully migrated ToDoItem id: {0}, Guid: {1}, Description:{2}", new object[] { item.Id, entityId.ToString(), item.Description });
                    }

                }
            }
            #endregion
        }


    }
}
