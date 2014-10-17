using NEventStore.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Domain.Common;

namespace Todo.Domain.Messages.Events
{
    public class ToDoEventsConverters : 
        IUpconvertEvents<AddedNewToDoItemEvent_V0, AddedNewToDoItemEvent>
        //IUpconvertEvents<AddedNewToDoItemEvent, AddedNewToDoItemEvent_V0>
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

        //public AddedNewToDoItemEvent_V0 Convert(AddedNewToDoItemEvent sourceEvent)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
