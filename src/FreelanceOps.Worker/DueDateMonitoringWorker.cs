using FreelanceOps.Application.BackgroundJobs.ExpiredProposalJob;
using FreelanceOps.Application.BackgroundJobs.OverdueInvoiceNotificationJob;
using Microsoft.Extensions.Options;

namespace FreelanceOps.Worker;

public sealed class DueDateMonitoringWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<BackgroundJobOptions> options,
    ILogger<DueDateMonitoringWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(
            Math.Max(1, options.Value.DueDateMonitoringIntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunJobsAsync(stoppingToken);
            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task RunJobsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var expiredProposalJob = scope.ServiceProvider.GetRequiredService<IExpiredProposalJob>();
            var overdueInvoiceNotificationJob =
                scope.ServiceProvider.GetRequiredService<IOverdueInvoiceNotificationJob>();

            await expiredProposalJob.ExecuteAsync(cancellationToken);
            await overdueInvoiceNotificationJob.ExecuteAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Due date monitoring jobs failed.");
        }
    }
}
