using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed record TestInvoiceContext(
    Guid InvoiceId,
    string InvoiceNumber,
    decimal TotalAmount);

public sealed record TestPaymentContext(Guid PaymentId);

public static class TestBillingHelper
{
    public static async Task<TestInvoiceContext> CreateInvoiceAsync(
        HttpClient client,
        Guid workspaceId,
        Guid clientId,
        Guid? projectId = null,
        decimal quantity = 10m,
        decimal unitPrice = 50m,
        decimal taxRate = 0m,
        DateOnly? issueDate = null,
        DateOnly? dueDate = null,
        string currency = "USD")
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/invoices",
            CreateInvoiceRequest(
                clientId,
                projectId,
                quantity,
                unitPrice,
                taxRate,
                issueDate,
                dueDate,
                currency),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateInvoiceTestResponse>(cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("Create invoice response could not be deserialized.");
        }

        return new TestInvoiceContext(
            result.Id,
            result.InvoiceNumber,
            result.TotalAmount);
    }

    public static object CreateInvoiceRequest(
        Guid clientId,
        Guid? projectId = null,
        decimal quantity = 10m,
        decimal unitPrice = 50m,
        decimal taxRate = 0m,
        DateOnly? issueDate = null,
        DateOnly? dueDate = null,
        string currency = "USD")
    {
        var resolvedIssueDate = issueDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return new
        {
            ClientId = clientId,
            ProjectId = projectId,
            IssueDate = resolvedIssueDate,
            DueDate = dueDate ?? resolvedIssueDate.AddDays(14),
            Currency = currency,
            Notes = "Integration test invoice.",
            Items = new[]
            {
                new
                {
                    Description = "Development",
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TaxRate = taxRate
                }
            }
        };
    }

    public static async Task SendInvoiceAsync(
        HttpClient client,
        Guid workspaceId,
        Guid invoiceId)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.PatchAsync(
            $"/api/workspaces/{workspaceId}/invoices/{invoiceId}/send",
            content: null,
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public static async Task<TestPaymentContext> RecordPaymentAsync(
        HttpClient client,
        Guid workspaceId,
        Guid invoiceId,
        decimal amount,
        DateOnly? paidAt = null)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/invoices/{invoiceId}/payments",
            new
            {
                Amount = amount,
                Method = "BankTransfer",
                Reference = $"TRX-{Guid.NewGuid():N}",
                PaidAt = paidAt ?? DateOnly.FromDateTime(DateTime.UtcNow)
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PaymentTestResponse>(cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("Record payment response could not be deserialized.");
        }

        return new TestPaymentContext(result.Id);
    }

    private sealed record CreateInvoiceTestResponse(
        Guid Id,
        string InvoiceNumber,
        decimal TotalAmount);

    private sealed record PaymentTestResponse(Guid Id);
}
