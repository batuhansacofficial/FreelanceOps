using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.IntegrationTests.Infrastructure;

namespace FreelanceOps.IntegrationTests.Tasks;

public sealed class TaskAuthorizationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateTask_ShouldReturnCreated_WhenAssigneeIsWorkspaceMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient, owner);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/projects/{setup.ProjectId}/tasks",
            CreateTaskRequest(member.UserId), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTask_ShouldReturnNotFound_WhenAssigneeIsNotWorkspaceMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var outsider = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient, owner);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/projects/{setup.ProjectId}/tasks",
            CreateTaskRequest(outsider.UserId), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTask_ShouldReturnNotFound_WhenProjectBelongsToAnotherWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient, owner);
        var workspaceB = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspaceB.WorkspaceId}/projects/{setup.ProjectId}/tasks",
            CreateTaskRequest(null), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Member_ShouldBeAbleToChangeTaskStatus()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateProjectSetupAsync(ownerClient, owner);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);
        var task = await TestProjectHelper.CreateTaskAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ProjectId,
            member.UserId);

        var response = await memberClient.PatchAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/tasks/{task.TaskId}/status",
            new
            {
                Status = "InProgress"
            }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Member_ShouldNotBeAbleToChangeProjectStatus()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateProjectSetupAsync(ownerClient, owner);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);

        var response = await memberClient.PatchAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/projects/{setup.ProjectId}/status",
            new
            {
                Status = "Active"
            }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static object CreateTaskRequest(Guid? assignedToUserId)
    {
        return new
        {
            Title = $"Task {Guid.NewGuid():N}",
            Description = "Integration test task.",
            Priority = "High",
            DueDate = "2026-06-30",
            AssignedToUserId = assignedToUserId
        };
    }

    private static async Task<ProjectSetup> CreateProjectSetupAsync(
        HttpClient ownerClient,
        TestUserContext owner)
    {
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        var project = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspace.WorkspaceId,
            client.ClientId);

        return new ProjectSetup(
            workspace.WorkspaceId,
            client.ClientId,
            project.ProjectId,
            owner.UserId);
    }

    private sealed record ProjectSetup(
        Guid WorkspaceId,
        Guid ClientId,
        Guid ProjectId,
        Guid OwnerUserId);
}
