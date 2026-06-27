using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces.Members;
using FreelanceOps.Domain.Users;
using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Workspaces.Members.AddWorkspaceMember;

public sealed class AddWorkspaceMemberHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<AddWorkspaceMemberCommand> validator)
{
    public async Task<WorkspaceMemberResponse> Handle(
        AddWorkspaceMemberCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var requesterUserId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureAnyRoleAsync(
            requesterUserId,
            command.WorkspaceId,
            WorkspaceRoles.Managers,
            cancellationToken);

        var email = User.NormalizeEmail(command.Email);
        var user = await dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == email && user.IsActive, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User was not found.");
        }

        var existingMember = await dbContext.WorkspaceMembers
            .FirstOrDefaultAsync(
                member => member.WorkspaceId == command.WorkspaceId && member.UserId == user.Id,
                cancellationToken);

        if (existingMember is not null && existingMember.IsActive)
        {
            throw new ConflictException("User is already an active workspace member.");
        }

        WorkspaceMember member;

        if (existingMember is null)
        {
            member = WorkspaceMember.Create(command.WorkspaceId, user.Id, command.Role);
            dbContext.WorkspaceMembers.Add(member);
        }
        else
        {
            existingMember.Reactivate(command.Role);
            member = existingMember;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkspaceMemberResponse(
            member.Id,
            user.Id,
            user.Email,
            user.FullName,
            member.Role.ToString(),
            member.JoinedAtUtc);
    }
}
