using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.TimeTracking.GetTimeEntries;

public sealed class GetTimeEntriesHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IWorkspaceAccessService workspaceAccessService,
    IValidator<GetTimeEntriesQuery> validator)
{
    public async Task<PagedResult<TimeEntryResponse>> Handle(
        GetTimeEntriesQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var currentUserId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == query.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureMemberAsync(
            currentUserId,
            query.WorkspaceId,
            cancellationToken);

        if (query.ProjectId.HasValue)
        {
            var projectExists = await dbContext.Projects
                .AnyAsync(
                    project =>
                        project.Id == query.ProjectId.Value &&
                        project.WorkspaceId == query.WorkspaceId &&
                        !project.IsDeleted,
                    cancellationToken);

            if (!projectExists)
            {
                throw new NotFoundException("Project was not found.");
            }
        }

        if (query.TaskId.HasValue)
        {
            var task = await TimeTrackingTaskGuard.GetActiveTaskAsync(
                dbContext,
                query.WorkspaceId,
                query.TaskId.Value,
                cancellationToken);

            if (query.ProjectId.HasValue && task.ProjectId != query.ProjectId.Value)
            {
                throw new NotFoundException("Project task was not found.");
            }
        }

        var role = await workspaceAccessService.GetRoleAsync(
            currentUserId,
            query.WorkspaceId,
            cancellationToken);
        var isManager = role.HasValue && WorkspaceRoles.Managers.Contains(role.Value);
        var effectiveUserId = isManager ? query.UserId : currentUserId;

        var timeEntriesQuery = dbContext.TimeEntries
            .AsNoTracking()
            .Where(timeEntry =>
                timeEntry.WorkspaceId == query.WorkspaceId &&
                !timeEntry.IsDeleted);

        if (effectiveUserId.HasValue)
        {
            timeEntriesQuery = timeEntriesQuery.Where(
                timeEntry => timeEntry.UserId == effectiveUserId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            timeEntriesQuery = timeEntriesQuery.Where(
                timeEntry => timeEntry.ProjectId == query.ProjectId.Value);
        }

        if (query.TaskId.HasValue)
        {
            timeEntriesQuery = timeEntriesQuery.Where(
                timeEntry => timeEntry.TaskId == query.TaskId.Value);
        }

        if (query.From.HasValue)
        {
            var fromUtc = query.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            timeEntriesQuery = timeEntriesQuery.Where(
                timeEntry => timeEntry.StartedAtUtc >= fromUtc);
        }

        if (query.To.HasValue)
        {
            var toExclusiveUtc = query.To.Value
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            timeEntriesQuery = timeEntriesQuery.Where(
                timeEntry => timeEntry.StartedAtUtc < toExclusiveUtc);
        }

        var totalCount = await timeEntriesQuery.CountAsync(cancellationToken);
        var timeEntries = await timeEntriesQuery
            .OrderByDescending(timeEntry => timeEntry.StartedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(timeEntry => new TimeEntryResponse(
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
                timeEntry.EndedAtUtc == null && timeEntry.DurationMinutes == null,
                timeEntry.CreatedAtUtc,
                timeEntry.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<TimeEntryResponse>(
            timeEntries,
            query.Page,
            query.PageSize,
            totalCount);
    }
}
