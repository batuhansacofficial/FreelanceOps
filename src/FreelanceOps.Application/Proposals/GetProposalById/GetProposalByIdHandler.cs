using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Proposals.GetProposalById;

public sealed class GetProposalByIdHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService)
{
    public async Task<ProposalDetailResponse> Handle(
        GetProposalByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.RequireUserId();

        await ProposalGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            query.WorkspaceId,
            cancellationToken);

        var proposal = await dbContext.Proposals
            .AsNoTracking()
            .Include(proposal => proposal.Items)
            .FirstOrDefaultAsync(
                proposal =>
                    proposal.Id == query.ProposalId &&
                    proposal.WorkspaceId == query.WorkspaceId &&
                    !proposal.IsDeleted,
                cancellationToken);

        if (proposal is null)
        {
            throw new NotFoundException("Proposal was not found.");
        }

        return new ProposalDetailResponse(
            proposal.Id,
            proposal.WorkspaceId,
            proposal.ClientId,
            proposal.ConvertedProjectId,
            proposal.ProposalNumber,
            proposal.Title,
            proposal.Scope,
            proposal.Status,
            proposal.ValidUntil,
            proposal.Currency,
            proposal.SubtotalAmount,
            proposal.TaxAmount,
            proposal.TotalAmount,
            proposal.Items
                .Select(item => new ProposalItemResponse(
                    item.Id,
                    item.Description,
                    item.Quantity,
                    item.UnitPrice,
                    item.TaxRate,
                    item.SubtotalAmount,
                    item.TaxAmount,
                    item.TotalAmount))
                .ToArray(),
            proposal.CreatedAtUtc,
            proposal.UpdatedAtUtc);
    }
}
