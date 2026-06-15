using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces.Members;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Workspaces.Members.GetWorkspaceMembers;

public sealed class GetWorkspaceMembersHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task<IReadOnlyCollection<WorkspaceMemberResponse>> Handle(
        GetWorkspaceMembersQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();

        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == query.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureMemberAsync(
            userId,
            query.WorkspaceId,
            cancellationToken);

        var members = await dbContext.WorkspaceMembers
            .Where(member => member.WorkspaceId == query.WorkspaceId && member.IsActive)
            .Join(
                dbContext.Users,
                member => member.UserId,
                user => user.Id,
                (member, user) => new
                {
                    member.Id,
                    UserId = user.Id,
                    user.Email,
                    user.FullName,
                    member.Role,
                    member.JoinedAtUtc
                })
            .OrderBy(member => member.FullName)
            .ToListAsync(cancellationToken);

        return members
            .Select(member => new WorkspaceMemberResponse(
                member.Id,
                member.UserId,
                member.Email,
                member.FullName,
                member.Role.ToString(),
                member.JoinedAtUtc))
            .ToList();
    }
}
