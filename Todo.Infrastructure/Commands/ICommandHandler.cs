
namespace Todo.Infrastructure.Commands
{
    public interface ICommandHandler<T>
    {
        void Handle(T command);
    }
}
