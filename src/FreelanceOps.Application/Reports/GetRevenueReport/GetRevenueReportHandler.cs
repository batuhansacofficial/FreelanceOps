using System.Globalization;
using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Reports.GetRevenueReport;

public sealed class GetRevenueReportHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetRevenueReportQuery> validator)
{
    public async Task<RevenueReportResponse> Handle(
        GetRevenueReportQuery query,
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

        var range = ReportDateRange.Resolve(query.From, query.To);
        var groupBy = query.GroupBy.ToLowerInvariant();
        var paymentRows = await (
                from payment in dbContext.PaymentRecords.AsNoTracking()
                join invoice in dbContext.Invoices.AsNoTracking()
                    on payment.InvoiceId equals invoice.Id
                where invoice.WorkspaceId == query.WorkspaceId
                      && !invoice.IsDeleted
                      && invoice.Status != InvoiceStatus.Cancelled
                      && payment.PaidAt >= range.From
                      && payment.PaidAt <= range.To
                select new PaymentRow(
                    payment.PaidAt,
                    invoice.Currency,
                    payment.Amount))
            .ToListAsync(cancellationToken);

        var itemsByCurrency = paymentRows
            .GroupBy(row => row.Currency)
            .Select(currencyGroup => new RevenueByCurrencyResponse(
                currencyGroup.Key,
                currencyGroup.Sum(row => row.Amount),
                currencyGroup
                    .GroupBy(row => FormatPeriod(row.PaidAt, groupBy))
                    .Select(periodGroup => new RevenuePeriodResponse(
                        periodGroup.Key,
                        periodGroup.Sum(row => row.Amount)))
                    .OrderBy(item => item.Period)
                    .ToArray()))
            .OrderBy(item => item.Currency)
            .ToArray();

        return new RevenueReportResponse(
            range.From,
            range.To,
            groupBy,
            itemsByCurrency);
    }

    private static string FormatPeriod(DateOnly paidAt, string groupBy)
    {
        return groupBy == "day"
            ? paidAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : paidAt.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    }

    private sealed record PaymentRow(
        DateOnly PaidAt,
        string Currency,
        decimal Amount);
}
