using FluentValidation;
using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Workspaces.Members.AddWorkspaceMember;

public sealed class AddWorkspaceMemberValidator : AbstractValidator<AddWorkspaceMemberCommand>
{
    public AddWorkspaceMemberValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(command => command.Role)
            .Must(role => role is WorkspaceRole.Admin or WorkspaceRole.Member)
            .WithMessage("Role must be Admin or Member.");
    }
}
