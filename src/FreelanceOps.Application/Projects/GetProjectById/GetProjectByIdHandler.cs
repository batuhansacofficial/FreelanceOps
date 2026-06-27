using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Projects.GetProjectById;

public sealed class GetProjectByIdHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task<ProjectDetailResponse> Handle(
        GetProjectByIdQuery query,
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

        var project = await dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project =>
                    project.Id == query.ProjectId &&
                    project.WorkspaceId == query.WorkspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project was not found.");
        }

        return new ProjectDetailResponse(
            project.Id,
            project.WorkspaceId,
            project.ClientId,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.Deadline,
            project.CreatedAtUtc,
            project.UpdatedAtUtc);
    }
}
