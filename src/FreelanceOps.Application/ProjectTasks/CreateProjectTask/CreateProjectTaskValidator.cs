using FluentValidation;

namespace FreelanceOps.Application.ProjectTasks.CreateProjectTask;

public sealed class CreateProjectTaskValidator : AbstractValidator<CreateProjectTaskCommand>
{
    public CreateProjectTaskValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .MaximumLength(4000);

        RuleFor(command => command.Priority)
            .IsInEnum();
    }
}
