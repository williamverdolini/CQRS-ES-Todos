using FluentValidation;
using NEventStore.Dispatcher;
using System;
using System.Collections.Generic;
using System.Reflection;
using Todo.Infrastructure.Commands;
using Todo.Infrastructure.Events;

namespace Todo.Infrastructure
{
    public class InMemoryBus : IBus, IDispatchCommits
    {
        private readonly ICommandHandlerFactory _commandHandlerfactory;
        private readonly IEventHandlerFactory _eventHandlerfactory;
        private readonly ICommandValidatorFactory _commandValidatorFactory;
        private IList<Type> _registeredEventHandlers;

        public InMemoryBus(ICommandHandlerFactory commandHandlerFactory, IEventHandlerFactory eventHandlerFactory, ICommandValidatorFactory commandValidatorFactory)
        {
            Contract.Requires<ArgumentNullException>(commandHandlerFactory != null, "commandHandlerFactory");
            Contract.Requires<ArgumentNullException>(eventHandlerFactory != null, "eventHandlerFactory");
            Contract.Requires<ArgumentNullException>(commandValidatorFactory != null, "commandValidatorFactory");
            _commandHandlerfactory = commandHandlerFactory;
            _eventHandlerfactory = eventHandlerFactory;
            _commandValidatorFactory = commandValidatorFactory;
            _registeredEventHandlers = new List<Type>();
        }

        #region Command Bus
        void IBus.Send<TCommand>(TCommand message)
        {
            #region Command Validations
            // Eventual consistency checks
            // ATTENTION: based on domain requisites could be possibile to add constraint to readmodel DB
            //          or to check before persisting. In this case, it's quite rare to have concurrent conflicts
            //          so, this "eventual check" is perfect in most of the domain cases and could be assumed as good default rule
            IValidator<TCommand>[] validators = _commandValidatorFactory.GetValidatorsForCommand<TCommand>(message);
            foreach (var validator in validators)
            {
                validator.ValidateAndThrow(message);
            }
            #endregion

            #region Command Handling
            ICommandHandler<TCommand>[] handlers = _commandHandlerfactory.GetHandlersForCommand<TCommand>(message);
            foreach (var handler in handlers)
            {
                handler.Handle(message);
            }
            #endregion

            #region Aggregates Snapshooting
            #if DEBUG
            foreach (var handler in handlers)
            {
                // If commandHandler is a SnapshotCreator it handles the snapshoot persistance.
                // NOTE: Very unrealistic Snapsooting policy/implementation, but it's just for training purposes
                MethodInfo method = handler.GetType().GetMethod("SaveSnapShot");
                if(method != null)
                    method.Invoke(handler, new object[] { message });
            }
            #endif
            #endregion
        }
        #endregion

        #region Event Bus
        public void Subscribe<TEvent>()
        {
            throw new NotImplementedException();
            //_registeredEventHandlers.Add(typeof(TEvent));
        }

        public void Unsubscribe<TEvent>()
        {
            throw new NotImplementedException();
            //_registeredEventHandlers.Remove(typeof(TEvent));
        }

        public void Publish<TEvent>(TEvent message)
        {
            IEventHandler<TEvent>[] handlers = _eventHandlerfactory.GetHandlersForEvent<TEvent>(message);
            foreach (var handler in handlers)
            {
                handler.Handle(message);
            }
        }
        #endregion

        #region NEventStore Event Dispatcher
        public void Dispatch(NEventStore.ICommit commit)
        {
            Contract.Requires<ArgumentNullException>(commit != null, "commit");

            // Dispatch to Event Bus Implementation
            foreach (var @event in commit.Events)
            {
                //Run-time conversion for typed event
                Publish((dynamic)@event.Body);
            }
        }
        #endregion

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
