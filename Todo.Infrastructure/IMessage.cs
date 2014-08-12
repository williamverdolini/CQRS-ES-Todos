using System;

namespace Todo.Infrastructure
{
    public interface IMessage
    {
        Guid Id { get; set; }
        int Version { get; set; }
    }
}
