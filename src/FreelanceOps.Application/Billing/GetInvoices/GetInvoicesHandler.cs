using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Billing.GetInvoices;

public sealed class GetInvoicesHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IWorkspaceAuthorizationService workspaceAuthorizationService,
    IValidator<GetInvoicesQuery> validator)
{
    public async Task<PagedResult<InvoiceSummaryResponse>> Handle(
        GetInvoicesQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userId = currentUserService.RequireUserId();

        await BillingGuard.EnsureManagerAsync(
            dbContext,
            workspaceAuthorizationService,
            userId,
            query.WorkspaceId,
            cancellationToken);

        var invoicesQuery = dbContext.Invoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.WorkspaceId == query.WorkspaceId &&
                !invoice.IsDeleted);

        if (query.Status.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(invoice => invoice.Status == query.Status.Value);
        }

        if (query.ClientId.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(invoice => invoice.ClientId == query.ClientId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(invoice => invoice.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            invoicesQuery = invoicesQuery.Where(
                invoice => invoice.InvoiceNumber.ToLower().Contains(search));
        }

        if (query.FromIssueDate.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(
                invoice => invoice.IssueDate >= query.FromIssueDate.Value);
        }

        if (query.ToIssueDate.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(
                invoice => invoice.IssueDate <= query.ToIssueDate.Value);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var totalCount = await invoicesQuery.CountAsync(cancellationToken);
        var invoices = await invoicesQuery
            .OrderByDescending(invoice => invoice.IssueDate)
            .ThenByDescending(invoice => invoice.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(invoice => new InvoiceSummaryResponse(
                invoice.Id,
                invoice.WorkspaceId,
                invoice.ClientId,
                invoice.ProjectId,
                invoice.InvoiceNumber,
                invoice.Status,
                invoice.IssueDate,
                invoice.DueDate,
                invoice.Currency,
                invoice.TotalAmount,
                invoice.PaidAmount,
                invoice.TotalAmount - invoice.PaidAmount,
                invoice.Status == InvoiceStatus.Sent &&
                invoice.DueDate < today &&
                invoice.TotalAmount - invoice.PaidAmount > 0,
                invoice.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceSummaryResponse>(
            invoices,
            query.Page,
            query.PageSize,
            totalCount);
    }
}
