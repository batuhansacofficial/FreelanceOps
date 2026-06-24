using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.BackgroundJobs.ExpiredProposalJob;
using FreelanceOps.Domain.Notifications;
using FreelanceOps.Domain.Proposals;
using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Infrastructure.BackgroundJobs;

public sealed class ExpiredProposalJob(
    IApplicationDbContext dbContext,
    INotificationService notificationService) : IExpiredProposalJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var proposals = await dbContext.Proposals
            .Where(proposal =>
                proposal.Status == ProposalStatus.Sent &&
                proposal.ValidUntil < today &&
                !proposal.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var proposal in proposals)
        {
            proposal.MarkExpired(today);

            await notificationService.CreateForWorkspaceRolesAsync(
                proposal.WorkspaceId,
                [WorkspaceRole.Owner, WorkspaceRole.Admin],
                NotificationType.ProposalExpired,
                "Proposal expired",
                $"Proposal {proposal.ProposalNumber} expired.",
                "Proposal",
                proposal.Id,
                $"proposal-expired:{proposal.Id}",
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
