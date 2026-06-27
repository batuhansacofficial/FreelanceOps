using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FreelanceOps.IntegrationTests.Infrastructure;

namespace FreelanceOps.IntegrationTests.Billing;

public sealed class BillingAuthorizationTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateInvoice_ShouldReturnCreated_WhenRequesterIsOwner()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices",
            TestBillingHelper.CreateInvoiceRequest(setup.ClientId, setup.ProjectId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateInvoice_ShouldReturnForbidden_WhenRequesterIsMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateProjectSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);

        var response = await memberClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices",
            TestBillingHelper.CreateInvoiceRequest(setup.ClientId, setup.ProjectId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateInvoice_ShouldReturnNotFound_WhenClientBelongsToAnotherWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspaceA = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var workspaceB = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var clientB = await TestClientHelper.CreateClientAsync(ownerClient, workspaceB.WorkspaceId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspaceA.WorkspaceId}/invoices",
            TestBillingHelper.CreateInvoiceRequest(clientB.ClientId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateInvoice_ShouldReturnNotFound_WhenProjectBelongsToAnotherWorkspace()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setupA = await CreateProjectSetupAsync(ownerClient);
        var setupB = await CreateProjectSetupAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setupA.WorkspaceId}/invoices",
            TestBillingHelper.CreateInvoiceRequest(setupA.ClientId, setupB.ProjectId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateInvoice_ShouldReturnBadRequest_WhenProjectClientDoesNotMatchInvoiceClient()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var clientA = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        var clientB = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        var projectA = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspace.WorkspaceId,
            clientA.ClientId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspace.WorkspaceId}/invoices",
            TestBillingHelper.CreateInvoiceRequest(clientB.ClientId, projectA.ProjectId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateInvoice_ShouldCalculateTotalsFromItems()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices",
            TestBillingHelper.CreateInvoiceRequest(
                setup.ClientId,
                setup.ProjectId,
                quantity: 2m,
                unitPrice: 100m,
                taxRate: 20m));
        var invoice = await ReadAsAsync<InvoiceTestResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        invoice.SubtotalAmount.Should().Be(200m);
        invoice.TaxAmount.Should().Be(40m);
        invoice.TotalAmount.Should().Be(240m);
        invoice.BalanceDue.Should().Be(240m);
    }

    [Fact]
    public async Task CreateInvoice_ShouldGenerateSequentialWorkspaceYearNumbers()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);

        var first = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);
        var second = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);

        first.InvoiceNumber.Should().MatchRegex(@"^INV-\d{4}-0001$");
        second.InvoiceNumber.Should().MatchRegex(@"^INV-\d{4}-0002$");
    }

    [Fact]
    public async Task SendInvoice_ShouldChangeStatusToSent_WhenInvoiceIsDraft()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);

        var sendResponse = await ownerClient.PatchAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}/send",
            content: null);
        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}");
        var detail = await ReadAsAsync<InvoiceTestResponse>(detailResponse);

        sendResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detail.Status.Should().Be("Sent");
    }

    [Fact]
    public async Task RecordPayment_ShouldMarkInvoiceAsPaid_WhenPaymentCoversBalance()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);
        await ownerClient.PatchAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}/send",
            content: null);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}/payments",
            PaymentRequest(invoice.TotalAmount));
        var payment = await ReadAsAsync<PaymentTestResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        payment.InvoiceStatus.Should().Be("Paid");
        payment.InvoiceBalanceDue.Should().Be(0m);
        payment.InvoicePaidAmount.Should().Be(invoice.TotalAmount);
    }

    [Fact]
    public async Task RecordPayment_ShouldReturnBadRequest_WhenAmountExceedsBalance()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);

        var response = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}/payments",
            PaymentRequest(invoice.TotalAmount + 1m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelInvoice_ShouldReturnBadRequest_WhenInvoiceIsPaid()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);
        await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}/payments",
            PaymentRequest(invoice.TotalAmount));

        var response = await ownerClient.PatchAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}/cancel",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteInvoice_ShouldSoftDeleteDraftInvoice()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);

        var deleteResponse = await ownerClient.DeleteAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}");
        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetInvoices_ShouldReturnOnlyWorkspaceInvoices()
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

        var response = await ownerClient.GetAsync(
            $"/api/workspaces/{setupA.WorkspaceId}/invoices?pageSize=100");
        var result = await ReadAsAsync<PagedResult<InvoiceListItem>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Items.Should().Contain(item => item.Id == invoiceA.InvoiceId);
        result.Items.Should().NotContain(item => item.Id == invoiceB.InvoiceId);
    }

    [Fact]
    public async Task GetInvoices_ShouldReturnForbidden_WhenRequesterIsMember()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var member = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var memberClient = CreateAuthenticatedClient(member);
        var setup = await CreateProjectSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(ownerClient, setup.WorkspaceId, member.Email);

        var response = await memberClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_ShouldCreateInvoiceAndListPaymentRecords()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        var admin = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        using var adminClient = CreateAuthenticatedClient(admin);
        var setup = await CreateProjectSetupAsync(ownerClient);
        await TestWorkspaceHelper.AddMemberAsync(
            ownerClient,
            setup.WorkspaceId,
            admin.Email,
            role: "Admin");
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            adminClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);
        var paymentResponse = await adminClient.PostAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}/payments",
            PaymentRequest(100m));

        var listResponse = await adminClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}/payments");
        var payments = await ReadAsAsync<IReadOnlyCollection<PaymentRecordTestResponse>>(listResponse);

        paymentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        payments.Should().ContainSingle(payment => payment.Amount == 100m);
    }

    [Fact]
    public async Task UpdateInvoice_ShouldReplaceItemsAndRecalculateTotals()
    {
        var owner = await TestAuthHelper.RegisterAndLoginAsync(Client);
        using var ownerClient = CreateAuthenticatedClient(owner);
        var setup = await CreateProjectSetupAsync(ownerClient);
        var invoice = await TestBillingHelper.CreateInvoiceAsync(
            ownerClient,
            setup.WorkspaceId,
            setup.ClientId,
            setup.ProjectId);

        var updateResponse = await ownerClient.PutAsJsonAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}",
            new
            {
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30),
                Currency = "EUR",
                Notes = "Updated invoice.",
                Items = new[]
                {
                    new
                    {
                        Description = "Updated item",
                        Quantity = 3m,
                        UnitPrice = 80m,
                        TaxRate = 10m
                    }
                }
            });
        var detailResponse = await ownerClient.GetAsync(
            $"/api/workspaces/{setup.WorkspaceId}/invoices/{invoice.InvoiceId}");
        var detail = await ReadAsAsync<InvoiceTestResponse>(detailResponse);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detail.Currency.Should().Be("EUR");
        detail.SubtotalAmount.Should().Be(240m);
        detail.TaxAmount.Should().Be(24m);
        detail.TotalAmount.Should().Be(264m);
    }

    private static object PaymentRequest(decimal amount)
    {
        return new
        {
            Amount = amount,
            Method = "BankTransfer",
            Reference = $"TRX-{Guid.NewGuid():N}",
            PaidAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }

    private static async Task<BillingSetup> CreateProjectSetupAsync(HttpClient ownerClient)
    {
        var workspace = await TestWorkspaceHelper.CreateWorkspaceAsync(ownerClient);
        var client = await TestClientHelper.CreateClientAsync(ownerClient, workspace.WorkspaceId);
        var project = await TestProjectHelper.CreateProjectAsync(
            ownerClient,
            workspace.WorkspaceId,
            client.ClientId);

        return new BillingSetup(
            workspace.WorkspaceId,
            client.ClientId,
            project.ProjectId);
    }

    private sealed record BillingSetup(
        Guid WorkspaceId,
        Guid ClientId,
        Guid ProjectId);

    private sealed record InvoiceTestResponse(
        Guid Id,
        string Status,
        string Currency,
        decimal SubtotalAmount,
        decimal TaxAmount,
        decimal TotalAmount,
        decimal BalanceDue);

    private sealed record PaymentTestResponse(
        decimal InvoicePaidAmount,
        decimal InvoiceBalanceDue,
        string InvoiceStatus);

    private sealed record PagedResult<T>(
        IReadOnlyCollection<T> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record InvoiceListItem(Guid Id);

    private sealed record PaymentRecordTestResponse(
        Guid Id,
        decimal Amount);
}
