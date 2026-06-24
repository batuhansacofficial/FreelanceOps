using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.Application.BackgroundJobs.ExpiredProposalJob;
using FreelanceOps.Application.BackgroundJobs.OverdueInvoiceNotificationJob;
using FreelanceOps.Domain.Billing;
using FreelanceOps.Domain.Notifications;
using FreelanceOps.Infrastructure.Persistence;
using FreelanceOps.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceOps.IntegrationTests.BackgroundJobs;

public sealed class BackgroundJobTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ExpiredProposalJob_ShouldExpireSentProposal_WhenValidUntilPassed()
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

        await ExecuteExpiredProposalJobAsync();

        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/proposals/{proposal.ProposalId}");
        var detail = await ReadAsAsync<ProposalDetailResponse>(detailResponse);
        detail.Status.Should().Be("Expired");
    }

    [Fact]
    public async Task ExpiredProposalJob_ShouldCreateNotificationForOwnerAndAdmin()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var admin = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var adminClient = CreateAuthenticatedClient(admin);
        var setup = await CreateProposalSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(
            ownerClient,
            setup.WorkspaceId,
            admin.Email,
            role: "Admin");
        var proposal = await TestProposalHelper.CreateProposalAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            validUntil: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
        await TestProposalHelper.SendProposalAsync(ownerClient, setup.WorkspaceId, proposal.ProposalId);

        await ExecuteExpiredProposalJobAsync();

        var ownerNotifications = await GetNotificationsAsync(ownerClient, setup.WorkspaceId);
        var adminNotifications = await GetNotificationsAsync(adminClient, setup.WorkspaceId);

        ownerNotifications.Items.Should()
            .Contain(item => item.Type == nameof(NotificationType.ProposalExpired));
        adminNotifications.Items.Should()
            .Contain(item => item.Type == nameof(NotificationType.ProposalExpired));
    }

    [Fact]
    public async Task ExpiredProposalJob_ShouldNotDuplicateNotifications()
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

        await ExecuteExpiredProposalJobAsync();
        await ExecuteExpiredProposalJobAsync();

        var count = await CountNotificationsAsync(
            setup.WorkspaceId,
            NotificationType.ProposalExpired,
            proposal.ProposalId);

        count.Should().Be(1);
    }

    [Fact]
    public async Task OverdueInvoiceJob_ShouldCreateNotification_WhenSentInvoiceIsPastDue()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateBillingSetupAsync(ownerClient);
        var invoice = await CreateOverdueInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);

        await ExecuteOverdueInvoiceJobAsync();

        var notifications = await GetNotificationsAsync(ownerClient, setup.WorkspaceId);

        notifications.Items.Should()
            .Contain(item =>
                item.Type == nameof(NotificationType.InvoiceOverdue) &&
                item.RelatedEntityId == invoice.InvoiceId);
    }

    [Fact]
    public async Task OverdueInvoiceJob_ShouldNotChangeInvoiceStatus()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateBillingSetupAsync(ownerClient);
        var invoice = await CreateOverdueInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);

        await ExecuteOverdueInvoiceJobAsync();

        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}");
        var detail = await ReadAsAsync<InvoiceDetailResponse>(detailResponse);

        detail.Status.Should().Be(nameof(InvoiceStatus.Sent));
    }

    [Fact]
    public async Task OverdueInvoiceJob_ShouldNotDuplicateNotifications()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateBillingSetupAsync(ownerClient);
        var invoice = await CreateOverdueInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);

        await ExecuteOverdueInvoiceJobAsync();
        await ExecuteOverdueInvoiceJobAsync();

        var count = await CountNotificationsAsync(
            setup.WorkspaceId,
            NotificationType.InvoiceOverdue,
            invoice.InvoiceId);

        count.Should().Be(1);
    }

    private async Task ExecuteExpiredProposalJobAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<IExpiredProposalJob>();

        await job.ExecuteAsync(CancellationToken.None);
    }

    private async Task ExecuteOverdueInvoiceJobAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<IOverdueInvoiceNotificationJob>();

        await job.ExecuteAsync(CancellationToken.None);
    }

    private async Task<int> CountNotificationsAsync(
        Guid workspaceId,
        NotificationType type,
        Guid relatedEntityId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Notifications.CountAsync(notification =>
            notification.WorkspaceId == workspaceId &&
            notification.Type == type &&
            notification.RelatedEntityId == relatedEntityId);
    }

    private static async Task<PagedResult<NotificationListItem>> GetNotificationsAsync(
        HttpClient client,
        Guid workspaceId)
    {
        var response = await client.GetAsync(
            $"/api/workspaces/{workspaceId}/notifications?pageSize=100");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<NotificationListItem>>();

        if (result is null)
        {
            throw new InvalidOperationException("Notification response could not be deserialized.");
        }

        return result;
    }

    private static async Task<TestInvoiceContext> CreateOverdueInvoiceAsync(
        HttpClient client,
        Guid workspaceId,
        Guid clientId,
        Guid projectId)
    {
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10);
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            client,
            workspaceId,
            clientId,
            projectId,
            issueDate: issueDate,
            dueDate: dueDate);

        await TestBillingHelper.SendInvoiceAsync(client, workspaceId, invoice.InvoiceId);

        return invoice;
    }

    private static async Task<ProposalSetup> CreateProposalSetupAsync(HttpClient ownerClient)
    {
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);

        return new ProposalSetup(workspace.WorkspaceId, client.ClientId);
    }

    private static async Task<BillingSetup> CreateBillingSetupAsync(HttpClient ownerClient)
    {
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        var project = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspace.WorkspaceId,
            client.ClientId);

        return new BillingSetup(workspace.WorkspaceId, client.ClientId, project.ProjectId);
    }

    private sealed record ProposalSetup(Guid WorkspaceId, Guid ClientId);

    private sealed record BillingSetup(
        Guid WorkspaceId,
        Guid ClientId,
        Guid ProjectId);

    private sealed record ProposalDetailResponse(string Status);

    private sealed record InvoiceDetailResponse(string Status);

    private sealed record PagedResult<T>(
        IReadOnlyCollection<T> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record NotificationListItem(
        Guid Id,
        string Type,
        Guid? RelatedEntityId);
}
