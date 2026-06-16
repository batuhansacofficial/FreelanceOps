using FluentValidation;

namespace FreelanceOps.Application.ProjectTasks.UpdateProjectTask;

public sealed class UpdateProjectTaskValidator : AbstractValidator<UpdateProjectTaskCommand>
{
    public UpdateProjectTaskValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.TaskId)
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
