using FluentValidation;

namespace FreelanceOps.Application.Projects.ChangeProjectStatus;

public sealed class ChangeProjectStatusValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.ProjectId)
            .NotEmpty();

        RuleFor(command => command.Status)
            .IsInEnum();
    }
}
