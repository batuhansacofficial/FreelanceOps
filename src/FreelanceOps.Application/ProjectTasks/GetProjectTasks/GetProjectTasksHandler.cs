using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.ProjectTasks.GetProjectTasks;

public sealed class GetProjectTasksHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetProjectTasksQuery> validator)
{
    public async Task<PagedResult<ProjectTaskSummaryResponse>> Handle(
        GetProjectTasksQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();
        var workspaceExists = await dbContext.Workspaces
            .AnyAsync(
                workspace => workspace.Id == query.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (!workspaceExists)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureMemberAsync(
            userId,
            query.WorkspaceId,
            cancellationToken);

        var projectExists = await dbContext.Projects
            .AnyAsync(
                project =>
                    project.Id == query.ProjectId &&
                    project.WorkspaceId == query.WorkspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project was not found.");
        }

        var tasksQuery = dbContext.ProjectTasks
            .AsNoTracking()
            .Where(task =>
                task.WorkspaceId == query.WorkspaceId &&
                task.ProjectId == query.ProjectId &&
                !task.IsDeleted);

        if (query.Status.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.Status == query.Status.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.AssignedToUserId == query.AssignedToUserId.Value);
        }

        var totalCount = await tasksQuery.CountAsync(cancellationToken);
        var tasks = await tasksQuery
            .OrderByDescending(task => task.CreatedAtUtc)
            .ThenBy(task => task.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(task => new ProjectTaskSummaryResponse(
                task.Id,
                task.WorkspaceId,
                task.ProjectId,
                task.Title,
                task.Status,
                task.Priority,
                task.DueDate,
                task.AssignedToUserId,
                task.CreatedAtUtc,
                task.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectTaskSummaryResponse>(
            tasks,
            query.Page,
            query.PageSize,
            totalCount);
    }
}
