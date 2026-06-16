using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Projects.DeleteProject;

public sealed class DeleteProjectHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task Handle(
        DeleteProjectCommand command,
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

        var project = await dbContext.Projects
            .FirstOrDefaultAsync(
                project =>
                    project.Id == command.ProjectId &&
                    project.WorkspaceId == command.WorkspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project was not found.");
        }

        project.SoftDelete();

        var tasks = await dbContext.ProjectTasks
            .Where(task =>
                task.WorkspaceId == command.WorkspaceId &&
                task.ProjectId == command.ProjectId &&
                !task.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var task in tasks)
        {
            task.SoftDelete();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
