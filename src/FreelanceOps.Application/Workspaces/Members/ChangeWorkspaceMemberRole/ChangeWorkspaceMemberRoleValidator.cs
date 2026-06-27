using FluentValidation;
using FreelanceOps.Domain.Workspaces;

namespace FreelanceOps.Application.Workspaces.Members.ChangeWorkspaceMemberRole;

public sealed class ChangeWorkspaceMemberRoleValidator : AbstractValidator<ChangeWorkspaceMemberRoleCommand>
{
    public ChangeWorkspaceMemberRoleValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.MemberId)
            .NotEmpty();

        RuleFor(command => command.Role)
            .Must(role => role is WorkspaceRole.Admin or WorkspaceRole.Member)
            .WithMessage("Role must be Admin or Member.");
    }
}
