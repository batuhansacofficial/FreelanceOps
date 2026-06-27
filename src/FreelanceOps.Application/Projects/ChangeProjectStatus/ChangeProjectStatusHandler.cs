using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Projects.ChangeProjectStatus;

public sealed class ChangeProjectStatusHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<ChangeProjectStatusCommand> validator)
{
    public async Task Handle(
        ChangeProjectStatusCommand command,
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

        var project = await dbContext.Projects
            .FirstOrDefaultAsync(
                project =>
                    project.Id == command.ProjectId &&
                    project.WorkspaceId == command.WorkspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project was not found.");
        }

        project.ChangeStatus(command.Status);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
