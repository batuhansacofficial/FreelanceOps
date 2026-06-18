using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.TimeTracking.UpdateTimeEntry;

public sealed class UpdateTimeEntryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IWorkspaceAccessService workspaceAccessService,
    IValidator<UpdateTimeEntryCommand> validator)
{
    public async Task Handle(
        UpdateTimeEntryCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var currentUserId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureMemberAsync(
            currentUserId,
            command.WorkspaceId,
            cancellationToken);

        var timeEntry = await dbContext.TimeEntries
            .FirstOrDefaultAsync(
                timeEntry =>
                    timeEntry.Id == command.TimeEntryId &&
                    timeEntry.WorkspaceId == command.WorkspaceId &&
                    !timeEntry.IsDeleted,
                cancellationToken);

        if (timeEntry is null)
        {
            throw new NotFoundException("Time entry was not found.");
        }

        var role = await workspaceAccessService.GetRoleAsync(
            currentUserId,
            command.WorkspaceId,
            cancellationToken);
        var isManager = role.HasValue && WorkspaceRoles.Managers.Contains(role.Value);

        if (!isManager && timeEntry.UserId != currentUserId)
        {
            throw new ForbiddenException("You cannot update another user's time entry.");
        }

        timeEntry.UpdateManualEntry(
            command.StartedAtUtc,
            command.DurationMinutes,
            command.Description);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
