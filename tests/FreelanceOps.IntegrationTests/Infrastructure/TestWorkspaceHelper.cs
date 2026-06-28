using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed record TestWorkspaceContext(Guid WorkspaceId);

public sealed record TestWorkspaceMemberContext(
    Guid MemberId,
    Guid UserId,
    string Email,
    string Role);

public static class TestWorkspaceHelper
{
    public static async Task<TestWorkspaceContext> CreateWorkspaceAsync(HttpClient client)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.PostAsJsonAsync(
            "/api/workspaces",
            new
            {
                Name = $"Workspace {Guid.NewGuid():N}"
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateWorkspaceTestResponse>(cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("Create workspace response could not be deserialized.");
        }

        return new TestWorkspaceContext(result.Id);
    }

    public static async Task<TestWorkspaceMemberContext> AddMemberAsync(
        HttpClient client,
        Guid workspaceId,
        string email,
        string role = "Member")
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/members",
            new
            {
                Email = email,
                Role = role
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<WorkspaceMemberTestResponse>(cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("Add member response could not be deserialized.");
        }

        return new TestWorkspaceMemberContext(
            result.Id,
            result.UserId,
            result.Email,
            result.Role);
    }

    private sealed record CreateWorkspaceTestResponse(
        Guid Id,
        string Name,
        string Slug,
        string Role,
        DateTime CreatedAtUtc);

    private sealed record WorkspaceMemberTestResponse(
        Guid Id,
        Guid UserId,
        string Email,
        string FullName,
        string Role,
        DateTime JoinedAtUtc);
}
