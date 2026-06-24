using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Notifications;
using FreelanceOps.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Proposals.ConvertProposalToProject;

public sealed class ConvertProposalToProjectHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    INotificationService notificationService,
    IValidator<ConvertProposalToProjectCommand> validator)
{
    public async Task<ConvertProposalToProjectResponse> Handle(
        ConvertProposalToProjectCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();

        await ProposalGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            command.WorkspaceId,
            cancellationToken);

        var proposal = await dbContext.Proposals
            .FirstOrDefaultAsync(
                proposal =>
                    proposal.Id == command.ProposalId &&
                    proposal.WorkspaceId == command.WorkspaceId &&
                    !proposal.IsDeleted,
                cancellationToken);

        if (proposal is null)
        {
            throw new NotFoundException("Proposal was not found.");
        }

        await ProposalGuard.EnsureActiveClientAsync(
            dbContext,
            proposal.WorkspaceId,
            proposal.ClientId,
            cancellationToken);

        var project = new Project(
            proposal.WorkspaceId,
            proposal.ClientId,
            proposal.Title,
            proposal.Scope,
            command.StartDate,
            command.Deadline);

        proposal.MarkConverted(project.Id);
        dbContext.Projects.Add(project);

        await notificationService.CreateForWorkspaceRolesAsync(
            proposal.WorkspaceId,
            WorkspaceRoles.Managers,
            NotificationType.ProposalConvertedToProject,
            "Proposal converted",
            $"Proposal {proposal.ProposalNumber} was converted to a project.",
            "Proposal",
            proposal.Id,
            $"proposal-converted:{proposal.Id}",
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ConvertProposalToProjectResponse(
            project.Id,
            proposal.Id,
            project.Name,
            project.Status);
    }
}
