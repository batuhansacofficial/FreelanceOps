using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Clients.GetClientById;

public sealed class GetClientByIdHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task<ClientDetailResponse> Handle(
        GetClientByIdQuery query,
        CancellationToken cancellationToken)
    {
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

        var client = await dbContext.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(
                client =>
                    client.Id == query.ClientId &&
                    client.WorkspaceId == query.WorkspaceId &&
                    !client.IsDeleted,
                cancellationToken);

        if (client is null)
        {
            throw new NotFoundException("Client was not found.");
        }

        return new ClientDetailResponse(
            client.Id,
            client.WorkspaceId,
            client.Name,
            client.Email,
            client.CompanyName,
            client.Notes,
            client.CreatedAtUtc,
            client.UpdatedAtUtc);
    }
}
