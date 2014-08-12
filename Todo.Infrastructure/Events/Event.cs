using System;

namespace Todo.Infrastructure.Events
{
    [Serializable]
    public abstract class Event : IMessage
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}
