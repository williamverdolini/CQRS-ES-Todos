using NEventStore.Dispatcher;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Infrastructure.Events
{
    class EventBus : IBus, IDispatchCommits
    {
        private readonly IEventHandlerFactory _factory;
        private IList<Type> _registeredEvent;

        public EventBus(IEventHandlerFactory factory)
        {
            Contract.Requires<ArgumentNullException>(factory != null, "factory");
            _factory = factory;
            _registeredEvent = new List<Type>();
        }

        public void Dispatch(NEventStore.ICommit commit)
        {
            Contract.Requires<ArgumentNullException>(commit != null, "commit");

            // Dispatch to EventBus Implementation
            foreach (var @event in commit.Events)
            {
                Publish(@event.Body);
            }
        }

        public void Subscribe<TEvent>()
        {
            throw new NotImplementedException();
            //_registeredEvent.Add(typeof(TEvent));
        }

        public void Unsubscribe<TEvent>()
        {
            throw new NotImplementedException();
        }

        public void Publish<TEvent>(TEvent message)
        {
            throw new NotImplementedException();
        }

        void IBus.Send<TCommand>(TCommand message)
        {
            throw new NotImplementedException();
        }

        #region Dispose
        private bool disposed;
        public void Dispose()
        {
            Dispose(true);
            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //noop atm
                }
                disposed = true;
            }
        }
        #endregion
    }
}
