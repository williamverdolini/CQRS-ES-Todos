using System;
using Microsoft.AspNet.SignalR;
using Todo.Infrastructure;

namespace Todo.QueryStack.Logic.Hubs
{
    public class NotifierHub : Hub
    {
        private readonly EventNotifier eventNotifier;

        public NotifierHub(EventNotifier eventNotifier)
        {
            Contract.Requires<ArgumentNullException>(eventNotifier != null, "eventNotifier");
            this.eventNotifier = eventNotifier;
        }
    }

}