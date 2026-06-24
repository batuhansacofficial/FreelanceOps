using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Proposals.AcceptProposal;

public sealed class AcceptProposalHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    INotificationService notificationService)
{
    public async Task Handle(
        AcceptProposalCommand command,
        CancellationToken cancellationToken)
    {
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

        proposal.Accept(DateOnly.FromDateTime(DateTime.UtcNow));

        await notificationService.CreateForWorkspaceRolesAsync(
            proposal.WorkspaceId,
            WorkspaceRoles.Managers,
            NotificationType.ProposalAccepted,
            "Proposal accepted",
            $"Proposal {proposal.ProposalNumber} was accepted.",
            "Proposal",
            proposal.Id,
            $"proposal-accepted:{proposal.Id}",
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
