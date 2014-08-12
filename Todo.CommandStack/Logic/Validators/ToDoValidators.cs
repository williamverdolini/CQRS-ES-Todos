using CommonDomain.Persistence;
using FluentValidation;
using FluentValidation.Resources;
using System;
using System.Linq;
using Todo.Domain.Messages.Commands;
using Todo.Domain.Model;
using Todo.Infrastructure;
using Todo.QueryStack;

namespace Todo.CommandStack.Logic.Validators
{
    public class CreateToDoListCommandValidator : AbstractValidator<CreateToDoListCommand>
    {
        private readonly IDatabase database;

        public CreateToDoListCommandValidator(IDatabase db)
        {
            Contract.Requires<ArgumentNullException>(db != null, "db");
            database = db;

            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.Title).NotEmpty();
            RuleFor(command => command.Title).Must(BeUniqueTitle).WithMessage("List's Title is already used. Please choose another.");
        }

        private bool BeUniqueTitle(string title)
        {
            return !database.ToDoLists.Any(t => t.Title.Equals(title));
        }
    }

    public class AddNewToDoItemCommandValidator : AbstractValidator<AddNewToDoItemCommand>
    {
        public AddNewToDoItemCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.Description).NotEmpty();
            // If DueDate is not null, it should be >= CreationDate
            RuleFor(command => command.DueDate).GreaterThanOrEqualTo(command => command.CreationDate).When(command => command.DueDate != null);
            RuleFor(command => command.Importance).GreaterThanOrEqualTo(0);
        }
    }

    public class MarkToDoItemAsCompletedCommandValidator :AbstractValidator<MarkToDoItemAsCompleteCommand>
    {
        private readonly IRepository repository;

        public MarkToDoItemAsCompletedCommandValidator(IRepository repo)
        {
            Contract.Requires<ArgumentNullException>(repo != null, "repo");
            repository = repo;

            RuleFor(command => command.ClosingDate).Must(GreaterThanOrEqualToCreation)
                .WithMessage(Messages.greaterthanorequal_error, "ClosingDate", "CreationDate");

            //// Alternative way to implement same custom validation rule (more flexible, but for most of the cases "Must" is ok)
            //Custom(command =>
            //{
            //    ToDoItem item = repository.GetById<ToDoItem>(command.Id);
            //    return command.ClosingDate < item.CreationDate ?
            //        new ValidationFailure("ClosingDate","'ClosingDate' deve essere minore della data di creazione") :
            //        null;
            //});
        }

        private bool GreaterThanOrEqualToCreation(MarkToDoItemAsCompleteCommand command, DateTime closingDate)
        {
            ToDoItem item = repository.GetById<ToDoItem>(command.Id);
            return closingDate >= item.CreationDate;
        }
    }

    public class ReOpenToDoItemCommandValidator : AbstractValidator<ReOpenToDoItemCommand>
    {
        public ReOpenToDoItemCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
        }
    }

    public class ChangeToDoItemImportanceCommandValidator : AbstractValidator<ChangeToDoItemImportanceCommand>
    {
        public ChangeToDoItemImportanceCommandValidator()
        {
            RuleFor(command => command.Importance).NotEmpty().GreaterThanOrEqualTo(0);
        }
    }

    public class ChangeToDoItemDescriptionCommandValidator : AbstractValidator<ChangeToDoItemDescriptionCommand>
    {
        public ChangeToDoItemDescriptionCommandValidator()
        {
            RuleFor(command => command.Description).NotEmpty();
        }
    }

    public class ChangeToDoItemDueDateCommandValidator : AbstractValidator<ChangeToDoItemDueDateCommand>
    {
        public ChangeToDoItemDueDateCommandValidator()
        {
            // If DueDate is not null, it should be >= today
            RuleFor(command => command.DueDate).GreaterThanOrEqualTo(DateTime.Now).When(command => command.DueDate != null);
        }
    }
}
