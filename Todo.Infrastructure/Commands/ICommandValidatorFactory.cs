using FluentValidation;

namespace Todo.Infrastructure.Commands
{
    public interface ICommandValidatorFactory
    {
        IValidator<T>[] GetValidatorsForCommand<T>(T command);        
    }
}
