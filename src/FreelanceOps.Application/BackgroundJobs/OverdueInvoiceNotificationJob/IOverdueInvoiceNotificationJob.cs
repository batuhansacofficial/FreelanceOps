namespace FreelanceOps.Application.BackgroundJobs.OverdueInvoiceNotificationJob;

public interface IOverdueInvoiceNotificationJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
