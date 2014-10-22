using NEventStore.Conversion;
using Todo.Domain.Common;

namespace Todo.Domain.Messages.Events
{
    public class ToDoEventsConverters : IUpconvertEvents<AddedNewToDoItemEvent_V0, AddedNewToDoItemEvent>
    {
        public AddedNewToDoItemEvent Convert(AddedNewToDoItemEvent_V0 sourceEvent)
        {
            var targetEvent = new AddedNewToDoItemEvent(
                sourceEvent.ToDoListId,
                sourceEvent.ToDoItemId,
                sourceEvent.CreationDate,
                sourceEvent.Description,
                sourceEvent.DueDate,
                sourceEvent.Importance,
                Constants.DEFAULT_USER);
            return targetEvent;
        }
    }

    public class MarkedToDoItemAsCompletedEventConverter : IUpconvertEvents<MarkedToDoItemAsCompletedEvent_V0, MarkedToDoItemAsCompletedEvent>
    {
        public MarkedToDoItemAsCompletedEvent Convert(MarkedToDoItemAsCompletedEvent_V0 sourceEvent)
        {
            var targetEvent = new MarkedToDoItemAsCompletedEvent(
                    sourceEvent.Id,
                    sourceEvent.ClosingDate,
                    "fake-user"
                );
            return targetEvent;
        }
    }
}
