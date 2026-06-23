using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Reports.GetClientSummary;

public sealed class GetClientSummaryHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetClientSummaryQuery> validator)
{
    public async Task<ClientSummaryResponse> Handle(
        GetClientSummaryQuery query,
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
        var clients = await dbContext.Clients
            .AsNoTracking()
            .Where(client =>
                client.WorkspaceId == query.WorkspaceId &&
                !client.IsDeleted)
            .Select(client => new ClientRow(client.Id, client.Name))
            .ToListAsync(cancellationToken);

        var projectCounts = await dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.WorkspaceId == query.WorkspaceId &&
                !project.IsDeleted)
            .GroupBy(project => project.ClientId)
            .Select(group => new CountRow(group.Key, group.Count()))
            .ToDictionaryAsync(row => row.Id, row => row.Count, cancellationToken);

        var invoiceRows = await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.WorkspaceId == query.WorkspaceId &&
                !invoice.IsDeleted &&
                invoice.Status != InvoiceStatus.Cancelled &&
                invoice.IssueDate >= range.From &&
                invoice.IssueDate <= range.To)
            .Select(invoice => new InvoiceRow(
                invoice.ClientId,
                invoice.Status,
                invoice.TotalAmount,
                invoice.PaidAmount))
            .ToListAsync(cancellationToken);

        var paymentRows = await (
                from payment in dbContext.PaymentRecords.AsNoTracking()
                join invoice in dbContext.Invoices.AsNoTracking()
                    on payment.InvoiceId equals invoice.Id
                where invoice.WorkspaceId == query.WorkspaceId
                      && !invoice.IsDeleted
                      && invoice.Status != InvoiceStatus.Cancelled
                      && payment.PaidAt >= range.From
                      && payment.PaidAt <= range.To
                select new AmountRow(invoice.ClientId, payment.Amount))
            .ToListAsync(cancellationToken);

        var timeRows = await (
                from timeEntry in dbContext.TimeEntries.AsNoTracking()
                join project in dbContext.Projects.AsNoTracking()
                    on timeEntry.ProjectId equals project.Id
                where timeEntry.WorkspaceId == query.WorkspaceId
                      && !timeEntry.IsDeleted
                      && timeEntry.DurationMinutes != null
                      && timeEntry.StartedAtUtc >= range.FromUtc
                      && timeEntry.StartedAtUtc < range.ToExclusiveUtc
                      && project.WorkspaceId == query.WorkspaceId
                      && !project.IsDeleted
                select new MinutesRow(
                    project.ClientId,
                    timeEntry.DurationMinutes!.Value))
            .ToListAsync(cancellationToken);

        var invoiceCounts = invoiceRows
            .GroupBy(row => row.ClientId)
            .ToDictionary(group => group.Key, group => group.Count());
        var outstandingAmounts = invoiceRows
            .Where(row =>
                row.Status == InvoiceStatus.Sent &&
                row.TotalAmount - row.PaidAmount > 0)
            .GroupBy(row => row.ClientId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(row => row.TotalAmount - row.PaidAmount));
        var paidAmounts = paymentRows
            .GroupBy(row => row.Id)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Amount));
        var trackedMinutes = timeRows
            .GroupBy(row => row.Id)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Minutes));

        var items = clients
            .Select(client =>
            {
                var minutes = trackedMinutes.GetValueOrDefault(client.Id);

                return new ClientSummaryItemResponse(
                    client.Id,
                    client.Name,
                    projectCounts.GetValueOrDefault(client.Id),
                    invoiceCounts.GetValueOrDefault(client.Id),
                    paidAmounts.GetValueOrDefault(client.Id),
                    outstandingAmounts.GetValueOrDefault(client.Id),
                    minutes,
                    ReportMath.ToHours(minutes));
            })
            .OrderByDescending(item => item.PaidAmount)
            .ThenBy(item => item.ClientName)
            .ToArray();

        return new ClientSummaryResponse(range.From, range.To, items);
    }

    private sealed record ClientRow(Guid Id, string Name);

    private sealed record CountRow(Guid Id, int Count);

    private sealed record InvoiceRow(
        Guid ClientId,
        InvoiceStatus Status,
        decimal TotalAmount,
        decimal PaidAmount);

    private sealed record AmountRow(Guid Id, decimal Amount);

    private sealed record MinutesRow(Guid Id, int Minutes);
}
