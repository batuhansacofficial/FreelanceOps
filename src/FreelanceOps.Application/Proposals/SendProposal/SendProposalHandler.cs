using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Proposals.SendProposal;

public sealed class SendProposalHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    INotificationService notificationService)
{
    public async Task Handle(
        SendProposalCommand command,
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
            .Include(proposal => proposal.Items)
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

        proposal.MarkAsSent();

        await notificationService.CreateForWorkspaceRolesAsync(
            proposal.WorkspaceId,
            WorkspaceRoles.Managers,
            NotificationType.ProposalSent,
            "Proposal sent",
            $"Proposal {proposal.ProposalNumber} was sent.",
            "Proposal",
            proposal.Id,
            $"proposal-sent:{proposal.Id}",
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
