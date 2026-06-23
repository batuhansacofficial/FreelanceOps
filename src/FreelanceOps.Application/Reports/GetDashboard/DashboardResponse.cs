using FreelanceOps.Domain.Billing;

namespace FreelanceOps.Application.Reports.GetDashboard;

public sealed record DashboardResponse(
    int TotalClients,
    int ActiveProjects,
    int CompletedProjects,
    int TrackedMinutesThisMonth,
    double TrackedHoursThisMonth,
    decimal PaidRevenueThisMonth,
    IReadOnlyCollection<CurrencyAmountResponse> PaidRevenueThisMonthByCurrency,
    decimal OutstandingInvoiceAmount,
    int OverdueInvoiceCount,
    int OpenInvoiceCount,
    IReadOnlyCollection<RecentInvoiceResponse> RecentInvoices,
    IReadOnlyCollection<TopClientRevenueResponse> TopClientsByRevenue);

public sealed record RecentInvoiceResponse(
    Guid Id,
    string InvoiceNumber,
    Guid ClientId,
    string ClientName,
    InvoiceStatus Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    bool IsOverdue);

public sealed record TopClientRevenueResponse(
    Guid ClientId,
    string ClientName,
    string Currency,
    decimal PaidRevenue);
