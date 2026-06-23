using System.Net.Http.Json;

namespace FreelanceOps.IntegrationTests.Infrastructure;

public sealed record TestTimeEntryContext(Guid TimeEntryId);

public static class TestTimeEntryHelper
{
    public static async Task<TestTimeEntryContext> StartTimerAsync(
        HttpClient client,
        Guid workspaceId,
        Guid taskId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/tasks/{taskId}/time-entries/start",
            new
            {
                Description = "Integration test timer."
            });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TimeEntryTestResponse>();

        if (result is null)
        {
            throw new InvalidOperationException("Start timer response could not be deserialized.");
        }

        return new TestTimeEntryContext(result.Id);
    }

    public static async Task<TestTimeEntryContext> CreateManualAsync(
        HttpClient client,
        Guid workspaceId,
        Guid taskId,
        int durationMinutes = 60,
        DateTime? startedAtUtc = null)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/tasks/{taskId}/time-entries/manual",
            new
            {
                StartedAtUtc = startedAtUtc ?? DateTime.UtcNow.AddHours(-2),
                DurationMinutes = durationMinutes,
                Description = "Integration test manual entry."
            });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TimeEntryTestResponse>();

        if (result is null)
        {
            throw new InvalidOperationException("Manual time entry response could not be deserialized.");
        }

        return new TestTimeEntryContext(result.Id);
    }

    private sealed record TimeEntryTestResponse(Guid Id);
}
