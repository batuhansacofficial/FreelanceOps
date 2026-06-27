using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.TimeTracking.GetTimeSummary;

public sealed class GetTimeSummaryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IWorkspaceAccessService workspaceAccessService,
    IValidator<GetTimeSummaryQuery> validator)
{
    public async Task<TimeSummaryResponse> Handle(
        GetTimeSummaryQuery query,
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

        var entriesQuery = dbContext.TimeEntries
            .AsNoTracking()
            .Where(timeEntry =>
                timeEntry.WorkspaceId == query.WorkspaceId &&
                !timeEntry.IsDeleted &&
                timeEntry.DurationMinutes != null);

        if (!isManager)
        {
            entriesQuery = entriesQuery.Where(timeEntry => timeEntry.UserId == currentUserId);
        }

        if (query.ProjectId.HasValue)
        {
            entriesQuery = entriesQuery.Where(
                timeEntry => timeEntry.ProjectId == query.ProjectId.Value);
        }

        if (query.TaskId.HasValue)
        {
            entriesQuery = entriesQuery.Where(
                timeEntry => timeEntry.TaskId == query.TaskId.Value);
        }

        if (query.From.HasValue)
        {
            var fromUtc = query.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            entriesQuery = entriesQuery.Where(timeEntry => timeEntry.StartedAtUtc >= fromUtc);
        }

        if (query.To.HasValue)
        {
            var toExclusiveUtc = query.To.Value
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            entriesQuery = entriesQuery.Where(timeEntry => timeEntry.StartedAtUtc < toExclusiveUtc);
        }

        var entriesCount = await entriesQuery.CountAsync(cancellationToken);
        var totalMinutes = await entriesQuery
            .SumAsync(timeEntry => timeEntry.DurationMinutes, cancellationToken) ?? 0;

        var projectRows = await entriesQuery
            .GroupBy(timeEntry => timeEntry.ProjectId)
            .Select(group => new SummaryRow(
                group.Key,
                group.Sum(timeEntry => timeEntry.DurationMinutes) ?? 0))
            .ToListAsync(cancellationToken);
        var projectIds = projectRows.Select(row => row.Id).ToArray();
        var projectNames = await dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.WorkspaceId == query.WorkspaceId &&
                projectIds.Contains(project.Id))
            .ToDictionaryAsync(project => project.Id, project => project.Name, cancellationToken);
        var byProject = projectRows
            .Select(row => new ProjectTimeSummaryResponse(
                row.Id,
                projectNames.GetValueOrDefault(row.Id, "Unknown project"),
                row.TotalMinutes,
                ToHours(row.TotalMinutes)))
            .OrderByDescending(row => row.TotalMinutes)
            .ToArray();

        var userRows = await entriesQuery
            .GroupBy(timeEntry => timeEntry.UserId)
            .Select(group => new SummaryRow(
                group.Key,
                group.Sum(timeEntry => timeEntry.DurationMinutes) ?? 0))
            .ToListAsync(cancellationToken);
        var userIds = userRows.Select(row => row.Id).ToArray();
        var userNames = await dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.FullName, cancellationToken);
        var byUser = userRows
            .Select(row => new UserTimeSummaryResponse(
                row.Id,
                userNames.GetValueOrDefault(row.Id, "Unknown user"),
                row.TotalMinutes,
                ToHours(row.TotalMinutes)))
            .OrderByDescending(row => row.TotalMinutes)
            .ToArray();

        return new TimeSummaryResponse(
            totalMinutes,
            ToHours(totalMinutes),
            entriesCount,
            byProject,
            byUser);
    }

    private static double ToHours(int minutes)
    {
        return Math.Round(minutes / 60d, 2);
    }

    private sealed record SummaryRow(Guid Id, int TotalMinutes);
}
