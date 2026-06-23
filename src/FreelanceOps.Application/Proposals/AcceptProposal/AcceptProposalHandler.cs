using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Proposals.AcceptProposal;

public sealed class AcceptProposalHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
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

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
