using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Proposals;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Infrastructure.Proposals;

public sealed class ProposalNumberGenerator(
    IApplicationDbContext dbContext) : IProposalNumberGenerator
{
    public async Task<string> GenerateAsync(
        Guid workspaceId,
        DateTime createdAtUtc,
        CancellationToken cancellationToken)
    {
        var yearStart = new DateTime(createdAtUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextYearStart = yearStart.AddYears(1);
        var proposalCount = await dbContext.Proposals
            .CountAsync(
                proposal =>
                    proposal.WorkspaceId == workspaceId &&
                    proposal.CreatedAtUtc >= yearStart &&
                    proposal.CreatedAtUtc < nextYearStart,
                cancellationToken);

        return $"PROP-{createdAtUtc.Year}-{proposalCount + 1:0000}";
    }
}
