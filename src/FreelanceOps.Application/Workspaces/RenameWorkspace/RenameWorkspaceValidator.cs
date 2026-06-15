using FluentValidation;

namespace FreelanceOps.Application.Workspaces.RenameWorkspace;

public sealed class RenameWorkspaceValidator : AbstractValidator<RenameWorkspaceCommand>
{
    public RenameWorkspaceValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(120);
    }
}
