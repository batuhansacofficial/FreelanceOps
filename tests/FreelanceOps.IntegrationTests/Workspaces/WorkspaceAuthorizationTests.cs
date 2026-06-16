using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.IntegrationTests.Infrastructure;

namespace FreelanceOps.IntegrationTests.Workspaces;

public sealed class WorkspaceAuthorizationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateWorkspace_ShouldCreateOwnerMembership()
    {
        var user = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var userClient = CreateAuthenticatedClient(user);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(userClient);

        var response = await userClient.GetAsync($"/api/workspaces/{workspace.WorkspaceId}/members");
        var members = await ReadAsAsync<IReadOnlyCollection<WorkspaceMemberTestResponse>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        members.Should().ContainSingle(member =>
            member.UserId == user.UserId &&
            member.Role == "Owner");
    }

    [Fact]
    public async Task GetWorkspace_ShouldReturnForbidden_WhenUserIsNotMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var outsider = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var outsiderClient = CreateAuthenticatedClient(outsider);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);

        var response = await outsiderClient.GetAsync($"/api/workspaces/{workspace.WorkspaceId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddMember_ShouldReturnForbidden_WhenRequesterIsMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var target = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, workspace.WorkspaceId, member.Email);

        var response = await memberClient.PostAsJsonAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/members",
            new
            {
                Email = target.Email,
                Role = "Member"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record WorkspaceMemberTestResponse(
        Guid Id,
        Guid UserId,
        string Email,
        string FullName,
        string Role,
        DateTime JoinedAtUtc);
}
