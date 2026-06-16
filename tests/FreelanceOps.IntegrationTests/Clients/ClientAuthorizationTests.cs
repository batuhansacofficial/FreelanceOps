using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.IntegrationTests.Infrastructure;

namespace FreelanceOps.IntegrationTests.Clients;

public sealed class ClientAuthorizationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateClient_ShouldReturnCreated_WhenRequesterIsOwner()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/clients",
            CreateClientRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateClient_ShouldReturnForbidden_WhenRequesterIsMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, workspace.WorkspaceId, member.Email);

        var response = await memberClient.PostAsJsonAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/clients",
            CreateClientRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetClientById_ShouldReturnNotFound_WhenClientBelongsToAnotherWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspaceA = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var workspaceB = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspaceA.WorkspaceId);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspaceB.WorkspaceId}/clients/{client.ClientId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetClients_ShouldNotReturnDeletedClients()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);

        var deleteResponse = await ownerClient.DeleteAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/clients/{client.ClientId}");
        var listResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/clients");
        var result = await ReadAsAsync<PagedResult<ClientSummaryResponse>>(listResponse);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Items.Should().NotContain(item => item.Id == client.ClientId);
    }

    private static object CreateClientRequest()
    {
        return new
        {
            Name = $"Client {Guid.NewGuid():N}",
            Email = $"client-{Guid.NewGuid():N}@example.com",
            CompanyName = "Test Company",
            Notes = "Integration test client."
        };
    }

    private sealed record PagedResult<T>(
        IReadOnlyCollection<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages,
        bool HasNextPage,
        bool HasPreviousPage);

    private sealed record ClientSummaryResponse(
        Guid Id,
        Guid WorkspaceId,
        string Name);
}
