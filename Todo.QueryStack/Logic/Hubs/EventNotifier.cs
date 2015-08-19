using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.SignalR.Hubs;
using Todo.Infrastructure;
using Todo.QueryStack.Model;

namespace Todo.QueryStack.Logic.Hubs
{
    public class EventNotifier : IEventNotifier
    {
        private readonly IHubConnectionContext<dynamic> clients;
        private readonly IMappingEngine mapper;

        public EventNotifier(IHubConnectionContext<dynamic> clients, IMappingEngine mapper)
        {
            Contract.Requires<ArgumentNullException>(clients != null, "clients");
            Contract.Requires<ArgumentNullException>(mapper != null, "mapper");
            this.clients = clients;
            this.mapper = mapper;
        }

        public async Task ChangedToDoListDescriptionEventNotify(ToDoList list)
        {
            await clients.All.changedToDoListDescription(mapper.Map<NotifiedToDoList>(list));
        }

        public async Task CreatedToDoListEventNotify(ToDoList list)
        {
            await clients.All.createdToDoListEvent(mapper.Map<NotifiedToDoList>(list));
        }

        public async Task AddedNewToDoItemEventNotify(ToDoItem item)
        {
            await clients.All.addedNewToDoItemEvent(mapper.Map<NotifiedToDoItem>(item));
        }

        public async Task MarkedToDoItemAsCompletedEventNotify(ToDoItem item)
        {
            await clients.All.markedToDoItemAsCompletedEvent(mapper.Map<NotifiedToDoItem>(item));
        }

        public async Task ReOpenedToDoItemEventNotify(ToDoItem item)
        {
            await clients.All.reOpenedToDoItemEvent(mapper.Map<NotifiedToDoItem>(item));
        }

        public async Task ChangedToDoItemImportanceEventNotify(ToDoItem item)
        {
            await clients.All.changedToDoItemImportanceEvent(mapper.Map<NotifiedToDoItem>(item));
        }

        public async Task ChangedToDoItemDescriptionEventNotify(ToDoItem item)
        {
            await clients.All.changedToDoItemDescriptionEvent(mapper.Map<NotifiedToDoItem>(item));
        }

        public async Task ChangedToDoItemDueDateEventNotify(ToDoItem item)
        {
            await clients.All.changedToDoItemDueDateEvent(mapper.Map<NotifiedToDoItem>(item));
        }
    }
}
