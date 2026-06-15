using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Clients.GetClients;

public sealed class GetClientsHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetClientsQuery> validator)
{
    public async Task<PagedResult<ClientSummaryResponse>> Handle(
        GetClientsQuery query,
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

        var clientsQuery = dbContext.Clients
            .AsNoTracking()
            .Where(client => client.WorkspaceId == query.WorkspaceId && !client.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();

            clientsQuery = clientsQuery.Where(client =>
                client.Name.ToLower().Contains(search) ||
                (client.Email != null && client.Email.ToLower().Contains(search)) ||
                (client.CompanyName != null && client.CompanyName.ToLower().Contains(search)));
        }

        var totalCount = await clientsQuery.CountAsync(cancellationToken);
        var clients = await clientsQuery
            .OrderByDescending(client => client.CreatedAtUtc)
            .ThenBy(client => client.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(client => new ClientSummaryResponse(
                client.Id,
                client.WorkspaceId,
                client.Name,
                client.Email,
                client.CompanyName,
                client.CreatedAtUtc,
                client.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientSummaryResponse>(
            clients,
            query.Page,
            query.PageSize,
            totalCount);
    }
}
