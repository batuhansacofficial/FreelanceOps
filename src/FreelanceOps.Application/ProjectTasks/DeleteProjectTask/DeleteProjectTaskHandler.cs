using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.ProjectTasks.DeleteProjectTask;

public sealed class DeleteProjectTaskHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task Handle(
        DeleteProjectTaskCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureAnyRoleAsync(
            userId,
            command.WorkspaceId,
            WorkspaceRoles.Managers,
            cancellationToken);

        var task = await dbContext.ProjectTasks
            .FirstOrDefaultAsync(
                task =>
                    task.Id == command.TaskId &&
                    task.WorkspaceId == command.WorkspaceId &&
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
                    project.WorkspaceId == command.WorkspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project task was not found.");
        }

        task.SoftDelete();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
