using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Billing;
using FreelanceOps.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Reports.GetProjectPerformance;

public sealed class GetProjectPerformanceHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetProjectPerformanceQuery> validator)
{
    public async Task<ProjectPerformanceResponse> Handle(
        GetProjectPerformanceQuery query,
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
        var projects = await (
                from project in dbContext.Projects.AsNoTracking()
                join client in dbContext.Clients.AsNoTracking()
                    on project.ClientId equals client.Id
                where project.WorkspaceId == query.WorkspaceId
                      && !project.IsDeleted
                select new ProjectRow(
                    project.Id,
                    project.Name,
                    project.ClientId,
                    client.Name,
                    project.Status))
            .ToListAsync(cancellationToken);

        var timeRows = await dbContext.TimeEntries
            .AsNoTracking()
            .Where(timeEntry =>
                timeEntry.WorkspaceId == query.WorkspaceId &&
                !timeEntry.IsDeleted &&
                timeEntry.DurationMinutes != null &&
                timeEntry.StartedAtUtc >= range.FromUtc &&
                timeEntry.StartedAtUtc < range.ToExclusiveUtc)
            .Select(timeEntry => new MinutesRow(
                timeEntry.ProjectId,
                timeEntry.DurationMinutes!.Value))
            .ToListAsync(cancellationToken);

        var invoiceRows = await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.WorkspaceId == query.WorkspaceId &&
                !invoice.IsDeleted &&
                invoice.Status != InvoiceStatus.Cancelled &&
                invoice.ProjectId != null &&
                invoice.IssueDate >= range.From &&
                invoice.IssueDate <= range.To)
            .Select(invoice => new InvoiceRow(
                invoice.ProjectId!.Value,
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
                      && invoice.ProjectId != null
                      && payment.PaidAt >= range.From
                      && payment.PaidAt <= range.To
                select new AmountRow(invoice.ProjectId!.Value, payment.Amount))
            .ToListAsync(cancellationToken);

        var trackedMinutes = timeRows
            .GroupBy(row => row.Id)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Minutes));
        var invoiceTotals = invoiceRows
            .GroupBy(row => row.ProjectId)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.TotalAmount));
        var paidAmounts = paymentRows
            .GroupBy(row => row.Id)
            .ToDictionary(group => group.Key, group => group.Sum(row => row.Amount));
        var outstandingAmounts = invoiceRows
            .Where(row =>
                row.Status == InvoiceStatus.Sent &&
                row.TotalAmount - row.PaidAmount > 0)
            .GroupBy(row => row.ProjectId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(row => row.TotalAmount - row.PaidAmount));

        var items = projects
            .Select(project =>
            {
                var minutes = trackedMinutes.GetValueOrDefault(project.Id);
                var paidAmount = paidAmounts.GetValueOrDefault(project.Id);

                return new ProjectPerformanceItemResponse(
                    project.Id,
                    project.Name,
                    project.ClientId,
                    project.ClientName,
                    project.Status,
                    minutes,
                    ReportMath.ToHours(minutes),
                    invoiceTotals.GetValueOrDefault(project.Id),
                    paidAmount,
                    outstandingAmounts.GetValueOrDefault(project.Id),
                    ReportMath.RevenuePerHour(paidAmount, minutes));
            })
            .OrderByDescending(item => item.PaidAmount)
            .ThenBy(item => item.ProjectName)
            .ToArray();

        return new ProjectPerformanceResponse(range.From, range.To, items);
    }

    private sealed record ProjectRow(
        Guid Id,
        string Name,
        Guid ClientId,
        string ClientName,
        ProjectStatus Status);

    private sealed record MinutesRow(Guid Id, int Minutes);

    private sealed record AmountRow(Guid Id, decimal Amount);

    private sealed record InvoiceRow(
        Guid ProjectId,
        InvoiceStatus Status,
        decimal TotalAmount,
        decimal PaidAmount);
}
