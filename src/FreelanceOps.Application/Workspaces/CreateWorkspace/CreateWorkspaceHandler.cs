using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Workspaces.CreateWorkspace;

public sealed class CreateWorkspaceHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    ISlugGenerator slugGenerator,
    IValidator<CreateWorkspaceCommand> validator)
{
    public async Task<CreateWorkspaceResponse> Handle(
        CreateWorkspaceCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();
        var slug = await CreateUniqueSlugAsync(command.Name, cancellationToken);
        var workspace = new Workspace(command.Name, slug, userId);

        dbContext.Workspaces.Add(workspace);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateWorkspaceResponse(
            workspace.Id,
            workspace.Name,
            workspace.Slug,
            WorkspaceRole.Owner.ToString(),
            workspace.CreatedAtUtc);
    }

    private async Task<string> CreateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = slugGenerator.Generate(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await dbContext.Workspaces.AnyAsync(workspace => workspace.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }
}
