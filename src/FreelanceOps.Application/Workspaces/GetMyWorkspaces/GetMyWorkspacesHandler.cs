using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Workspaces.GetMyWorkspaces;

public sealed class GetMyWorkspacesHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
{
    public async Task<IReadOnlyCollection<WorkspaceSummaryResponse>> Handle(
        GetMyWorkspacesQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();

        var workspaces = await dbContext.WorkspaceMembers
            .Where(member => member.UserId == userId && member.IsActive)
            .Join(
                dbContext.Workspaces.Where(workspace => !workspace.IsDeleted),
                member => member.WorkspaceId,
                workspace => workspace.Id,
                (member, workspace) => new
                {
                    workspace.Id,
                    workspace.Name,
                    workspace.Slug,
                    member.Role,
                    workspace.CreatedAtUtc
                })
            .OrderBy(workspace => workspace.Name)
            .ToListAsync(cancellationToken);

        return workspaces
            .Select(workspace => new WorkspaceSummaryResponse(
                workspace.Id,
                workspace.Name,
                workspace.Slug,
                workspace.Role.ToString(),
                workspace.CreatedAtUtc))
            .ToList();
    }
}
