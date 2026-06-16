using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed record TestProjectContext(Guid ProjectId);

public sealed record TestTaskContext(Guid TaskId);

public static class TestProjectHelper
{
    public static async Task<TestProjectContext> CreateProjectAsync(
        HttpClient client,
        Guid workspaceId,
        Guid clientId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects",
            new
            {
                ClientId = clientId,
                Name = $"Project {Guid.NewGuid():N}",
                Description = "Integration test project.",
                StartDate = "2026-06-16",
                Deadline = "2026-07-16"
            });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateProjectTestResponse>();

        if (result is null)
        {
            throw new InvalidOperationException("Create project response could not be deserialized.");
        }

        return new TestProjectContext(result.Id);
    }

    public static async Task<TestTaskContext> CreateTaskAsync(
        HttpClient client,
        Guid workspaceId,
        Guid projectId,
        Guid? assignedToUserId = null)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects/{projectId}/tasks",
            new
            {
                Title = $"Task {Guid.NewGuid():N}",
                Description = "Integration test task.",
                Priority = "High",
                DueDate = "2026-06-30",
                AssignedToUserId = assignedToUserId
            });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateTaskTestResponse>();

        if (result is null)
        {
            throw new InvalidOperationException("Create task response could not be deserialized.");
        }

        return new TestTaskContext(result.Id);
    }

    private sealed record CreateProjectTestResponse(
        Guid Id,
        Guid WorkspaceId,
        Guid ClientId,
        string Name);

    private sealed record CreateTaskTestResponse(
        Guid Id,
        Guid WorkspaceId,
        Guid ProjectId,
        string Title);
}
