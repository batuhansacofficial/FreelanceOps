using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed record TestInvoiceContext(
    Guid InvoiceId,
    string InvoiceNumber,
    decimal TotalAmount);

public static class TestBillingHelper
{
    public static async Task<TestInvoiceContext> CreateInvoiceAsync(
        HttpClient client,
        Guid workspaceId,
        Guid clientId,
        Guid? projectId = null,
        decimal quantity = 10m,
        decimal unitPrice = 50m,
        decimal taxRate = 0m)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/invoices",
            CreateInvoiceRequest(clientId, projectId, quantity, unitPrice, taxRate));

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateInvoiceTestResponse>();

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
        decimal taxRate = 0m)
    {
        return new
        {
            ClientId = clientId,
            ProjectId = projectId,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            Currency = "USD",
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

    private sealed record CreateInvoiceTestResponse(
        Guid Id,
        string InvoiceNumber,
        decimal TotalAmount);
}
