using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.IntegrationTests.Infrastructure;

namespace FreelanceOps.IntegrationTests.Projects;

public sealed class ProjectAuthorizationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateProject_ShouldReturnCreated_WhenClientBelongsToWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/projects",
            CreateProjectRequest(client.ClientId), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateProject_ShouldReturnNotFound_WhenClientBelongsToAnotherWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspaceA = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var workspaceB = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var clientB = await TestClientHelper.CreateClientAsync(ownerClient, workspaceB.WorkspaceId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspaceA.WorkspaceId}/projects",
            CreateProjectRequest(clientB.ClientId), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProject_ShouldReturnForbidden_WhenRequesterIsMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, workspace.WorkspaceId, member.Email);

        var response = await memberClient.PostAsJsonAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/projects",
            CreateProjectRequest(client.ClientId), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProjectById_ShouldReturnNotFound_WhenProjectBelongsToAnotherWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspaceA = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var workspaceB = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var clientA = await TestClientHelper.CreateClientAsync(ownerClient, workspaceA.WorkspaceId);
        var projectA = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspaceA.WorkspaceId,
            clientA.ClientId);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspaceB.WorkspaceId}/projects/{projectA.ProjectId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static object CreateProjectRequest(Guid clientId)
    {
        return new
        {
            ClientId = clientId,
            Name = $"Project {Guid.NewGuid():N}",
            Description = "Integration test project.",
            StartDate = "2026-06-16",
            Deadline = "2026-07-16"
        };
    }
}
