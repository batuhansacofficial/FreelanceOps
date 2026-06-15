using FluentValidation;

namespace FreelanceOps.Application.Workspaces.CreateWorkspace;

public sealed class CreateWorkspaceValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(120);
    }
}
