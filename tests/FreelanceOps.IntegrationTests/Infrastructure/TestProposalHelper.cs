using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed record TestProposalContext(
    Guid ProposalId,
    string ProposalNumber,
    decimal TotalAmount);

public static class TestProposalHelper
{
    public static async Task<TestProposalContext> CreateProposalAsync(
        HttpClient client,
        Guid workspaceId,
        Guid clientId,
        string? title = null,
        DateOnly? validUntil = null,
        decimal quantity = 10m,
        decimal unitPrice = 50m,
        decimal taxRate = 0m,
        string currency = "USD")
    {
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/proposals",
            CreateProposalRequest(
                clientId,
                title,
                validUntil,
                quantity,
                unitPrice,
                taxRate,
                currency));

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateProposalTestResponse>();

        if (result is null)
        {
            throw new InvalidOperationException("Create proposal response could not be deserialized.");
        }

        return new TestProposalContext(
            result.Id,
            result.ProposalNumber,
            result.TotalAmount);
    }

    public static object CreateProposalRequest(
        Guid clientId,
        string? title = null,
        DateOnly? validUntil = null,
        decimal quantity = 10m,
        decimal unitPrice = 50m,
        decimal taxRate = 0m,
        string currency = "USD")
    {
        return new
        {
            ClientId = clientId,
            Title = title ?? $"Proposal {Guid.NewGuid():N}",
            Scope = "Integration test proposal scope.",
            ValidUntil = validUntil ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            Currency = currency,
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

    public static object UpdateProposalRequest(
        DateOnly? validUntil = null,
        decimal quantity = 3m,
        decimal unitPrice = 80m,
        decimal taxRate = 10m,
        string currency = "EUR")
    {
        return new
        {
            Title = $"Updated Proposal {Guid.NewGuid():N}",
            Scope = "Updated integration test proposal scope.",
            ValidUntil = validUntil ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30),
            Currency = currency,
            Items = new[]
            {
                new
                {
                    Description = "Updated development",
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TaxRate = taxRate
                }
            }
        };
    }

    public static async Task SendProposalAsync(
        HttpClient client,
        Guid workspaceId,
        Guid proposalId)
    {
        var response = await client.PatchAsync(
            $"/api/workspaces/{workspaceId}/proposals/{proposalId}/send",
            content: null);

        response.EnsureSuccessStatusCode();
    }

    public static async Task AcceptProposalAsync(
        HttpClient client,
        Guid workspaceId,
        Guid proposalId)
    {
        var response = await client.PatchAsync(
            $"/api/workspaces/{workspaceId}/proposals/{proposalId}/accept",
            content: null);

        response.EnsureSuccessStatusCode();
    }

    private sealed record CreateProposalTestResponse(
        Guid Id,
        string ProposalNumber,
        decimal TotalAmount);
}
