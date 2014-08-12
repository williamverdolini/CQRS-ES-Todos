using FluentValidation;

namespace Todo.Infrastructure.Commands
{
    public interface ICommandValidatorFactory
    {
        //ICommandValidator<T>[] GetValidatorsForCommand<T>(T command);
        IValidator<T>[] GetValidatorsForCommand<T>(T command);        
    }
}
