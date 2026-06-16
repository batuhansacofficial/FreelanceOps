using FluentValidation;

namespace FreelanceOps.Application.ProjectTasks.ChangeProjectTaskStatus;

public sealed class ChangeProjectTaskStatusValidator : AbstractValidator<ChangeProjectTaskStatusCommand>
{
    public ChangeProjectTaskStatusValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.TaskId)
            .NotEmpty();

        RuleFor(command => command.Status)
            .IsInEnum();
    }
}
