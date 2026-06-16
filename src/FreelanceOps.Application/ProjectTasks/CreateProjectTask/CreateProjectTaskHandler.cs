using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.ProjectTasks.CreateProjectTask;

public sealed class CreateProjectTaskHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<CreateProjectTaskCommand> validator)
{
    public async Task<CreateProjectTaskResponse> Handle(
        CreateProjectTaskCommand command,
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

        var projectExists = await dbContext.Projects
            .AnyAsync(
                project =>
                    project.Id == command.ProjectId &&
                    project.WorkspaceId == command.WorkspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project was not found.");
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

        var task = new ProjectTask(
            command.WorkspaceId,
            command.ProjectId,
            command.Title,
            command.Description,
            command.Priority,
            command.DueDate,
            command.AssignedToUserId);

        dbContext.ProjectTasks.Add(task);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectTaskResponse(
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
