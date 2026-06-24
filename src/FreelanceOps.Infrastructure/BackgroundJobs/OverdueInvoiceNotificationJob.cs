using FreelanceOps.Application.Abstractions.Notifications;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.BackgroundJobs.OverdueInvoiceNotificationJob;
using FreelanceOps.Domain.Billing;
using FreelanceOps.Domain.Notifications;
using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Infrastructure.BackgroundJobs;

public sealed class OverdueInvoiceNotificationJob(
    IApplicationDbContext dbContext,
    INotificationService notificationService) : IOverdueInvoiceNotificationJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var invoices = await dbContext.Invoices
            .Where(invoice =>
                invoice.Status == InvoiceStatus.Sent &&
                invoice.DueDate < today &&
                invoice.TotalAmount - invoice.PaidAmount > 0 &&
                !invoice.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var invoice in invoices)
        {
            await notificationService.CreateForWorkspaceRolesAsync(
                invoice.WorkspaceId,
                [WorkspaceRole.Owner, WorkspaceRole.Admin],
                NotificationType.InvoiceOverdue,
                "Invoice overdue",
                $"Invoice {invoice.InvoiceNumber} is overdue.",
                "Invoice",
                invoice.Id,
                $"invoice-overdue:{invoice.Id}",
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
