using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Clients.UpdateClient;

public sealed class UpdateClientHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<UpdateClientCommand> validator)
{
    public async Task Handle(
        UpdateClientCommand command,
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

        await workspaceAuthorizationService.EnsureAnyRoleAsync(
            userId,
            command.WorkspaceId,
            WorkspaceRoles.Managers,
            cancellationToken);

        var client = await dbContext.Clients
            .FirstOrDefaultAsync(
                client =>
                    client.Id == command.ClientId &&
                    client.WorkspaceId == command.WorkspaceId &&
                    !client.IsDeleted,
                cancellationToken);

        if (client is null)
        {
            throw new NotFoundException("Client was not found.");
        }

        client.Update(
            command.Name,
            command.Email,
            command.CompanyName,
            command.Notes);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
