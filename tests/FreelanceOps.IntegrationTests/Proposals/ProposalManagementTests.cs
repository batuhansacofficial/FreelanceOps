using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.Domain.Proposals;
using FreelanceOps.Infrastructure.Persistence;
using FreelanceOps.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceOps.IntegrationTests.Proposals;

public sealed class ProposalManagementTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateProposal_ShouldReturnCreated_WhenRequesterIsOwner()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals",
            TestProposalHelper.CreateProposalRequest(setup.ClientId), TestContext.Current.CancellationToken);
        var proposal = await ReadAsAsync<ProposalTestResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        proposal.ProposalNumber.Should().MatchRegex(@"^PROP-\d{4}-0001$");
        proposal.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task CreateProposal_ShouldReturnForbidden_WhenRequesterIsMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateProposalSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);

        var response = await memberClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals",
            TestProposalHelper.CreateProposalRequest(setup.ClientId), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateProposal_ShouldReturnNotFound_WhenClientBelongsToAnotherWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspaceA = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var workspaceB = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var clientB = await TestClientHelper.CreateClientAsync(ownerClient, workspaceB.WorkspaceId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspaceA.WorkspaceId}/proposals",
            TestProposalHelper.CreateProposalRequest(clientB.ClientId), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProposal_ShouldCalculateTotalsFromItems()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals",
            TestProposalHelper.CreateProposalRequest(
                setup.ClientId,
                quantity: 2m,
                unitPrice: 100m,
                taxRate: 20m), TestContext.Current.CancellationToken);
        var proposal = await ReadAsAsync<ProposalTestResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        proposal.SubtotalAmount.Should().Be(200m);
        proposal.TaxAmount.Should().Be(40m);
        proposal.TotalAmount.Should().Be(240m);
    }

    [Fact]
    public async Task GetProposals_ShouldReturnOnlyWorkspaceProposals()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setupA = await CreateProposalSetupAsync(ownerClient);
        var setupB = await CreateProposalSetupAsync(ownerClient);
        var proposalA = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setupA.WorkspaceId,
            setupA.ClientId);
        var proposalB = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setupB.WorkspaceId,
            setupB.ClientId);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{setupA.WorkspaceId}/proposals?pageSize=100", TestContext.Current.CancellationToken);
        var result = await ReadAsAsync<PagedResult<ProposalListItem>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Items.Should().Contain(item => item.Id == proposalA.ProposalId);
        result.Items.Should().NotContain(item => item.Id == proposalB.ProposalId);
    }

    [Fact]
    public async Task UpdateProposal_ShouldReturnBadRequest_WhenProposalIsSent()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId);
        await TestProposalHelper.SendProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);

        var response = await ownerClient.PutAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}",
            TestProposalHelper.UpdateProposalRequest(), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendProposal_ShouldChangeStatusToSent_WhenProposalIsDraft()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId);

        var sendResponse = await ownerClient.PatchAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}/send",
            content: null, cancellationToken: TestContext.Current.CancellationToken);
        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}", TestContext.Current.CancellationToken);
        var detail = await ReadAsAsync<ProposalTestResponse>(detailResponse);

        sendResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detail.Status.Should().Be("Sent");
    }

    [Fact]
    public async Task SendProposal_ShouldReturnBadRequest_WhenProposalHasNoItems()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposalId = await SeedDraftProposalWithoutItemsAsync(
            setup.WorkspaceId,
            setup.ClientId);

        var response = await ownerClient.PatchAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposalId}/send",
            content: null, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AcceptProposal_ShouldChangeStatusToAccepted_WhenProposalIsSent()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId);
        await TestProposalHelper.SendProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);

        var acceptResponse = await ownerClient.PatchAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}/accept",
            content: null, cancellationToken: TestContext.Current.CancellationToken);
        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}", TestContext.Current.CancellationToken);
        var detail = await ReadAsAsync<ProposalTestResponse>(detailResponse);

        acceptResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detail.Status.Should().Be("Accepted");
    }

    [Fact]
    public async Task AcceptProposal_ShouldReturnBadRequest_WhenProposalIsExpired()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            validUntil: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
        await TestProposalHelper.SendProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);

        var response = await ownerClient.PatchAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}/accept",
            content: null, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RejectProposal_ShouldChangeStatusToRejected_WhenProposalIsSent()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId);
        await TestProposalHelper.SendProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);

        var rejectResponse = await ownerClient.PatchAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}/reject",
            content: null, cancellationToken: TestContext.Current.CancellationToken);
        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}", TestContext.Current.CancellationToken);
        var detail = await ReadAsAsync<ProposalTestResponse>(detailResponse);

        rejectResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detail.Status.Should().Be("Rejected");
    }

    [Fact]
    public async Task ConvertToProject_ShouldCreateProject_WhenProposalIsAccepted()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            title: "Backend API Development");
        await TestProposalHelper.SendProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);
        await TestProposalHelper.AcceptProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}/convert-to-project",
            ConvertRequest(), TestContext.Current.CancellationToken);
        var project = await ReadAsAsync<ConvertToProjectResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        project.ProposalId.Should().Be(proposal.ProposalId);
        project.Name.Should().Be("Backend API Development");
        project.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task ConvertToProject_ShouldReturnBadRequest_WhenProposalIsNotAccepted()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}/convert-to-project",
            ConvertRequest(), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConvertToProject_ShouldReturnBadRequest_WhenProposalAlreadyConverted()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId);
        await TestProposalHelper.SendProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);
        await TestProposalHelper.AcceptProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);
        var firstResponse = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}/convert-to-project",
            ConvertRequest(), TestContext.Current.CancellationToken);

        var secondResponse = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}/convert-to-project",
            ConvertRequest(), TestContext.Current.CancellationToken);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteProposal_ShouldSoftDeleteDraftProposal()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProposalSetupAsync(ownerClient);
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId);

        var deleteResponse = await ownerClient.DeleteAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}", TestContext.Current.CancellationToken);
        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}", TestContext.Current.CancellationToken);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedDraftProposalWithoutItemsAsync(
        Guid workspaceId,
        Guid clientId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var proposal = new Proposal(
            workspaceId,
            clientId,
            $"PROP-SEED-{Guid.NewGuid():N}"[..32],
            "Empty proposal",
            "No items for send validation.",
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            "USD");

        dbContext.Proposals.Add(proposal);
        await dbContext.SaveChangesAsync();

        return proposal.Id;
    }

    private static object ConvertRequest()
    {
        return new
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            Deadline = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)
        };
    }

    private static async Task<ProposalSetup> CreateProposalSetupAsync(HttpClient ownerClient)
    {
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);

        return new ProposalSetup(
            workspace.WorkspaceId,
            client.ClientId);
    }

    private sealed record ProposalSetup(
        Guid WorkspaceId,
        Guid ClientId);

    private sealed record ProposalTestResponse(
        Guid Id,
        string ProposalNumber,
        string Status,
        decimal SubtotalAmount,
        decimal TaxAmount,
        decimal TotalAmount);

    private sealed record PagedResult<T>(
        IReadOnlyCollection<T> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record ProposalListItem(Guid Id);

    private sealed record ConvertToProjectResponse(
        Guid ProjectId,
        Guid ProposalId,
        string Name,
        string Status);
}
