
namespace Todo.Infrastructure.Events
{
    public interface IEventHandlerFactory
    {
        IEventHandler<T>[] GetHandlersForEvent<T>(T @event);
    }
}
