namespace FreelanceOps.Application.BackgroundJobs.ExpiredProposalJob;

public interface IExpiredProposalJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
