using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Workspaces.RenameWorkspace;

public sealed class RenameWorkspaceHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<RenameWorkspaceCommand> validator)
{
    public async Task Handle(RenameWorkspaceCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();
        var workspace = await dbContext.Workspaces
            .FirstOrDefaultAsync(
                workspace => workspace.Id == command.WorkspaceId && !workspace.IsDeleted,
                cancellationToken);

        if (workspace is null)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        await workspaceAuthorizationService.EnsureAnyRoleAsync(
            userId,
            command.WorkspaceId,
            WorkspaceRoles.Managers,
            cancellationToken);

        workspace.Rename(command.Name);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
