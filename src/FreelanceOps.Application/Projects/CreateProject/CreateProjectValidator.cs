using FluentValidation;

namespace FreelanceOps.Application.Projects.CreateProject;

public sealed class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.ClientId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(command => command.Description)
            .MaximumLength(4000);

        RuleFor(command => command)
            .Must(command =>
                !command.StartDate.HasValue ||
                !command.Deadline.HasValue ||
                command.Deadline.Value >= command.StartDate.Value)
            .WithMessage("Deadline cannot be before start date.");
    }
}
