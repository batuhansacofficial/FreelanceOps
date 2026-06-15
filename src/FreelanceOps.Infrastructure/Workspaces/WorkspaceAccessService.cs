using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Domain.Workspaces;
using FreelanceOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Infrastructure.Workspaces;

public sealed class WorkspaceAccessService(ApplicationDbContext dbContext) : IWorkspaceAccessService
{
    public async Task<bool> HasAccessAsync(
        Guid userId,
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        return await ActiveMemberQuery(userId, workspaceId)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> HasAnyRoleAsync(
        Guid userId,
        Guid workspaceId,
        IReadOnlyCollection<WorkspaceRole> roles,
        CancellationToken cancellationToken)
    {
        return await ActiveMemberQuery(userId, workspaceId)
            .AnyAsync(member => roles.Contains(member.Role), cancellationToken);
    }

    public async Task<WorkspaceRole?> GetRoleAsync(
        Guid userId,
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        return await ActiveMemberQuery(userId, workspaceId)
            .Select(member => (WorkspaceRole?)member.Role)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<WorkspaceMember> ActiveMemberQuery(Guid userId, Guid workspaceId)
    {
        return dbContext.WorkspaceMembers
            .Where(member =>
                member.UserId == userId &&
                member.WorkspaceId == workspaceId &&
                member.IsActive)
            .Join(
                dbContext.Workspaces.Where(workspace => !workspace.IsDeleted),
                member => member.WorkspaceId,
                workspace => workspace.Id,
                (member, _) => member);
    }
}
