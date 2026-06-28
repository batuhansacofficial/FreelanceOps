using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed record TestClientContext(Guid ClientId);

public static class TestClientHelper
{
    public static async Task<TestClientContext> CreateClientAsync(
        HttpClient client,
        Guid workspaceId)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/clients",
            new
            {
                Name = $"Client {Guid.NewGuid():N}",
                Email = $"client-{Guid.NewGuid():N}@example.com",
                CompanyName = "Test Company",
                Notes = "Integration test client."
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateClientTestResponse>(cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("Create client response could not be deserialized.");
        }

        return new TestClientContext(result.Id);
    }

    private sealed record CreateClientTestResponse(
        Guid Id,
        Guid WorkspaceId,
        string Name,
        string? Email,
        string? CompanyName,
        string? Notes,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
