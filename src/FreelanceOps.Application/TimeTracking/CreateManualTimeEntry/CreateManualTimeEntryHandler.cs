using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.TimeTracking.GetTimeEntries;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.TimeTracking;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.TimeTracking.CreateManualTimeEntry;

public sealed class CreateManualTimeEntryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<CreateManualTimeEntryCommand> validator)
{
    public async Task<TimeEntryResponse> Handle(
        CreateManualTimeEntryCommand command,
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

        var timeEntry = TimeEntry.CreateManual(
            command.WorkspaceId,
            task.ProjectId,
            task.Id,
            userId,
            command.StartedAtUtc,
            command.DurationMinutes,
            command.Description);

        dbContext.TimeEntries.Add(timeEntry);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TimeEntryResponse(
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
            timeEntry.IsRunning,
            timeEntry.CreatedAtUtc,
            timeEntry.UpdatedAtUtc);
    }
}
