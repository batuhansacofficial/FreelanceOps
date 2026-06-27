using FreelanceOps.Application.Abstractions.Billing;
using FreelanceOps.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Infrastructure.Billing;

public sealed class InvoiceNumberGenerator(
    IApplicationDbContext dbContext) : IInvoiceNumberGenerator
{
    public async Task<string> GenerateAsync(
        Guid workspaceId,
        DateOnly issueDate,
        CancellationToken cancellationToken)
    {
        var yearStart = new DateOnly(issueDate.Year, 1, 1);
        var nextYearStart = yearStart.AddYears(1);
        var invoiceCount = await dbContext.Invoices
            .CountAsync(
                invoice =>
                    invoice.WorkspaceId == workspaceId &&
                    invoice.IssueDate >= yearStart &&
                    invoice.IssueDate < nextYearStart,
                cancellationToken);

        return $"INV-{issueDate.Year}-{invoiceCount + 1:0000}";
    }
}
