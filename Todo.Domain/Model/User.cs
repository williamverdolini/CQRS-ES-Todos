using CommonDomain.Core;
using System;

namespace Todo.Domain.Model
{
    public class User : AggregateBase
    {
        public string UserName { get; private set; }

        public User()
        {
            Id = Guid.NewGuid();
            //UserName = userName;

            // In more elegant code is possible to register the Aggregate's event handler
            // through 
            // for more detail: https://github.com/haf/Documently/search?q=HandlesEvent
            // https://github.com/haf/Documently/blob/master/src/Documently.Infrastructure/RegisterEventHandlersInBus.cs
            //this.Register<RegisteredEvent>(this.Apply);
        }
    }
}
