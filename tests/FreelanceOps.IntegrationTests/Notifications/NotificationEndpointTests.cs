using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.Domain.Notifications;
using FreelanceOps.Infrastructure.Persistence;
using FreelanceOps.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceOps.IntegrationTests.Notifications;

public sealed class NotificationEndpointTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetNotifications_ShouldReturnOnlyCurrentUsersNotifications()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, workspace.WorkspaceId, member.Email);
        var ownerNotificationId = await SeedNotificationAsync(
            workspace.WorkspaceId,
            owner.UserId,
            NotificationType.ProposalAccepted);
        var memberNotificationId = await SeedNotificationAsync(
            workspace.WorkspaceId,
            member.UserId,
            NotificationType.InvoiceSent);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/notifications?pageSize=100", TestContext.Current.CancellationToken);
        var result = await ReadAsAsync<PagedResult<NotificationListItem>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Items.Should().Contain(item => item.Id == ownerNotificationId);
        result.Items.Should().NotContain(item => item.Id == memberNotificationId);
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnForbidden_WhenUserIsNotWorkspaceMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var outsider = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var outsiderClient = CreateAuthenticatedClient(outsider);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);

        var response = await outsiderClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/notifications", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnreadCount_ShouldReturnOnlyUnreadNotifications()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, workspace.WorkspaceId, member.Email);
        await SeedNotificationAsync(workspace.WorkspaceId, owner.UserId, NotificationType.InvoiceSent);
        await SeedNotificationAsync(workspace.WorkspaceId, owner.UserId, NotificationType.InvoicePaid);
        await SeedNotificationAsync(
            workspace.WorkspaceId,
            owner.UserId,
            NotificationType.ProposalAccepted,
            isRead: true);
        await SeedNotificationAsync(workspace.WorkspaceId, member.UserId, NotificationType.InvoiceOverdue);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/notifications/unread-count", TestContext.Current.CancellationToken);
        var result = await ReadAsAsync<UnreadCountResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task MarkAsRead_ShouldMarkNotificationAsRead()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var notificationId = await SeedNotificationAsync(
            workspace.WorkspaceId,
            owner.UserId,
            NotificationType.ProposalConvertedToProject);

        var markResponse = await ownerClient.PatchAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/notifications/{notificationId}/read",
            content: null, cancellationToken: TestContext.Current.CancellationToken);
        var listResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/notifications?pageSize=100", TestContext.Current.CancellationToken);
        var result = await ReadAsAsync<PagedResult<NotificationListItem>>(listResponse);

        markResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result.Items.Should().ContainSingle(item => item.Id == notificationId && item.IsRead);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnNotFound_WhenNotificationBelongsToAnotherUser()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, workspace.WorkspaceId, member.Email);
        var memberNotificationId = await SeedNotificationAsync(
            workspace.WorkspaceId,
            member.UserId,
            NotificationType.InvoiceSent);

        var response = await ownerClient.PatchAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/notifications/{memberNotificationId}/read",
            content: null, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReadAll_ShouldMarkCurrentUsersWorkspaceNotificationsAsRead()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, workspace.WorkspaceId, member.Email);
        await SeedNotificationAsync(workspace.WorkspaceId, owner.UserId, NotificationType.InvoiceSent);
        await SeedNotificationAsync(workspace.WorkspaceId, owner.UserId, NotificationType.InvoicePaid);
        var memberNotificationId = await SeedNotificationAsync(
            workspace.WorkspaceId,
            member.UserId,
            NotificationType.InvoiceOverdue);

        var response = await ownerClient.PatchAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/notifications/read-all",
            content: null, cancellationToken: TestContext.Current.CancellationToken);
        var unreadResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/notifications/unread-count", TestContext.Current.CancellationToken);
        var unread = await ReadAsAsync<UnreadCountResponse>(unreadResponse);
        var memberNotificationIsRead = await IsNotificationReadAsync(memberNotificationId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        unread.Count.Should().Be(0);
        memberNotificationIsRead.Should().BeFalse();
    }

    private async Task<Guid> SeedNotificationAsync(
        Guid workspaceId,
        Guid userId,
        NotificationType type,
        bool isRead = false)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notification = new Notification(
            workspaceId,
            userId,
            type,
            $"{type} title",
            $"{type} message",
            type.ToString(),
            Guid.NewGuid(),
            $"test-notification:{Guid.NewGuid():N}");

        if (isRead)
        {
            notification.MarkAsRead();
        }

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        return notification.Id;
    }

    private async Task<bool> IsNotificationReadAsync(Guid notificationId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notification = await dbContext.Notifications.FindAsync(notificationId);

        return notification?.IsRead ?? false;
    }

    private sealed record PagedResult<T>(
        IReadOnlyCollection<T> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record NotificationListItem(
        Guid Id,
        string Type,
        bool IsRead);

    private sealed record UnreadCountResponse(int Count);
}
