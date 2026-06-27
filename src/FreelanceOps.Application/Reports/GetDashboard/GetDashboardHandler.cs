using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Billing;
using FreelanceOps.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Reports.GetDashboard;

public sealed class GetDashboardHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetDashboardQuery> validator)
{
    public async Task<DashboardResponse> Handle(
        GetDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var currentUserId = currentUserService.RequireUserId();

        await ReportGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            currentUserId,
            query.WorkspaceId,
            cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthRange = new ResolvedReportDateRange(monthStart, today);

        var totalClients = await dbContext.Clients
            .AsNoTracking()
            .CountAsync(
                client =>
                    client.WorkspaceId == query.WorkspaceId &&
                    !client.IsDeleted,
                cancellationToken);
        var activeProjects = await dbContext.Projects
            .AsNoTracking()
            .CountAsync(
                project =>
                    project.WorkspaceId == query.WorkspaceId &&
                    !project.IsDeleted &&
                    project.Status == ProjectStatus.Active,
                cancellationToken);
        var completedProjects = await dbContext.Projects
            .AsNoTracking()
            .CountAsync(
                project =>
                    project.WorkspaceId == query.WorkspaceId &&
                    !project.IsDeleted &&
                    project.Status == ProjectStatus.Completed,
                cancellationToken);

        var trackedMinutes = await dbContext.TimeEntries
            .AsNoTracking()
            .Where(timeEntry =>
                timeEntry.WorkspaceId == query.WorkspaceId &&
                !timeEntry.IsDeleted &&
                timeEntry.DurationMinutes != null &&
                timeEntry.StartedAtUtc >= monthRange.FromUtc &&
                timeEntry.StartedAtUtc < monthRange.ToExclusiveUtc)
            .SumAsync(timeEntry => timeEntry.DurationMinutes, cancellationToken) ?? 0;

        var paymentRows = await (
                from payment in dbContext.PaymentRecords.AsNoTracking()
                join invoice in dbContext.Invoices.AsNoTracking()
                    on payment.InvoiceId equals invoice.Id
                join client in dbContext.Clients.AsNoTracking()
                    on invoice.ClientId equals client.Id
                where invoice.WorkspaceId == query.WorkspaceId
                      && !invoice.IsDeleted
                      && invoice.Status != InvoiceStatus.Cancelled
                      && payment.PaidAt >= monthRange.From
                      && payment.PaidAt <= monthRange.To
                select new PaymentRow(
                    invoice.ClientId,
                    client.Name,
                    invoice.Currency,
                    payment.Amount))
            .ToListAsync(cancellationToken);

        var revenueByCurrency = paymentRows
            .GroupBy(row => row.Currency)
            .Select(group => new CurrencyAmountResponse(
                group.Key,
                group.Sum(row => row.Amount)))
            .OrderBy(item => item.Currency)
            .ToArray();

        var outstandingInvoices = dbContext.Invoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.WorkspaceId == query.WorkspaceId &&
                !invoice.IsDeleted &&
                invoice.Status == InvoiceStatus.Sent &&
                invoice.TotalAmount - invoice.PaidAmount > 0);
        var outstandingInvoiceAmount = await outstandingInvoices
            .SumAsync(
                invoice => (decimal?)(invoice.TotalAmount - invoice.PaidAmount),
                cancellationToken) ?? 0m;
        var openInvoiceCount = await outstandingInvoices.CountAsync(cancellationToken);
        var overdueInvoiceCount = await outstandingInvoices
            .CountAsync(invoice => invoice.DueDate < today, cancellationToken);

        var recentInvoices = await (
                from invoice in dbContext.Invoices.AsNoTracking()
                join client in dbContext.Clients.AsNoTracking()
                    on invoice.ClientId equals client.Id
                where invoice.WorkspaceId == query.WorkspaceId
                      && !invoice.IsDeleted
                      && invoice.Status != InvoiceStatus.Cancelled
                orderby invoice.IssueDate descending, invoice.CreatedAtUtc descending
                select new RecentInvoiceResponse(
                    invoice.Id,
                    invoice.InvoiceNumber,
                    invoice.ClientId,
                    client.Name,
                    invoice.Status,
                    invoice.IssueDate,
                    invoice.DueDate,
                    invoice.Currency,
                    invoice.TotalAmount,
                    invoice.PaidAmount,
                    invoice.TotalAmount - invoice.PaidAmount,
                    invoice.Status == InvoiceStatus.Sent &&
                    invoice.DueDate < today &&
                    invoice.TotalAmount - invoice.PaidAmount > 0))
            .Take(5)
            .ToListAsync(cancellationToken);

        var topClients = paymentRows
            .GroupBy(row => new
            {
                row.ClientId,
                row.ClientName,
                row.Currency
            })
            .Select(group => new TopClientRevenueResponse(
                group.Key.ClientId,
                group.Key.ClientName,
                group.Key.Currency,
                group.Sum(row => row.Amount)))
            .OrderByDescending(item => item.PaidRevenue)
            .ThenBy(item => item.ClientName)
            .Take(5)
            .ToArray();

        return new DashboardResponse(
            totalClients,
            activeProjects,
            completedProjects,
            trackedMinutes,
            ReportMath.ToHours(trackedMinutes),
            paymentRows.Sum(row => row.Amount),
            revenueByCurrency,
            outstandingInvoiceAmount,
            overdueInvoiceCount,
            openInvoiceCount,
            recentInvoices,
            topClients);
    }

    private sealed record PaymentRow(
        Guid ClientId,
        string ClientName,
        string Currency,
        decimal Amount);
}
