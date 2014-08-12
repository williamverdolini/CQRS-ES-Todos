
namespace Todo.Infrastructure.Events
{
    public interface IEventHandler<T>
    {
        void Handle(T @event);
    }
}
