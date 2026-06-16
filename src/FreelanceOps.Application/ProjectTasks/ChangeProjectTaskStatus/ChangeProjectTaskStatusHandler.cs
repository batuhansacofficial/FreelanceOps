using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.ProjectTasks.ChangeProjectTaskStatus;

public sealed class ChangeProjectTaskStatusHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<ChangeProjectTaskStatusCommand> validator)
{
    public async Task Handle(
        ChangeProjectTaskStatusCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureMemberAsync(
            userId,
            command.WorkspaceId,
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

        task.ChangeStatus(command.Status);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
