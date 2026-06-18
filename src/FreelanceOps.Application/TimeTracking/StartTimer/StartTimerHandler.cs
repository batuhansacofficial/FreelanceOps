using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.TimeTracking;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.TimeTracking.StartTimer;

public sealed class StartTimerHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<StartTimerCommand> validator)
{
    public async Task<StartTimerResponse> Handle(
        StartTimerCommand command,
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

        var task = await TimeTrackingTaskGuard.GetActiveTaskAsync(
            dbContext,
            command.WorkspaceId,
            command.TaskId,
            cancellationToken);

        var hasActiveTimer = await dbContext.TimeEntries
            .AnyAsync(
                timeEntry =>
                    timeEntry.UserId == userId &&
                    timeEntry.EndedAtUtc == null &&
                    timeEntry.DurationMinutes == null &&
                    !timeEntry.IsDeleted,
                cancellationToken);

        if (hasActiveTimer)
        {
            throw new ConflictException("User already has an active timer.");
        }

        var timeEntry = TimeEntry.StartTimer(
            command.WorkspaceId,
            task.ProjectId,
            task.Id,
            userId,
            DateTime.UtcNow,
            command.Description);

        dbContext.TimeEntries.Add(timeEntry);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new StartTimerResponse(
            timeEntry.Id,
            timeEntry.WorkspaceId,
            timeEntry.ProjectId,
            timeEntry.TaskId,
            timeEntry.UserId,
            timeEntry.StartedAtUtc,
            timeEntry.EndedAtUtc,
            timeEntry.DurationMinutes,
            timeEntry.Description,
            timeEntry.Source,
            timeEntry.IsRunning);
    }
}
