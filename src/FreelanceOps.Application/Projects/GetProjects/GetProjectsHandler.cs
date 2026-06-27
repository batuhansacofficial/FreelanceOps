using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Projects.GetProjects;

public sealed class GetProjectsHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetProjectsQuery> validator)
{
    public async Task<PagedResult<ProjectSummaryResponse>> Handle(
        GetProjectsQuery query,
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

        var projectsQuery = dbContext.Projects
            .AsNoTracking()
            .Where(project => project.WorkspaceId == query.WorkspaceId && !project.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();

            projectsQuery = projectsQuery.Where(project =>
                project.Name.ToLower().Contains(search) ||
                (project.Description != null && project.Description.ToLower().Contains(search)));
        }

        if (query.Status.HasValue)
        {
            projectsQuery = projectsQuery.Where(project => project.Status == query.Status.Value);
        }

        if (query.ClientId.HasValue)
        {
            projectsQuery = projectsQuery.Where(project => project.ClientId == query.ClientId.Value);
        }

        var totalCount = await projectsQuery.CountAsync(cancellationToken);
        var projects = await projectsQuery
            .OrderByDescending(project => project.CreatedAtUtc)
            .ThenBy(project => project.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(project => new ProjectSummaryResponse(
                project.Id,
                project.WorkspaceId,
                project.ClientId,
                project.Name,
                project.Status,
                project.StartDate,
                project.Deadline,
                project.CreatedAtUtc,
                project.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectSummaryResponse>(
            projects,
            query.Page,
            query.PageSize,
            totalCount);
    }
}
