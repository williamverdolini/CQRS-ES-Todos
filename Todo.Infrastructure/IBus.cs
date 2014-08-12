
namespace Todo.Infrastructure
{
    public interface IBus
    {
        // Commands
        void Send<TCommand>(TCommand message);

        //Events
        void Subscribe<TEvent>();

        void Unsubscribe<TEvent>();

        void Publish<TEvent>(TEvent message);
    }
}
