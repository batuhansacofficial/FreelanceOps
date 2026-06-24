using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.Domain.Billing;
using FreelanceOps.Domain.Projects;
using FreelanceOps.Domain.Users;
using FreelanceOps.Domain.Workspaces;
using FreelanceOps.Infrastructure.Persistence;
using FreelanceOps.Infrastructure.Persistence.Seeding;
using FreelanceOps.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceOps.IntegrationTests.Seeding;

public sealed class DemoSeedTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task DemoSeeder_ShouldCreateDemoUser()
    {
        var result = await SeedFreshAsync();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.Users.SingleAsync(
            existingUser => existingUser.Email == User.NormalizeEmail(DemoDataSeeder.DemoEmail));

        result.UserId.Should().Be(user.Id);
        user.Email.Should().Be("demo@freelanceops.dev");
        user.FullName.Should().Be("Demo Freelancer");
        user.IsActive.Should().BeTrue();
        user.PasswordHash.Should().NotBeNullOrWhiteSpace();
        user.PasswordHash.Should().NotBe(DemoDataSeeder.DemoPassword);
    }

    [Fact]
    public async Task DemoSeeder_ShouldCreateDemoWorkspace()
    {
        var result = await SeedFreshAsync();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var workspace = await dbContext.Workspaces.SingleAsync(
            existingWorkspace => existingWorkspace.Slug == DemoDataSeeder.DemoWorkspaceSlug);
        var ownerMember = await dbContext.WorkspaceMembers.SingleAsync(
            member =>
                member.WorkspaceId == workspace.Id &&
                member.UserId == result.UserId);

        result.WorkspaceId.Should().Be(workspace.Id);
        workspace.Name.Should().Be("FreelanceOps Demo Workspace");
        workspace.OwnerUserId.Should().Be(result.UserId);
        ownerMember.Role.Should().Be(WorkspaceRole.Owner);
        ownerMember.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DemoSeeder_ShouldBeIdempotent()
    {
        var first = await SeedFreshAsync();
        var second = await SeedAsync(resetBeforeSeed: false);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        second.Created.Should().BeFalse();
        second.UserId.Should().Be(first.UserId);
        second.WorkspaceId.Should().Be(first.WorkspaceId);
        (await CountDemoUsersAsync(dbContext)).Should().Be(1);
        (await CountDemoWorkspacesAsync(dbContext)).Should().Be(1);
        (await dbContext.Clients.CountAsync(client => client.WorkspaceId == first.WorkspaceId))
            .Should().Be(2);
        (await dbContext.Projects.CountAsync(project => project.WorkspaceId == first.WorkspaceId))
            .Should().Be(3);
        (await dbContext.ProjectTasks.CountAsync(task => task.WorkspaceId == first.WorkspaceId))
            .Should().Be(10);
        (await dbContext.TimeEntries.CountAsync(entry => entry.WorkspaceId == first.WorkspaceId))
            .Should().Be(9);
        (await dbContext.Invoices.CountAsync(invoice => invoice.WorkspaceId == first.WorkspaceId))
            .Should().Be(2);
        (await dbContext.Proposals.CountAsync(proposal => proposal.WorkspaceId == first.WorkspaceId))
            .Should().Be(2);
        (await dbContext.Notifications.CountAsync(notification => notification.WorkspaceId == first.WorkspaceId))
            .Should().Be(3);
        (await dbContext.PaymentRecords.CountAsync())
            .Should().Be(2);
    }

    [Fact]
    public async Task DemoSeeder_ShouldCreateClientsProjectsTasks()
    {
        var result = await SeedFreshAsync();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var clients = await dbContext.Clients
            .Where(client => client.WorkspaceId == result.WorkspaceId)
            .ToListAsync();
        var projects = await dbContext.Projects
            .Where(project => project.WorkspaceId == result.WorkspaceId)
            .ToListAsync();
        var tasks = await dbContext.ProjectTasks
            .Where(task => task.WorkspaceId == result.WorkspaceId)
            .ToListAsync();

        clients.Should().HaveCount(2);
        clients.Select(client => client.Name).Should()
            .BeEquivalentTo("Acme Studio", "Northwind Digital");
        projects.Should().HaveCount(3);
        projects.Should().Contain(project =>
            project.Name == "SaaS Backend API" &&
            project.Status == ProjectStatus.Active);
        projects.Should().Contain(project =>
            project.Name == "Internal Dashboard" &&
            project.Status == ProjectStatus.Completed);
        tasks.Should().HaveCount(10);
        tasks.Should().Contain(task => task.Status == ProjectTaskStatus.InProgress);
        tasks.Should().Contain(task => task.Status == ProjectTaskStatus.Done);
        tasks.Should().OnlyContain(task => task.AssignedToUserId == result.UserId);
    }

    [Fact]
    public async Task DemoSeeder_ShouldCreateInvoicesPaymentsAndReportsData()
    {
        await SeedFreshAsync();
        var demoUser = await LoginDemoUserAsync();
        using var demoClient = CreateAuthenticatedClient(demoUser);
        var workspacesResponse = await demoClient.GetAsync("/api/workspaces");
        var workspaces = await ReadAsAsync<IReadOnlyCollection<WorkspaceListItem>>(workspacesResponse);
        var workspace = workspaces.Single(item => item.Slug == DemoDataSeeder.DemoWorkspaceSlug);

        var dashboardResponse = await demoClient.GetAsync(
            $"/api/workspaces/{workspace.Id}/reports/dashboard");
        var dashboard = await ReadAsAsync<DemoDashboardResponse>(dashboardResponse);
        var revenueResponse = await demoClient.GetAsync(
            $"/api/workspaces/{workspace.Id}/reports/revenue");
        var revenue = await ReadAsAsync<DemoRevenueResponse>(revenueResponse);
        var clientSummaryResponse = await demoClient.GetAsync(
            $"/api/workspaces/{workspace.Id}/reports/client-summary");
        var clientSummary = await ReadAsAsync<DemoClientSummaryResponse>(clientSummaryResponse);
        var projectPerformanceResponse = await demoClient.GetAsync(
            $"/api/workspaces/{workspace.Id}/reports/project-performance");
        var projectPerformance =
            await ReadAsAsync<DemoProjectPerformanceResponse>(projectPerformanceResponse);
        var notificationsResponse = await demoClient.GetAsync(
            $"/api/workspaces/{workspace.Id}/notifications");
        var notifications = await ReadAsAsync<DemoNotificationPageResponse>(notificationsResponse);

        dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        dashboard.TotalClients.Should().Be(2);
        dashboard.ActiveProjects.Should().Be(2);
        dashboard.CompletedProjects.Should().Be(1);
        dashboard.TrackedMinutesThisMonth.Should().Be(3840);
        dashboard.PaidRevenueThisMonth.Should().Be(4210m);
        dashboard.OutstandingInvoiceAmount.Should().Be(2600m);
        dashboard.OverdueInvoiceCount.Should().Be(1);
        dashboard.OpenInvoiceCount.Should().Be(1);

        revenue.ItemsByCurrency.Should().ContainSingle(item =>
            item.Currency == "USD" &&
            item.TotalPaidRevenue == 4210m);
        clientSummary.Items.Should().HaveCount(2);
        clientSummary.Items.Should().Contain(item =>
            item.ClientName == "Acme Studio" &&
            item.PaidAmount == 2500m &&
            item.OutstandingAmount == 2600m &&
            item.TrackedMinutes == 2760);
        clientSummary.Items.Should().Contain(item =>
            item.ClientName == "Northwind Digital" &&
            item.PaidAmount == 1710m &&
            item.OutstandingAmount == 0m &&
            item.TrackedMinutes == 1080);
        projectPerformance.Items.Should().HaveCount(3);
        projectPerformance.Items.Should().Contain(item =>
            item.ProjectName == "SaaS Backend API" &&
            item.TrackedMinutes == 2520 &&
            item.InvoiceTotal == 5100m &&
            item.PaidAmount == 2500m &&
            item.OutstandingAmount == 2600m);
        notifications.Items.Should().HaveCount(3);
    }

    private async Task<DemoSeedResult> SeedFreshAsync()
    {
        return await SeedAsync(resetBeforeSeed: true);
    }

    private async Task<DemoSeedResult> SeedAsync(bool resetBeforeSeed)
    {
        using var scope = Factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDemoDataSeeder>();

        return await seeder.SeedAsync(resetBeforeSeed);
    }

    private async Task<TestUserContext> LoginDemoUserAsync()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = DemoDataSeeder.DemoEmail,
                Password = DemoDataSeeder.DemoPassword
            });

        response.EnsureSuccessStatusCode();

        var loginResult = await response.Content
            .ReadFromJsonAsync<TestAuthHelper.LoginTestResponse>();

        if (loginResult is null)
        {
            throw new InvalidOperationException("Login response could not be deserialized.");
        }

        return new TestUserContext(
            loginResult.User.Id,
            loginResult.User.Email,
            loginResult.AccessToken,
            loginResult.RefreshToken);
    }

    private static async Task<int> CountDemoUsersAsync(ApplicationDbContext dbContext)
    {
        return await dbContext.Users.CountAsync(
            user => user.Email == User.NormalizeEmail(DemoDataSeeder.DemoEmail));
    }

    private static async Task<int> CountDemoWorkspacesAsync(ApplicationDbContext dbContext)
    {
        return await dbContext.Workspaces.CountAsync(
            workspace => workspace.Slug == DemoDataSeeder.DemoWorkspaceSlug);
    }

    private sealed record WorkspaceListItem(
        Guid Id,
        string Name,
        string Slug,
        string Role);

    private sealed record DemoDashboardResponse(
        int TotalClients,
        int ActiveProjects,
        int CompletedProjects,
        int TrackedMinutesThisMonth,
        decimal PaidRevenueThisMonth,
        decimal OutstandingInvoiceAmount,
        int OverdueInvoiceCount,
        int OpenInvoiceCount);

    private sealed record DemoRevenueResponse(
        IReadOnlyCollection<DemoRevenueCurrencyResponse> ItemsByCurrency);

    private sealed record DemoRevenueCurrencyResponse(
        string Currency,
        decimal TotalPaidRevenue);

    private sealed record DemoClientSummaryResponse(
        IReadOnlyCollection<DemoClientSummaryItemResponse> Items);

    private sealed record DemoClientSummaryItemResponse(
        string ClientName,
        decimal PaidAmount,
        decimal OutstandingAmount,
        int TrackedMinutes);

    private sealed record DemoProjectPerformanceResponse(
        IReadOnlyCollection<DemoProjectPerformanceItemResponse> Items);

    private sealed record DemoProjectPerformanceItemResponse(
        string ProjectName,
        int TrackedMinutes,
        decimal InvoiceTotal,
        decimal PaidAmount,
        decimal OutstandingAmount);

    private sealed record DemoNotificationPageResponse(
        IReadOnlyCollection<DemoNotificationItemResponse> Items);

    private sealed record DemoNotificationItemResponse(
        string Title);
}
