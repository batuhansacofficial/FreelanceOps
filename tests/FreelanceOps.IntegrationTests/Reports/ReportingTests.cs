using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.IntegrationTests.Infrastructure;

namespace FreelanceOps.IntegrationTests.Reports;

public sealed class ReportingTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Dashboard_ShouldReturnForbidden_WhenRequesterIsMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(
            ownerClient,
            workspace.WorkspaceId,
            member.Email);

        var response = await memberClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Dashboard_ShouldReturnZeros_WhenWorkspaceHasNoData()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);
        var dashboard = await ReadAsAsync<DashboardTestResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        dashboard.TotalClients.Should().Be(0);
        dashboard.ActiveProjects.Should().Be(0);
        dashboard.CompletedProjects.Should().Be(0);
        dashboard.TrackedMinutesThisMonth.Should().Be(0);
        dashboard.PaidRevenueThisMonth.Should().Be(0m);
        dashboard.OutstandingInvoiceAmount.Should().Be(0m);
        dashboard.OverdueInvoiceCount.Should().Be(0);
        dashboard.OpenInvoiceCount.Should().Be(0);
    }

    [Fact]
    public async Task Dashboard_ShouldCalculateActiveProjectsAndClients()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var clientA = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        var clientB = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        var activeProject = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspace.WorkspaceId,
            clientA.ClientId);
        var completedProject = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspace.WorkspaceId,
            clientB.ClientId);
        await ChangeProjectStatusAsync(
            ownerClient,
            workspace.WorkspaceId,
            activeProject.ProjectId,
            "Active");
        await ChangeProjectStatusAsync(
            ownerClient,
            workspace.WorkspaceId,
            completedProject.ProjectId,
            "Completed");

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);
        var dashboard = await ReadAsAsync<DashboardTestResponse>(response);

        dashboard.TotalClients.Should().Be(2);
        dashboard.ActiveProjects.Should().Be(1);
        dashboard.CompletedProjects.Should().Be(1);
    }

    [Fact]
    public async Task Dashboard_ShouldCalculatePaidRevenueFromPaymentsOnly()
    {
        var setup = await CreateProjectSetupAsync();
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId,
            quantity: 10m,
            unitPrice: 50m);
        await TestBillingHelper.SendInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            invoice.InvoiceId);
        await TestBillingHelper.RecordPaymentAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            invoice.InvoiceId,
            amount: 200m);

        var response = await setup.OwnerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);
        var dashboard = await ReadAsAsync<DashboardTestResponse>(response);

        dashboard.PaidRevenueThisMonth.Should().Be(200m);
        dashboard.PaidRevenueThisMonth.Should().NotBe(invoice.TotalAmount);
    }

    [Fact]
    public async Task Dashboard_ShouldCalculateOutstandingInvoices()
    {
        var setup = await CreateProjectSetupAsync();
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);
        await TestBillingHelper.SendInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            invoice.InvoiceId);
        await TestBillingHelper.RecordPaymentAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            invoice.InvoiceId,
            amount: 125m);

        var response = await setup.OwnerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);
        var dashboard = await ReadAsAsync<DashboardTestResponse>(response);

        dashboard.OutstandingInvoiceAmount.Should().Be(invoice.TotalAmount - 125m);
        dashboard.OpenInvoiceCount.Should().Be(1);
    }

    [Fact]
    public async Task Dashboard_ShouldCalculateOverdueInvoices()
    {
        var setup = await CreateProjectSetupAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId,
            issueDate: today.AddDays(-10),
            dueDate: today.AddDays(-1));
        await TestBillingHelper.SendInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            invoice.InvoiceId);

        var response = await setup.OwnerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);
        var dashboard = await ReadAsAsync<DashboardTestResponse>(response);

        dashboard.OverdueInvoiceCount.Should().Be(1);
    }

    [Fact]
    public async Task Dashboard_ShouldExcludeActiveTimersFromTrackedTime()
    {
        var setup = await CreateProjectSetupAsync();
        await TestTimeEntryHelper.StartTimerAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.TaskId);

        var response = await setup.OwnerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);
        var dashboard = await ReadAsAsync<DashboardTestResponse>(response);

        dashboard.TrackedMinutesThisMonth.Should().Be(0);
        dashboard.TrackedHoursThisMonth.Should().Be(0d);
    }

    [Fact]
    public async Task RevenueReport_ShouldGroupPaymentsByCurrency()
    {
        var setup = await CreateProjectSetupAsync();
        var usdInvoice = await TestBillingHelper.CreateInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId,
            currency: "USD");
        var eurInvoice = await TestBillingHelper.CreateInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId,
            currency: "EUR");
        await TestBillingHelper.RecordPaymentAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            usdInvoice.InvoiceId,
            amount: 150m);
        await TestBillingHelper.RecordPaymentAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            eurInvoice.InvoiceId,
            amount: 275m);

        var range = CurrentMonthRange();
        var response = await setup.OwnerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/reports/revenue" +
            $"?from={range.From:yyyy-MM-dd}&to={range.To:yyyy-MM-dd}&groupBy=month", TestContext.Current.CancellationToken);
        var report = await ReadAsAsync<RevenueReportTestResponse>(response);

        report.ItemsByCurrency.Should().ContainSingle(item =>
            item.Currency == "USD" && item.TotalPaidRevenue == 150m);
        report.ItemsByCurrency.Should().ContainSingle(item =>
            item.Currency == "EUR" && item.TotalPaidRevenue == 275m);
    }

    [Fact]
    public async Task RevenueReport_ShouldRespectDateRange()
    {
        var setup = await CreateProjectSetupAsync();
        var range = CurrentMonthRange();
        var insideInvoice = await TestBillingHelper.CreateInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);
        var outsideInvoice = await TestBillingHelper.CreateInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);
        await TestBillingHelper.RecordPaymentAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            insideInvoice.InvoiceId,
            amount: 100m,
            paidAt: range.From);
        await TestBillingHelper.RecordPaymentAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            outsideInvoice.InvoiceId,
            amount: 250m,
            paidAt: range.From.AddDays(-1));

        var response = await setup.OwnerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/reports/revenue" +
            $"?from={range.From:yyyy-MM-dd}&to={range.To:yyyy-MM-dd}&groupBy=day", TestContext.Current.CancellationToken);
        var report = await ReadAsAsync<RevenueReportTestResponse>(response);

        report.ItemsByCurrency.Should().ContainSingle();
        report.ItemsByCurrency.Single().TotalPaidRevenue.Should().Be(100m);
        report.ItemsByCurrency.Single().Items.Should().ContainSingle(
            item => item.Period == range.From.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task ClientSummary_ShouldReturnOnlyWorkspaceData()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setupA = await CreateProjectSetupAsync(ownerClient);
        var setupB = await CreateProjectSetupAsync(ownerClient);
        var invoiceA = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setupA.WorkspaceId,
            setupA.ClientId,
            setupA.ProjectId);
        var invoiceB = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setupB.WorkspaceId,
            setupB.ClientId,
            setupB.ProjectId);
        await TestBillingHelper.RecordPaymentAsync(
            ownerClient,
            setupA.WorkspaceId,
            invoiceA.InvoiceId,
            amount: 100m);
        await TestBillingHelper.RecordPaymentAsync(
            ownerClient,
            setupB.WorkspaceId,
            invoiceB.InvoiceId,
            amount: 300m);

        var range = CurrentMonthRange();
        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{setupA.WorkspaceId}/reports/client-summary" +
            $"?from={range.From:yyyy-MM-dd}&to={range.To:yyyy-MM-dd}", TestContext.Current.CancellationToken);
        var report = await ReadAsAsync<ClientSummaryTestResponse>(response);

        report.Items.Should().ContainSingle(item =>
            item.ClientId == setupA.ClientId && item.PaidAmount == 100m);
        report.Items.Should().NotContain(item => item.ClientId == setupB.ClientId);
    }

    [Fact]
    public async Task ProjectPerformance_ShouldCalculateTrackedHoursAndRevenuePerHour()
    {
        var setup = await CreateProjectSetupAsync();
        await TestTimeEntryHelper.CreateManualAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.TaskId,
            durationMinutes: 120);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId,
            quantity: 6m,
            unitPrice: 100m);
        await TestBillingHelper.SendInvoiceAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            invoice.InvoiceId);
        await TestBillingHelper.RecordPaymentAsync(
            setup.OwnerClient,
            setup.WorkspaceId,
            invoice.InvoiceId,
            amount: 300m);

        var range = CurrentMonthRange();
        var response = await setup.OwnerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/reports/project-performance" +
            $"?from={range.From:yyyy-MM-dd}&to={range.To:yyyy-MM-dd}", TestContext.Current.CancellationToken);
        var report = await ReadAsAsync<ProjectPerformanceTestResponse>(response);
        var item = report.Items.Single(project => project.ProjectId == setup.ProjectId);

        item.TrackedMinutes.Should().Be(120);
        item.TrackedHours.Should().Be(2d);
        item.InvoiceTotal.Should().Be(600m);
        item.PaidAmount.Should().Be(300m);
        item.OutstandingAmount.Should().Be(300m);
        item.RevenuePerTrackedHour.Should().Be(150m);
    }

    [Fact]
    public async Task Reports_ShouldReturnForbidden_WhenUserIsNotWorkspaceMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var outsider = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var outsiderClient = CreateAuthenticatedClient(outsider);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var paths = new[]
        {
            "dashboard",
            "revenue",
            "client-summary",
            "project-performance"
        };

        foreach (var path in paths)
        {
            var response = await outsiderClient.GetAsync(
                $"/api/workspaces/{workspace.WorkspaceId}/reports/{path}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(
                HttpStatusCode.Forbidden,
                $"non-members must not access {path}");
        }
    }

    [Fact]
    public async Task Reports_ShouldNotLeakCrossWorkspaceFinancialData()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setupA = await CreateProjectSetupAsync(ownerClient);
        var setupB = await CreateProjectSetupAsync(ownerClient);
        var invoiceB = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setupB.WorkspaceId,
            setupB.ClientId,
            setupB.ProjectId);
        await TestBillingHelper.RecordPaymentAsync(
            ownerClient,
            setupB.WorkspaceId,
            invoiceB.InvoiceId,
            amount: 400m);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{setupA.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);
        var dashboard = await ReadAsAsync<DashboardTestResponse>(response);

        dashboard.PaidRevenueThisMonth.Should().Be(0m);
        dashboard.OutstandingInvoiceAmount.Should().Be(0m);
    }

    [Fact]
    public async Task Dashboard_ShouldReturnOk_WhenRequesterIsAdmin()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var admin = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var adminClient = CreateAuthenticatedClient(admin);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(
            ownerClient,
            workspace.WorkspaceId,
            admin.Email,
            role: "Admin");

        var response = await adminClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/reports/dashboard", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RevenueReport_ShouldReturnBadRequest_WhenDateRangeIsReversed()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/reports/revenue" +
            $"?from={today:yyyy-MM-dd}&to={today.AddDays(-1):yyyy-MM-dd}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RevenueReport_ShouldReturnBadRequest_WhenDefaultedFromIsAfterTo()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var previousMonthEnd = new DateOnly(today.Year, today.Month, 1).AddDays(-1);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/reports/revenue" +
            $"?to={previousMonthEnd:yyyy-MM-dd}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RevenueReport_ShouldReturnBadRequest_WhenDateRangeExceeds366Days()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var from = new DateOnly(2025, 1, 1);
        var to = from.AddDays(366);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/reports/revenue" +
            $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RevenueReport_ShouldReturnBadRequest_WhenGroupByIsInvalid()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/reports/revenue?groupBy=year", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<ReportSetup> CreateProjectSetupAsync()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var ownerClient = CreateAuthenticatedClient(owner);

        return await CreateProjectSetupAsync(ownerClient);
    }

    private static async Task<ReportSetup> CreateProjectSetupAsync(HttpClient ownerClient)
    {
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(
            ownerClient,
            workspace.WorkspaceId);
        var project = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspace.WorkspaceId,
            client.ClientId);
        var task = await TestProjectHelper.CreateTaskAsync(
            ownerClient,
            workspace.WorkspaceId,
            project.ProjectId);

        return new ReportSetup(
            ownerClient,
            workspace.WorkspaceId,
            client.ClientId,
            project.ProjectId,
            task.TaskId);
    }

    private static async Task ChangeProjectStatusAsync(
        HttpClient client,
        Guid workspaceId,
        Guid projectId,
        string status)
    {
        var response = await client.PatchAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects/{projectId}/status",
            new { Status = status }, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private static DateRange CurrentMonthRange()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new DateRange(
            new DateOnly(today.Year, today.Month, 1),
            today);
    }

    private sealed record ReportSetup(
        HttpClient OwnerClient,
        Guid WorkspaceId,
        Guid ClientId,
        Guid ProjectId,
        Guid TaskId);

    private sealed record DateRange(DateOnly From, DateOnly To);

    private sealed record DashboardTestResponse(
        int TotalClients,
        int ActiveProjects,
        int CompletedProjects,
        int TrackedMinutesThisMonth,
        double TrackedHoursThisMonth,
        decimal PaidRevenueThisMonth,
        decimal OutstandingInvoiceAmount,
        int OverdueInvoiceCount,
        int OpenInvoiceCount);

    private sealed record RevenueReportTestResponse(
        IReadOnlyCollection<RevenueCurrencyTestResponse> ItemsByCurrency);

    private sealed record RevenueCurrencyTestResponse(
        string Currency,
        decimal TotalPaidRevenue,
        IReadOnlyCollection<RevenuePeriodTestResponse> Items);

    private sealed record RevenuePeriodTestResponse(
        string Period,
        decimal Amount);

    private sealed record ClientSummaryTestResponse(
        IReadOnlyCollection<ClientSummaryItemTestResponse> Items);

    private sealed record ClientSummaryItemTestResponse(
        Guid ClientId,
        decimal PaidAmount);

    private sealed record ProjectPerformanceTestResponse(
        IReadOnlyCollection<ProjectPerformanceItemTestResponse> Items);

    private sealed record ProjectPerformanceItemTestResponse(
        Guid ProjectId,
        int TrackedMinutes,
        double TrackedHours,
        decimal InvoiceTotal,
        decimal PaidAmount,
        decimal OutstandingAmount,
        decimal RevenuePerTrackedHour);
}
