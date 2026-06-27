namespace FreelanceOps.Application.Abstractions.Proposals;

public interface IProposalNumberGenerator
{
    Task<string> GenerateAsync(
        Guid workspaceId,
        DateTime createdAtUtc,
        CancellationToken cancellationToken);
}
