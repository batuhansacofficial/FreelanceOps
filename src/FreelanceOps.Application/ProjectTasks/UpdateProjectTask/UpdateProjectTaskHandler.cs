using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.ProjectTasks.UpdateProjectTask;

public sealed class UpdateProjectTaskHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    INotificationService notificationService,
    IValidator<UpdateProjectTaskCommand> validator)
{
    public async Task Handle(
        UpdateProjectTaskCommand command,
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

        if (command.AssignedToUserId.HasValue)
        {
            var assignedMemberExists = await dbContext.WorkspaceMembers
                .AnyAsync(
                    member =>
                        member.WorkspaceId == command.WorkspaceId &&
                        member.UserId == command.AssignedToUserId.Value &&
                        member.IsActive,
                    cancellationToken);

            if (!assignedMemberExists)
            {
                throw new NotFoundException("Assigned workspace member was not found.");
            }
        }

        var previousAssignedToUserId = task.AssignedToUserId;

        task.Update(
            command.Title,
            command.Description,
            command.Priority,
            command.DueDate,
            command.AssignedToUserId);

        if (task.AssignedToUserId.HasValue &&
            task.AssignedToUserId != previousAssignedToUserId)
        {
            await notificationService.CreateAsync(
                task.WorkspaceId,
                task.AssignedToUserId.Value,
                NotificationType.TaskAssigned,
                "Task assigned",
                $"Task {task.Title} was assigned to you.",
                "Task",
                task.Id,
                $"task-assigned:{task.Id}:{task.AssignedToUserId.Value}",
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
