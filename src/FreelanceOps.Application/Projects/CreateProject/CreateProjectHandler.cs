using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Projects.CreateProject;

public sealed class CreateProjectHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<CreateProjectCommand> validator)
{
    public async Task<CreateProjectResponse> Handle(
        CreateProjectCommand command,
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

        var clientExists = await dbContext.Clients
            .AnyAsync(
                client =>
                    client.Id == command.ClientId &&
                    client.WorkspaceId == command.WorkspaceId &&
                    !client.IsDeleted,
                cancellationToken);

        if (!clientExists)
        {
            throw new NotFoundException("Client was not found.");
        }

        var project = new Project(
            command.WorkspaceId,
            command.ClientId,
            command.Name,
            command.Description,
            command.StartDate,
            command.Deadline);

        dbContext.Projects.Add(project);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectResponse(
            project.Id,
            project.WorkspaceId,
            project.ClientId,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.Deadline,
            project.CreatedAtUtc,
            project.UpdatedAtUtc);
    }
}
