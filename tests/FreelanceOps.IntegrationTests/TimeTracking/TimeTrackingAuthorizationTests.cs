using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.IntegrationTests.Infrastructure;

namespace FreelanceOps.IntegrationTests.TimeTracking;

public sealed class TimeTrackingAuthorizationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task StartTimer_ShouldReturnCreated_WhenTaskBelongsToWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateTaskSetupAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/tasks/{setup.TaskId}/time-entries/start",
            new
            {
                Description = "Starting timer."
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task StartTimer_ShouldReturnNotFound_WhenTaskBelongsToAnotherWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateTaskSetupAsync(ownerClient);
        var otherWorkspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{otherWorkspace.WorkspaceId}/tasks/{setup.TaskId}/time-entries/start",
            new
            {
                Description = "Cross-workspace timer."
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartTimer_ShouldReturnConflict_WhenUserAlreadyHasActiveTimer()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setupA = await CreateTaskSetupAsync(ownerClient);
        var setupB = await CreateTaskSetupAsync(ownerClient);
        await TestTimeEntryHelper.StartTimerAsync(ownerClient, setupA.WorkspaceId, setupA.TaskId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setupB.WorkspaceId}/tasks/{setupB.TaskId}/time-entries/start",
            new
            {
                Description = "Second active timer."
            });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task StopTimer_ShouldReturnNoContent_WhenTimerIsRunning()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateTaskSetupAsync(ownerClient);
        var timeEntry = await TestTimeEntryHelper.StartTimerAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.TaskId);

        var response = await ownerClient.PostAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries/{timeEntry.TimeEntryId}/stop",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task StopTimer_ShouldReturnForbidden_WhenMemberStopsAnotherUsersTimer()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateTaskSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);
        var timeEntry = await TestTimeEntryHelper.StartTimerAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.TaskId);

        var response = await memberClient.PostAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries/{timeEntry.TimeEntryId}/stop",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateManualTimeEntry_ShouldReturnCreated_WhenRequestIsValid()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateTaskSetupAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/tasks/{setup.TaskId}/time-entries/manual",
            ManualEntryRequest(90));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateManualTimeEntry_ShouldReturnBadRequest_WhenDurationIsInvalid()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateTaskSetupAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/tasks/{setup.TaskId}/time-entries/manual",
            ManualEntryRequest(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTimeEntries_ShouldReturnOnlyCurrentUsersEntries_WhenRequesterIsMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateTaskSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);
        await TestTimeEntryHelper.CreateManualAsync(ownerClient, setup.WorkspaceId, setup.TaskId, 30);
        await TestTimeEntryHelper.CreateManualAsync(memberClient, setup.WorkspaceId, setup.TaskId, 45);

        var response = await memberClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries?userId={owner.UserId}");
        var result = await ReadAsAsync<PagedResult<TimeEntryListItem>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(entry => entry.UserId == member.UserId);
    }

    [Fact]
    public async Task GetTimeEntries_ShouldReturnAllWorkspaceEntries_WhenRequesterIsAdmin()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var admin = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var adminClient = CreateAuthenticatedClient(admin);
        var setup = await CreateTaskSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(
            ownerClient,
            setup.WorkspaceId,
            admin.Email,
            role: "Admin");
        await TestTimeEntryHelper.CreateManualAsync(ownerClient, setup.WorkspaceId, setup.TaskId, 30);
        await TestTimeEntryHelper.CreateManualAsync(adminClient, setup.WorkspaceId, setup.TaskId, 45);

        var response = await adminClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries");
        var result = await ReadAsAsync<PagedResult<TimeEntryListItem>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Items.Should().Contain(entry => entry.UserId == owner.UserId);
        result.Items.Should().Contain(entry => entry.UserId == admin.UserId);
    }

    [Fact]
    public async Task GetTimeSummary_ShouldReturnWorkspaceTotals()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateTaskSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);
        await TestTimeEntryHelper.CreateManualAsync(ownerClient, setup.WorkspaceId, setup.TaskId, 60);
        await TestTimeEntryHelper.CreateManualAsync(memberClient, setup.WorkspaceId, setup.TaskId, 90);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/reports/time-summary");
        var summary = await ReadAsAsync<TimeSummaryTestResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        summary.TotalMinutes.Should().Be(150);
        summary.TotalHours.Should().Be(2.5);
        summary.EntriesCount.Should().Be(2);
        summary.ByProject.Should().ContainSingle(project =>
            project.ProjectId == setup.ProjectId &&
            project.TotalMinutes == 150);
        summary.ByUser.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteTimeEntry_ShouldSoftDeleteEntry()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateTaskSetupAsync(ownerClient);
        var timeEntry = await TestTimeEntryHelper.CreateManualAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.TaskId,
            30);

        var deleteResponse = await ownerClient.DeleteAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries/{timeEntry.TimeEntryId}");
        var listResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries");
        var result = await ReadAsAsync<PagedResult<TimeEntryListItem>>(listResponse);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.Items.Should().NotContain(entry => entry.Id == timeEntry.TimeEntryId);
    }

    [Fact]
    public async Task UpdateTimeEntry_ShouldReturnNoContent_WhenUpdatingOwnManualEntry()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateTaskSetupAsync(ownerClient);
        var timeEntry = await TestTimeEntryHelper.CreateManualAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.TaskId,
            30);

        var updateResponse = await ownerClient.PutAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries/{timeEntry.TimeEntryId}",
            new
            {
                StartedAtUtc = DateTime.UtcNow.AddHours(-4),
                DurationMinutes = 120,
                Description = "Updated manual entry."
            });
        var listResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries");
        var result = await ReadAsAsync<PagedResult<TimeEntryListItem>>(listResponse);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.Items.Should().ContainSingle(entry =>
            entry.Id == timeEntry.TimeEntryId &&
            entry.DurationMinutes == 120);
    }

    [Fact]
    public async Task UpdateTimeEntry_ShouldReturnForbidden_WhenMemberUpdatesAnotherUsersEntry()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateTaskSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);
        var timeEntry = await TestTimeEntryHelper.CreateManualAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.TaskId,
            30);

        var response = await memberClient.PutAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/time-entries/{timeEntry.TimeEntryId}",
            new
            {
                StartedAtUtc = DateTime.UtcNow.AddHours(-4),
                DurationMinutes = 120,
                Description = "Unauthorized update."
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StartTimer_ShouldReturnNotFound_WhenTaskIsDeleted()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateTaskSetupAsync(ownerClient);
        var deleteResponse = await ownerClient.DeleteAsync(
            $"/api/workspaces/{setup.WorkspaceId}/tasks/{setup.TaskId}");

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/tasks/{setup.TaskId}/time-entries/start",
            new
            {
                Description = "Deleted task timer."
            });

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static object ManualEntryRequest(int durationMinutes)
    {
        return new
        {
            StartedAtUtc = DateTime.UtcNow.AddHours(-2),
            DurationMinutes = durationMinutes,
            Description = "Manual entry."
        };
    }

    private static async Task<TimeTrackingSetup> CreateTaskSetupAsync(HttpClient ownerClient)
    {
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        var project = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspace.WorkspaceId,
            client.ClientId);
        var task = await TestProjectHelper.CreateTaskAsync(
            ownerClient,
            workspace.WorkspaceId,
            project.ProjectId);

        return new TimeTrackingSetup(
            workspace.WorkspaceId,
            project.ProjectId,
            task.TaskId);
    }

    private sealed record TimeTrackingSetup(
        Guid WorkspaceId,
        Guid ProjectId,
        Guid TaskId);

    private sealed record PagedResult<T>(
        IReadOnlyCollection<T> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record TimeEntryListItem(
        Guid Id,
        Guid UserId,
        int? DurationMinutes);

    private sealed record TimeSummaryTestResponse(
        int TotalMinutes,
        double TotalHours,
        int EntriesCount,
        IReadOnlyCollection<ProjectSummaryTestResponse> ByProject,
        IReadOnlyCollection<UserSummaryTestResponse> ByUser);

    private sealed record ProjectSummaryTestResponse(
        Guid ProjectId,
        int TotalMinutes);

    private sealed record UserSummaryTestResponse(
        Guid UserId,
        int TotalMinutes);
}
