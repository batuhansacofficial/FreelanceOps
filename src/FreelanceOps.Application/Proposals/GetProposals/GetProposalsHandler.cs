using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Proposals.GetProposals;

public sealed class GetProposalsHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetProposalsQuery> validator)
{
    public async Task<PagedResult<ProposalSummaryResponse>> Handle(
        GetProposalsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();

        await ProposalGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            query.WorkspaceId,
            cancellationToken);

        var proposalsQuery = dbContext.Proposals
            .AsNoTracking()
            .Where(proposal =>
                proposal.WorkspaceId == query.WorkspaceId &&
                !proposal.IsDeleted);

        if (query.Status.HasValue)
        {
            proposalsQuery = proposalsQuery.Where(proposal => proposal.Status == query.Status.Value);
        }

        if (query.ClientId.HasValue)
        {
            proposalsQuery = proposalsQuery.Where(proposal => proposal.ClientId == query.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            proposalsQuery = proposalsQuery.Where(
                proposal =>
                    proposal.ProposalNumber.ToLower().Contains(search) ||
                    proposal.Title.ToLower().Contains(search));
        }

        var totalCount = await proposalsQuery.CountAsync(cancellationToken);
        var proposals = await proposalsQuery
            .OrderByDescending(proposal => proposal.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(proposal => new ProposalSummaryResponse(
                proposal.Id,
                proposal.WorkspaceId,
                proposal.ClientId,
                proposal.ConvertedProjectId,
                proposal.ProposalNumber,
                proposal.Title,
                proposal.Status,
                proposal.ValidUntil,
                proposal.Currency,
                proposal.TotalAmount,
                proposal.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProposalSummaryResponse>(
            proposals,
            query.Page,
            query.PageSize,
            totalCount);
    }
}
