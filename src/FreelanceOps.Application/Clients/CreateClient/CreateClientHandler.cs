using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Clients.CreateClient;

public sealed class CreateClientHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<CreateClientCommand> validator)
{
    public async Task<CreateClientResponse> Handle(
        CreateClientCommand command,
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

        var client = new Client(
            command.WorkspaceId,
            command.Name,
            command.Email,
            command.CompanyName,
            command.Notes);

        dbContext.Clients.Add(client);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateClientResponse(
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
