using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.ProjectTasks.GetProjectTaskById;

public sealed class GetProjectTaskByIdHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task<ProjectTaskDetailResponse> Handle(
        GetProjectTaskByIdQuery query,
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

        var task = await dbContext.ProjectTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(
                task =>
                    task.Id == query.TaskId &&
                    task.WorkspaceId == query.WorkspaceId &&
                    !task.IsDeleted,
                cancellationToken);

        if (task is null)
        {
            throw new NotFoundException("Project task was not found.");
        }

        var projectExists = await dbContext.Projects
            .AnyAsync(
                project =>
                    project.Id == task.ProjectId &&
                    project.WorkspaceId == query.WorkspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project task was not found.");
        }

        return new ProjectTaskDetailResponse(
            task.Id,
            task.WorkspaceId,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.AssignedToUserId,
            task.CreatedAtUtc,
            task.UpdatedAtUtc);
    }
}
