namespace FreelanceOps.Application.Abstractions.Billing;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateAsync(
        Guid workspaceId,
        DateOnly issueDate,
        CancellationToken cancellationToken);
}
